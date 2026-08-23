using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MiniGameBase;

public class minigame_1_1_remake_DEBUG : MiniGameBase
{
    protected override float TimerDuration => 15f;
    protected override string MinigameTitle => "범인 찾기";

    protected override string MinigameExplain => "죄수가 순서대로 나타나 타이밍을 알려줍니다.";

    // 1-1은 CSV의 Input을 쓰지만,
    // RhythmManager의 Perfect/Good/Miss 판정 점수는 사용하지 않는다.
    protected override bool UseRhythmJudgementScore => false;

    // 총 대상 수는 고정 12가 아니라 실제 Show 성공 개수로 Finalize 때 세팅한다.
    protected override int ManualTotalNodeCount => -1;

    private IRhythmManager rhythmManager;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    [Header("Enemies (0~3 정답 순서)")]
    public enemy_1_1_test[] enemies;

    [Header("Show Positions")]
    public Transform[] showPositions;

    [Header("Scope")]
    public Transform targetScope;
    public GameObject Scope;

    [Header("Auto Off Time")]
    public float autoOffSeconds = 2.5f;

    [Header("Input Window")]
    [SerializeField] private float inputWindowSeconds = 1f;

    private const int ENEMY_COUNT = 4;
    private const int TOTAL_ROUND = 3;
    private const int EXPECTED_TARGET_COUNT = ENEMY_COUNT * TOTAL_ROUND;

    private int round;
    private int showIndex;
    private int inputIndex;

    private bool canClick;
    private bool ended;
    private bool roundsCompleted;

    private int shownTargetCount;
    private int localSuccessCount;
    private int localAutoFailCount;

    private int[] shuffledPosIndex;
    private bool[] resolvedThisRound;
    private Coroutine[] autoOffJobs;

    private string lastRhythmAction = null;
    private Coroutine inputWindowJob = null;
    private readonly Queue<int> pendingInputs = new Queue<int>();

    private Camera cam;

    private void Log(string msg)
    {
        if (!debugLog) return;
        Debug.Log(msg);
    }

    void Start()
    {
        if (Scope != null)
            Scope.SetActive(false);

        cam = Camera.main;

        // 기존 흐름 유지.
        // UIManager가 StartGame을 따로 호출하는 구조라면 중복 초기화될 수 있지만,
        // 이 코드는 기존 1-1 동작을 최대한 유지하기 위해 그대로 둔다.
        StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        roundsCompleted = false;
        canClick = false;

        round = 0;
        showIndex = 0;
        inputIndex = 0;

        shownTargetCount = 0;
        localSuccessCount = 0;
        localAutoFailCount = 0;

        resolvedThisRound = new bool[ENEMY_COUNT];
        autoOffJobs = new Coroutine[ENEMY_COUNT];

        lastRhythmAction = null;
        pendingInputs.Clear();

        if (Scope != null)
            Scope.SetActive(false);

        PrepareShowPositions();
        ResetRoundObjects();

        Log("[1-1] StartGame");
    }

    public override void BindRhythmManager(IRhythmManager manager)
    {
        if (rhythmManager != null)
            rhythmManager.OnEventTriggered -= OnRhythmEvent;

        rhythmManager = manager;

        if (rhythmManager != null)
        {
            rhythmManager.OnEventTriggered += OnRhythmEvent;
            Log("[1-1] RhythmManager bound");
        }
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended || string.IsNullOrEmpty(action)) return;

        action = action.Trim();

        // End는 라운드 완료 이후에도 받아야 한다.
        if (action == "End")
        {
            Log("[1-1] End event received");
            return;
        }

        if (roundsCompleted)
            return;

        base.OnRhythmEvent(action);

        if (lastRhythmAction == "Show" && action == "Input")
        {
            if (Scope != null)
                Scope.SetActive(true);
        }
        else if (lastRhythmAction == "Input" && action == "Show")
        {
            if (Scope != null)
                Scope.SetActive(false);
        }

        lastRhythmAction = action;

