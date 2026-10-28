using System.Collections;
using UnityEngine;

public class minigame_1_1_remake_DEBUG : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 15f;
    protected override string MinigameTitle => "범인 찾기";

    protected override string MinigameExplain => "죄수가 순서대로 나타나 타이밍을 알려줍니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "배경이 어두워지면 타이밍에 맞게 죄수를 터치해주세요."
    };

    // 1-1은 CSV의 Show/Input 타이밍만 쓰고,
    // 점수는 ReportManualSuccess/Fail로 직접 보고한다.
    // (RhythmManager의 CheckMiss 로그는 1-1 점수와 무관하다)
    protected override bool UseRhythmJudgementScore => false;
    protected override int ManualTotalNodeCount => -1;

    private IRhythmManager rhythmManager;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;
    [Tooltip("판정마다 diff(ms)를 출력한다. 확인 끝나면 끄기.")]
    [SerializeField] private bool judgeLog = true;

    [Header("Enemies (0~3 정답 순서)")]
    public enemy_1_1_test[] enemies;

    [Header("Show Positions")]
    public Transform[] showPositions;

    [Header("Scope")]
    public Transform targetScope;
    public GameObject Scope;

    [Header("Judge Window (seconds, Input 노트 기준)")]
    [Tooltip("Input 노트보다 이만큼 먼저 눌러도 성공")]
    [SerializeField] private float earlyWindowSeconds = 0.15f;
    [Tooltip("Input 노트보다 이만큼 늦게 눌러도 성공. 지나면 실패")]
    [SerializeField] private float lateWindowSeconds = 0.25f;

    private const int ENEMY_COUNT = 4;
    private const int TOTAL_ROUND = 3;
    private const int EXPECTED_TARGET_COUNT = ENEMY_COUNT * TOTAL_ROUND;

    private enum TargetState { Hidden, Shown, Windowed, Resolved }

    private int round;
    private int showIndex;      // 이번 라운드에서 다음에 Show될 적
    private int inputCursor;    // 이번 라운드에서 다음 Input 노트가 가리킬 적
    private int resolvedCount;

    private bool ended;
    private bool roundsCompleted;

    private int shownTargetCount;
    private int localSuccessCount;
    private int localFailCount;

    private int[] shuffledPosIndex;
    private TargetState[] states;
    private double[] windowOpenTime;   // Input 노트가 발생한 시각(≈노트 시각)
    private Coroutine[] hideJobs;

    // 노트보다 살짝 먼저 누른 입력 버퍼
    private int earlyClickIdx = -1;
    private double earlyClickTime;

    private string lastRhythmAction = null;
    private Camera cam;
    private bool isDemoMode = false;

    // ───────── 시간 ─────────
    // 판정은 RhythmManager와 같은 dsp 기반 SongTime으로 잰다.
    private double Now
    {
        get
        {
            if (rhythmManager is RhythmManager rm && rm.IsRunning)
                return rm.SongTime;

            return Time.timeAsDouble;
        }
    }

    private void Log(string msg)
    {
        if (!debugLog) return;
        Debug.Log(msg);
    }

    private void JudgeLog(string msg)
    {
        if (!judgeLog) return;
        Debug.Log(msg);
    }

    void Start()
    {
        if (Scope != null)
            Scope.SetActive(false);

        cam = Camera.main;

        StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        roundsCompleted = false;

        round = 0;
        shownTargetCount = 0;
        localSuccessCount = 0;
        localFailCount = 0;

        states = new TargetState[ENEMY_COUNT];
        windowOpenTime = new double[ENEMY_COUNT];

        if (hideJobs != null)
        {
            for (int i = 0; i < hideJobs.Length; i++)
                StopHideJob(i);
        }
        hideJobs = new Coroutine[ENEMY_COUNT];

        lastRhythmAction = null;

        if (Scope != null)
            Scope.SetActive(false);

        BeginRound();

        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            if (!IsValidEnemyIndex(i) || enemies[i] == null) continue;

            enemies[i].ResetEnemy();
            EnemySetInactiveImmediate(enemies[i]);
        }

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

        if (action == "End")
            return;

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
        if (ended || roundsCompleted) return;

        CheckExpiredWindows();

        if (isDemoMode) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (cam == null) return;

        double t = Now;

        Vector3 w = cam.ScreenToWorldPoint(Input.mousePosition);
        w.z = 0;

        if (targetScope != null)
            targetScope.position = w;

        int clickedIdx = FindClickedEnemy(w);

        if (clickedIdx < 0)
        {
            JudgeLog($"[1-1 Judge] t={t:F3} 빈 곳/적 아님 클릭 (무시)");
            return;
        }

        TargetState s = states[clickedIdx];

        // 1) 판정창이 열린 적 → diff로 판정
        if (s == TargetState.Windowed)
        {
            double diff = t - windowOpenTime[clickedIdx];
            JudgeLog($"[1-1 Judge] HIT enemy={clickedIdx} diff={diff * 1000:F0}ms (late 허용 {lateWindowSeconds * 1000:F0}ms)");
            ResolveSuccess(clickedIdx);
            return;
        }

        // 2) 아직 Input 노트 전인데 다음 차례 적을 누름 → early 버퍼
        if (s == TargetState.Shown && clickedIdx == inputCursor)
        {
            earlyClickIdx = clickedIdx;
            earlyClickTime = t;
            JudgeLog($"[1-1 Judge] EARLY 버퍼 enemy={clickedIdx} t={t:F3} (Input 노트 대기)");
            return;
        }

        JudgeLog($"[1-1 Judge] 무시 enemy={clickedIdx} state={s} 다음차례={inputCursor} t={t:F3}");
    }

    private int FindClickedEnemy(Vector2 worldPos)
    {
        // 조준경 등 다른 콜라이더가 겹쳐도 적을 찾도록 전부 검사
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);

        for (int h = 0; h < hits.Length; h++)
        {
            var e = hits[h].GetComponentInParent<enemy_1_1_test>();
            if (e == null) continue;

            for (int i = 0; i < ENEMY_COUNT; i++)
            {
                if (IsValidEnemyIndex(i) && enemies[i] == e)
                    return i;
            }
        }

        return -1;
    }

    void HandleShow()
    {
        // 이전 라운드가 아직 안 끝났는데 다음 Show가 오면 남은 대상 실패 처리 후 라운드 넘김
        if (showIndex >= ENEMY_COUNT)
        {
            Debug.LogWarning($"[1-1] Show가 왔지만 라운드 {round} 미종료 → 강제 종료 (resolved={resolvedCount})");
            ForceFinishRound();

            if (roundsCompleted) return;
        }

        if (!IsValidEnemyIndex(showIndex)) return;
        if (showPositions == null || showPositions.Length < ENEMY_COUNT) return;
        if (shuffledPosIndex == null || shuffledPosIndex.Length < ENEMY_COUNT) return;

        int posIdx = shuffledPosIndex[showIndex];

        if (posIdx < 0 || posIdx >= showPositions.Length) return;
        if (showPositions[posIdx] == null) return;

        var e = enemies[showIndex];
        if (e == null) return;

        // 이전 라운드에서 같은 적의 페이드아웃이 남아 있으면 끊는다
        StopHideJob(showIndex);
        e.ResetEnemy();

        e.transform.position = showPositions[posIdx].position;
        EnemySetActiveTrueThenShow(e);

        states[showIndex] = TargetState.Shown;
        shownTargetCount++;

        Log($"[1-1] SHOW round={round} idx={showIndex} t={Now:F3}");

        showIndex++;
    }

    void HandleInput()
    {
        double t = Now;

        if (inputCursor >= ENEMY_COUNT || inputCursor >= showIndex)
        {
            Debug.LogWarning($"[1-1] Input 노트가 왔지만 대상 없음 (cursor={inputCursor}, shown={showIndex}, round={round}) t={t:F3}");
            return;
        }

        int idx = inputCursor;
        inputCursor++;

        if (states[idx] != TargetState.Shown)
        {
            Log($"[1-1] Input target {idx} state={states[idx]} → skip");
            return;
        }

        states[idx] = TargetState.Windowed;
        windowOpenTime[idx] = t;

        JudgeLog($"[1-1 Judge] INPUT 노트 round={round} enemy={idx} t={t:F3}");

        // early 버퍼 확인
        if (earlyClickIdx == idx)
        {
            double diff = earlyClickTime - t; // 음수
            earlyClickIdx = -1;

            if (-diff <= earlyWindowSeconds)
            {
                JudgeLog($"[1-1 Judge] HIT(early) enemy={idx} diff={diff * 1000:F0}ms (early 허용 {earlyWindowSeconds * 1000:F0}ms)");
                ResolveSuccess(idx);
                return;
            }

            JudgeLog($"[1-1 Judge] early 너무 빠름 enemy={idx} diff={diff * 1000:F0}ms → 무효, 창은 유지");
        }
    }

    void CheckExpiredWindows()
    {
        if (states == null) return;

        double t = Now;

        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            if (states[i] != TargetState.Windowed) continue;
            if (t <= windowOpenTime[i] + lateWindowSeconds) continue;

            JudgeLog($"[1-1 Judge] MISS(시간초과) enemy={i} 노트={windowOpenTime[i]:F3} now={t:F3}");
            ResolveFail(i);
        }
    }

    void ResolveSuccess(int idx)
    {
        if (!IsValidEnemyIndex(idx)) return;
        if (states[idx] == TargetState.Resolved) return;

        states[idx] = TargetState.Resolved;

        if (earlyClickIdx == idx)
            earlyClickIdx = -1;

        localSuccessCount++;
        ReportManualSuccess();

        StopHideJob(idx);
        hideJobs[idx] = StartCoroutine(EnemySuccessHideThenSetActiveFalse(enemies[idx]));

        OnTargetResolved();
    }

    void ResolveFail(int idx)
    {
        if (!IsValidEnemyIndex(idx)) return;
        if (states[idx] == TargetState.Resolved) return;

        states[idx] = TargetState.Resolved;

        if (earlyClickIdx == idx)
            earlyClickIdx = -1;

        localFailCount++;
        ReportManualFail();

        StopHideJob(idx);
        hideJobs[idx] = StartCoroutine(EnemyHideThenSetActiveFalse(enemies[idx]));

        OnTargetResolved();
    }

    void OnTargetResolved()
    {
        resolvedCount++;

        if (resolvedCount < ENEMY_COUNT) return;

        // 라운드 종료는 페이드를 기다리지 않고 즉시 (다음 Show를 놓치지 않도록)
        EndRound();
    }

    void ForceFinishRound()
    {
        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            if (states[i] == TargetState.Shown || states[i] == TargetState.Windowed)
                ResolveFail(i);
        }

        // 아예 Show되지 않은 대상이 있으면 resolvedCount가 안 차므로 직접 종료
        if (!roundsCompleted && showIndex >= ENEMY_COUNT && resolvedCount < ENEMY_COUNT)
            EndRound();
    }

    void EndRound()
    {
        round++;

        Log($"[1-1] ROUND END → round={round} success={localSuccessCount} fail={localFailCount}");

        if (Scope != null)
            Scope.SetActive(false);

        if (targetScope != null)
            targetScope.position = Vector2.zero;

        if (round >= TOTAL_ROUND)
        {
            roundsCompleted = true;

            Log(
                $"[1-1] ROUNDS COMPLETED " +
                $"shown={shownTargetCount}, success={localSuccessCount}, fail={localFailCount}"
            );

            return;
        }

        BeginRound();
    }

    void BeginRound()
    {
        showIndex = 0;
        inputCursor = 0;
        resolvedCount = 0;
        earlyClickIdx = -1;

        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            states[i] = TargetState.Hidden;
            windowOpenTime[i] = 0;
        }

        PrepareShowPositions();
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

    void StopHideJob(int idx)
    {
        if (hideJobs == null) return;
        if (idx < 0 || idx >= hideJobs.Length) return;

        if (hideJobs[idx] != null)
        {
            StopCoroutine(hideJobs[idx]);
            hideJobs[idx] = null;
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
        int processedCount = localSuccessCount + localFailCount;
        int runtimeTotal = Mathf.Max(shownTargetCount, processedCount);

        SetRuntimeTotalNodeCount(runtimeTotal);

        if (runtimeTotal != EXPECTED_TARGET_COUNT)
        {
            Debug.LogWarning(
                $"[1-1] Expected {EXPECTED_TARGET_COUNT} targets, but runtime total is {runtimeTotal}. " +
                $"shown={shownTargetCount}, processed={processedCount}"
            );
        }

        JudgeLog($"[1-1 Judge] 결과 success={localSuccessCount} fail={localFailCount} shown={shownTargetCount}");

        return base.FinalizeScoreSession();
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // 1-1은 RhythmManager 판정을 쓰지 않는다.
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

        e.gameObject.SetActive(true);

        FadeActiveToggle fade = e.TryGetFade();

        if (fade != null)
        {
            fade.SetAlphaImmediate(fade.inactiveAlpha);
            fade.FadeIn();
        }
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

    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (ended || roundsCompleted) return;

        if (!string.Equals(actionType, "Input", System.StringComparison.OrdinalIgnoreCase))
            return;

        // OnRhythmEvent("Input")가 먼저 실행되어 방금 노트의 대상이 Windowed 상태다.
        int idx = inputCursor - 1;

        if (!IsValidEnemyIndex(idx) || states[idx] != TargetState.Windowed)
        {
            Log($"[1-1 Demo] 실행할 대상 없음 actionIndex={actionIndex}");
            return;
        }

        enemy_1_1_test target = enemies[idx];
        if (target == null) return;

        if (targetScope != null)
            targetScope.position = target.transform.position;

        ResolveSuccess(idx);

        Log($"[1-1 Demo] 자동 입력 성공 actionIndex={actionIndex}, enemy={idx}");
    }

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        if (isDemoMode)
        {
            earlyClickIdx = -1;

            if (Scope != null)
                Scope.SetActive(false);
        }

        Log($"[1-1] SetDemoMode = {isDemoMode}");
    }
}