        if (action == "Show")
            HandleShow();
        else if (action == "Input")
            HandleInput();
    }

    void Update()
    {
        if (ended) return;
        if (roundsCompleted) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (cam == null) return;

        Vector3 w = cam.ScreenToWorldPoint(Input.mousePosition);
        w.z = 0;

        if (targetScope != null)
            targetScope.position = w;

        Collider2D col = Physics2D.OverlapPoint(w);

        if (col == null)
        {
            // 빈 곳 클릭은 대상 하나를 놓친 게 아니라 오입력이므로
            // ReportManualFail()은 호출하지 않는다.
            return;
        }

        var clicked = col.GetComponent<enemy_1_1_test>();

        if (clicked == null)
        {
            // 적이 아닌 것 클릭도 오입력.
            return;
        }

        if (!canClick || pendingInputs.Count == 0)
        {
            // 입력 창 밖 클릭도 오입력.
            return;
        }

        int expected = pendingInputs.Peek();

        if (!IsValidEnemyIndex(expected))
        {
            pendingInputs.Dequeue();
            return;
        }

        if (clicked != enemies[expected])
        {
            // 틀린 적 클릭도 오입력.
            // 실제 대상은 아직 살아있으므로 AutoOff에서 실패 처리된다.
            return;
        }

        pendingInputs.Dequeue();
        ResolveSuccess(expected);
    }

    void HandleShow()
    {
        if (showIndex >= ENEMY_COUNT) return;
        if (!IsValidEnemyIndex(showIndex)) return;
        if (showPositions == null || showPositions.Length < ENEMY_COUNT) return;
        if (shuffledPosIndex == null || shuffledPosIndex.Length < ENEMY_COUNT) return;

        int posIdx = shuffledPosIndex[showIndex];

        if (posIdx < 0 || posIdx >= showPositions.Length) return;
        if (showPositions[posIdx] == null) return;

        var e = enemies[showIndex];
        var p = showPositions[posIdx];

        if (e == null) return;

        e.transform.position = p.position;

        EnemySetActiveTrueThenShow(e);

        // 실제로 Show에 성공한 대상만 총점 후보로 센다.
        shownTargetCount++;

        StopAutoOff(showIndex);
        autoOffJobs[showIndex] = StartCoroutine(AutoOffRoutine(showIndex));

        Log($"[1-1] SHOW idx={showIndex}, shownTargetCount={shownTargetCount}");

        showIndex++;
    }

    void HandleInput()
    {
        if (canClick)
            CloseInputWindow();

        pendingInputs.Clear();

        for (int i = inputIndex; i < showIndex; i++)
        {
            if (IsValidEnemyIndex(i) && !resolvedThisRound[i] && enemies[i] != null)
            {
                pendingInputs.Enqueue(i);
                break;
            }
        }

        if (pendingInputs.Count == 0)
            return;

        canClick = true;

        if (inputWindowJob != null)
            StopCoroutine(inputWindowJob);

        inputWindowJob = StartCoroutine(InputWindowRoutine());

        Log($"[1-1] INPUT expected={pendingInputs.Peek()} inputIndex={inputIndex} showIndex={showIndex}");
    }

    IEnumerator InputWindowRoutine()
    {
        yield return new WaitForSeconds(inputWindowSeconds);

        canClick = false;
        inputWindowJob = null;
    }

    IEnumerator AutoOffRoutine(int idx)
    {
        yield return new WaitForSeconds(autoOffSeconds);

        if (!IsValidEnemyIndex(idx)) yield break;
        if (resolvedThisRound[idx]) yield break;

        resolvedThisRound[idx] = true;

        localAutoFailCount++;
        ReportManualFail();

        yield return EnemyHideThenSetActiveFalse(enemies[idx]);

        Log($"[1-1] AUTO FAIL idx={idx}, localAutoFailCount={localAutoFailCount}");

        if (idx == inputIndex)
        {
            inputIndex++;
            TryEndRound();
        }
    }

    void ResolveSuccess(int idx)
    {
        if (!IsValidEnemyIndex(idx)) return;
        if (resolvedThisRound[idx]) return;

        StartCoroutine(ResolveSuccessRoutine(idx));
    }

    IEnumerator ResolveSuccessRoutine(int idx)
    {
        resolvedThisRound[idx] = true;

        localSuccessCount++;
        ReportManualSuccess();

        StopAutoOff(idx);
        CloseInputWindow();

        yield return EnemySuccessHideThenSetActiveFalse(enemies[idx]);

        Log($"[1-1] SUCCESS idx={idx}, localSuccessCount={localSuccessCount}");

        inputIndex++;
        TryEndRound();
    }

    IEnumerator EnemySuccessHideThenSetActiveFalse(enemy_1_1_test e)
    {
        if (e == null) yield break;

        FadeActiveToggle fade = e.TryGetFade();

        e.Clear();

        if (fade != null)
            yield return new WaitForSeconds(fade.GetFadeTime());

        e.ResetEnemy();
        e.gameObject.SetActive(false);
    }

    void CloseInputWindow()
    {
        canClick = false;
        pendingInputs.Clear();

        if (inputWindowJob != null)
        {
            StopCoroutine(inputWindowJob);
            inputWindowJob = null;
        }
    }

    void TryEndRound()
    {
        if (inputIndex < ENEMY_COUNT) return;

        EndRound();

        if (Scope != null)
            Scope.SetActive(false);

        if (targetScope != null)
            targetScope.position = Vector2.zero;
    }

    void EndRound()
    {
        round++;

        canClick = false;
        showIndex = 0;
        inputIndex = 0;
        pendingInputs.Clear();

        if (inputWindowJob != null)
        {
            StopCoroutine(inputWindowJob);
            inputWindowJob = null;
        }

        if (round >= TOTAL_ROUND)
        {
            roundsCompleted = true;

            if (Scope != null)
                Scope.SetActive(false);

            Log(
                $"[1-1] ROUNDS COMPLETED " +
                $"shown={shownTargetCount}, success={localSuccessCount}, autoFail={localAutoFailCount}"
            );

            return;
        }

        PrepareShowPositions();
        ResetRoundObjects();
    }

    void ResetRoundObjects()
    {
        if (resolvedThisRound == null || resolvedThisRound.Length < ENEMY_COUNT)
            resolvedThisRound = new bool[ENEMY_COUNT];

        if (autoOffJobs == null || autoOffJobs.Length < ENEMY_COUNT)
            autoOffJobs = new Coroutine[ENEMY_COUNT];

        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            resolvedThisRound[i] = false;

            StopAutoOff(i);

            if (!IsValidEnemyIndex(i)) continue;
            if (enemies[i] == null) continue;

            enemies[i].ResetEnemy();
            EnemySetInactiveImmediate(enemies[i]);
        }
    }

    void StopAutoOff(int idx)
    {
        if (autoOffJobs == null) return;
        if (idx < 0 || idx >= autoOffJobs.Length) return;

        if (autoOffJobs[idx] != null)
        {
            StopCoroutine(autoOffJobs[idx]);
            autoOffJobs[idx] = null;
        }
    }

    void PrepareShowPositions()
    {
        shuffledPosIndex = new int[ENEMY_COUNT];

        for (int i = 0; i < ENEMY_COUNT; i++)
            shuffledPosIndex[i] = i;

        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            int r = Random.Range(i, ENEMY_COUNT);
            (shuffledPosIndex[i], shuffledPosIndex[r]) = (shuffledPosIndex[r], shuffledPosIndex[i]);
        }
    }

    public override ScoreResult FinalizeScoreSession()
    {
        int processedCount = localSuccessCount + localAutoFailCount;
        int runtimeTotal = Mathf.Max(shownTargetCount, processedCount);

        SetRuntimeTotalNodeCount(runtimeTotal);

        if (runtimeTotal != EXPECTED_TARGET_COUNT)
        {
            Debug.LogWarning(
                $"[1-1] Expected {EXPECTED_TARGET_COUNT} targets, but runtime total is {runtimeTotal}. " +
                $"shown={shownTargetCount}, processed={processedCount}"
            );
        }

        return base.FinalizeScoreSession();
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // 1-1은 RhythmManager의 Perfect/Good/Miss 판정 점수를 사용하지 않는다.
        // CSV Input은 클릭 가능 창을 여는 트리거로만 사용한다.
    }

    public void Succeed()
    {
        if (ended) return;

        ended = true;
        Success();
    }

    public void Failure()
    {
        if (ended) return;

        ended = true;
        Fail();
    }

    void EnemySetActiveTrueThenShow(enemy_1_1_test e)
    {
        if (e == null) return;

        GameObject go = e.gameObject;
        go.SetActive(true);

        FadeActiveToggle fade = e.TryGetFade();

        if (fade != null)
        {
            fade.SetAlphaImmediate(fade.inactiveAlpha);
            fade.FadeIn();
        }
    }

    IEnumerator EnemyHideThenSetActiveFalse(enemy_1_1_test e)
    {
        if (e == null) yield break;

        FadeActiveToggle fade = e.TryGetFade();

        if (fade != null)
        {
            fade.FadeOut();
            yield return new WaitForSeconds(fade.GetFadeTime());
        }

        e.ResetEnemy();
        e.gameObject.SetActive(false);
    }

    void EnemySetInactiveImmediate(enemy_1_1_test e)
    {
        if (e == null) return;

        FadeActiveToggle fade = e.TryGetFade();

        if (fade != null)
            fade.SetAlphaImmediate(fade.inactiveAlpha);

        e.gameObject.SetActive(false);
    }

    private bool IsValidEnemyIndex(int idx)
    {
        return enemies != null && idx >= 0 && idx < enemies.Length && idx < ENEMY_COUNT;
    }

    public override void ExecutePracticeAction(
    int actionIndex,
    string actionType)
    {
        if (ended ||
            roundsCompleted)
        {
            return;
        }

        /*
         * 1-1에서 플레이어 행동은
         * CSV의 Input 시점에만 발생한다.
         */
        if (!string.Equals(
                actionType,
                "Input",
                System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        /*
         * OnRhythmEvent("Input")에서
         * HandleInput()이 먼저 실행되어
         * pendingInputs에 현재 정답이 들어간다.
         */
        if (!canClick ||
            pendingInputs.Count == 0)
        {
            Log(
                $"[1-1 Demo] 실행할 대상 없음 " +
                $"actionIndex={actionIndex}"
            );

            return;
        }

        int expected =
            pendingInputs.Peek();

        if (!IsValidEnemyIndex(expected))
        {
            pendingInputs.Dequeue();
            return;
        }

        enemy_1_1_test target =
            enemies[expected];

        if (target == null)
            return;

        /*
         * 실제 플레이어가 클릭하면
         * targetScope가 클릭 위치로 움직이므로
         * 예시보기에서도 같은 시각적 반응을 준다.
         */
        if (targetScope != null)
        {
            targetScope.position =
                target.transform.position;
        }

        /*
         * 실제 클릭 성공과 동일하게
         * 현재 정답 입력을 소비한다.
         */
        pendingInputs.Dequeue();

        ResolveSuccess(expected);

        Log(
            $"[1-1 Demo] 자동 입력 성공 " +
            $"actionIndex={actionIndex}, " +
            $"enemy={expected}"
        );
    }
}