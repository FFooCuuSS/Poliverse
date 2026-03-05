using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MiniGameBase;

public class minigame_1_1_remake : MiniGameBase
{
    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "분류해라!";

    private IRhythmManager rhythmManager;

    [Header("Enemies (0~3 정답 순서)")]
    public enemy_1_1_test[] enemies;

    [Header("Show Positions (4개)")]
    public Transform[] showPositions;

    [Header("Target Scope")]
    public Transform targetScope;
    [Header("Scope Sprite")]
    public GameObject Scope;

    [Header("Auto Off Time (seconds)")]
    public float autoOffSeconds = 2.5f;

    [Header("Input Window")]
    [SerializeField] private float inputWindowSeconds = 1f;

    private const int ENEMY_COUNT = 4;
    private const int TOTAL_ROUND = 3;

    private int round;
    private int showIndex;
    private int inputIndex;

    private bool canClick;
    private bool ended;

    private int successCount;
    private int missCount;

    private int[] shuffledPosIndex;
    private bool[] resolvedThisRound;
    private Coroutine[] autoOffJobs;

    private string lastRhythmAction = null;
    private Coroutine inputWindowJob = null;
    private readonly Queue<int> pendingInputs = new Queue<int>();

    private Camera cam;

    void Start()
    {
        Scope.SetActive(false);
        cam = Camera.main;
        StartGame();
        /*if (Scope != null) Scope.SetActive(false);
        cam = Camera.main;
        StartGame();
         */
    }

    public override void StartGame()
    {
        ended = false;
        canClick = false;

        round = 0;
        showIndex = 0;
        inputIndex = 0;

        successCount = 0;
        missCount = 0;

        if (resolvedThisRound == null || resolvedThisRound.Length != ENEMY_COUNT)
            resolvedThisRound = new bool[ENEMY_COUNT];

        if (autoOffJobs == null || autoOffJobs.Length != ENEMY_COUNT)
            autoOffJobs = new Coroutine[ENEMY_COUNT];

        lastRhythmAction = null;
        if (Scope != null) Scope.SetActive(false);

        PrepareShowPositions();
        ResetRoundObjects();
    }

    public override void BindRhythmManager(IRhythmManager manager)
    {
        if (rhythmManager != null)
            rhythmManager.OnEventTriggered -= OnRhythmEvent;

        rhythmManager = manager;

        if (rhythmManager != null)
            rhythmManager.OnEventTriggered += OnRhythmEvent;
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended || string.IsNullOrEmpty(action)) return;

        action = action.Trim();

        if (lastRhythmAction == "Show" && action == "Input")
        {
            if (Scope != null) Scope.SetActive(true);
        }
        else if (lastRhythmAction == "Input" && action == "Show")
        {
            if (Scope != null) Scope.SetActive(false);
        }

        lastRhythmAction = action;

        if (action == "Show") HandleShow();
        else if (action == "Input") HandleInput();
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        Debug.Log($"{judgement}");
    }

    void Update()
    {
        if (ended) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (cam == null) return;

        Vector3 w = cam.ScreenToWorldPoint(Input.mousePosition);
        w.z = 0;

        if (targetScope != null)
            targetScope.position = w;

        RaycastHit2D hit = Physics2D.Raycast(w, Vector2.zero);

        if (!hit)
        {
            if (canClick)
            {
                missCount++;
                Debug.Log("[1-1] MISS: Clicked Empty (no collider)");
            }
            return;
        }

        var clicked = hit.collider.GetComponent<enemy_1_1_test>();

        if (clicked == null)
        {
            if (canClick)
            {
                missCount++;
                Debug.Log("[1-1] MISS: Clicked Non-Enemy");
            }
            return;
        }

        if (!canClick)
        {
            missCount++;
            Debug.Log("[1-1] MISS: Clicked Enemy Outside Input Time");
            return;
        }

        if (!canClick || pendingInputs.Count == 0)
        {
            missCount++;
            Debug.Log("[1-1] MISS: Clicked Enemy Outside Input Time");
            return;
        }

        int expected = pendingInputs.Peek();

        if (resolvedThisRound[expected])
        {
            pendingInputs.Dequeue();
            return;
        }

        if (clicked != enemies[expected])
        {
            missCount++;
            Debug.Log("[1-1] MISS: Wrong Enemy (expected=" + expected + ")");
            return;
        }

        ResolveSuccess(expected);
        pendingInputs.Dequeue();

        while (inputIndex < ENEMY_COUNT && resolvedThisRound[inputIndex])
            inputIndex++;

        TryEndRound();
    }

    void HandleShow()
    {
        Debug.Log("[1-1] HandleShow entered showIndex=" + showIndex);

        if (showIndex >= ENEMY_COUNT)
        {
            Debug.Log("[1-1] HandleShow return: showIndex>=ENEMY_COUNT");
            return;
        }

        if (enemies == null)
        {
            Debug.Log("[1-1] HandleShow return: enemies is null");
            return;
        }

        if (enemies.Length < ENEMY_COUNT)
        {
            Debug.Log("[1-1] HandleShow return: enemies length=" + enemies.Length);
            return;
        }

        if (showPositions == null)
        {
            Debug.Log("[1-1] HandleShow return: showPositions is null");
            return;
        }

        if (showPositions.Length < ENEMY_COUNT)
        {
            Debug.Log("[1-1] HandleShow return: showPositions length=" + showPositions.Length);
            return;
        }

        if (shuffledPosIndex == null || shuffledPosIndex.Length != ENEMY_COUNT)
        {
            Debug.Log("[1-1] HandleShow: PrepareShowPositions()");
            PrepareShowPositions();
        }

        int posIdx = shuffledPosIndex[showIndex];

        var e = enemies[showIndex];
        var p = showPositions[posIdx];

        if (e == null)
        {
            Debug.Log("[1-1] HandleShow return: enemies[" + showIndex + "] is null");
            return;
        }

        if (p == null)
        {
            Debug.Log("[1-1] HandleShow return: showPositions[" + posIdx + "] is null");
            return;
        }

        e.transform.position = p.position;

        // 여기서부터 네가 원하는 순서
        EnemySetActiveTrueThenShow(e);

        StopAutoOff(showIndex);

        //resolvedThisRound[showIndex] = false;
        autoOffJobs[showIndex] = StartCoroutine(AutoOffRoutine(showIndex));

        showIndex++;
    }

    void HandleInput()
    {
        if (canClick)
            CloseInputWindow();

        pendingInputs.Clear();

        // 핵심: showIndex까지만, 그리고 아직 미해결인 첫 대상
        for (int i = inputIndex; i < showIndex; i++)
        {
            if (!resolvedThisRound[i] && enemies[i] != null)
            {
                pendingInputs.Enqueue(i);
                break;
            }
        }

        if (pendingInputs.Count == 0)
        {
            Debug.LogWarning($"[1-1] INPUT IGNORED: empty queue. inputIndex={inputIndex}, showIndex={showIndex}, " +
                             $"resolved=[{string.Join(",", resolvedThisRound)}]");
            return;
        }

        canClick = true;

        if (inputWindowJob != null) StopCoroutine(inputWindowJob);
        inputWindowJob = StartCoroutine(InputWindowRoutine());

        Debug.Log($"[1-1] INPUT START expected={pendingInputs.Peek()} inputIndex={inputIndex} showIndex={showIndex}");
    }

    IEnumerator InputWindowRoutine()
    {
        yield return new WaitForSeconds(inputWindowSeconds);

        if (ended) yield break;

        canClick = false;
        inputWindowJob = null;
    }

    IEnumerator AutoOffRoutine(int idx)
    {
        yield return new WaitForSeconds(autoOffSeconds);

        if (ended) yield break;
        if (resolvedThisRound[idx]) yield break;

        resolvedThisRound[idx] = true;
        missCount++;

        if (enemies[idx] != null)
        {
            // 원하는 흐름: FadeOut(Hide) -> SetActive(false)
            yield return EnemyHideThenSetActiveFalse(enemies[idx]);
        }

        Debug.Log("[1-1] MISS: AutoOff Timeout idx=" + idx);

        if (idx == inputIndex)
        {
            inputIndex++;
            TryEndRound();
        }
    }

    void ResolveSuccess(int idx)
    {
        if (ended) return;
        if (resolvedThisRound[idx]) return;

        resolvedThisRound[idx] = true;
        successCount++;

        StopAutoOff(idx);

        if (enemies[idx] != null)
            StartCoroutine(EnemyHideThenSetActiveFalse(enemies[idx]));

        Debug.Log("[1-1] SUCCESS: idx=" + idx);

        CloseInputWindow();

        inputIndex++;
        TryEndRound();
    }
    private void CloseInputWindow()
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

        if (Scope != null) Scope.SetActive(false);
        if (targetScope != null) targetScope.position = new Vector2(0, 0);
    }

    void EndRound()
    {
        round++;

        if (round >= TOTAL_ROUND)
        {
            Debug.Log("[1-1] FINAL RESULT success=" + successCount + " miss=" + missCount);

            if (successCount >= 7) Succeed();
            else Failure();
            return;
        }

        canClick = false;
        showIndex = 0;
        inputIndex = 0;

        PrepareShowPositions();
        ResetRoundObjects();
        if (inputWindowJob != null) { StopCoroutine(inputWindowJob); inputWindowJob = null; }
        canClick = false;
    }

    void ResetRoundObjects()
    {
        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            resolvedThisRound[i] = false;
            StopAutoOff(i);

            if (enemies != null && i < enemies.Length && enemies[i] != null)
            {
                enemies[i].ResetEnemy();

                // 라운드 리셋은 즉시 정리: 알파를 낮춰두고 비활성화
                EnemySetInactiveImmediate(enemies[i]);
            }
        }
        canClick = false;
        if (inputWindowJob != null) { StopCoroutine(inputWindowJob); inputWindowJob = null; }
    }

    void StopAutoOff(int idx)
    {
        if (autoOffJobs[idx] != null)
        {
            StopCoroutine(autoOffJobs[idx]);
            autoOffJobs[idx] = null;
        }
    }

    void PrepareShowPositions()
    {
        if (shuffledPosIndex == null || shuffledPosIndex.Length != ENEMY_COUNT)
            shuffledPosIndex = new int[ENEMY_COUNT];

        for (int i = 0; i < ENEMY_COUNT; i++)
            shuffledPosIndex[i] = i;

        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            int r = Random.Range(i, ENEMY_COUNT);
            (shuffledPosIndex[i], shuffledPosIndex[r]) = (shuffledPosIndex[r], shuffledPosIndex[i]);
        }
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

    // 아래부터 enemy 표시/숨김 유틸(요구한 순서 그대로 구현)

    void EnemySetActiveTrueThenShow(enemy_1_1_test e)
    {
        if (e == null) return;

        GameObject go = e.gameObject;

        // 먼저 켜기
        go.SetActive(true);

        // 알파를 낮은 값으로 강제로 맞춘 후 페이드 인 시작(처음 한 프레임 번쩍임 방지)
        FadeActiveToggle fade = go.GetComponent<FadeActiveToggle>();
        if (fade != null)
        {
            fade.SetAlphaImmediate(fade.inactiveAlpha);
            fade.FadeIn();
        }
    }

    IEnumerator EnemyHideThenSetActiveFalse(enemy_1_1_test e)
    {
        if (e == null) yield break;

        GameObject go = e.gameObject;

        FadeActiveToggle fade = go.GetComponent<FadeActiveToggle>();
        if (fade != null)
        {
            fade.FadeOut();
            yield return new WaitForSeconds(fade.GetFadeTime());
        }

        // 마지막에 끄기
        go.SetActive(false);
    }

    void EnemySetInactiveImmediate(enemy_1_1_test e)
    {
        if (e == null) return;

        GameObject go = e.gameObject;

        FadeActiveToggle fade = go.GetComponent<FadeActiveToggle>();
        if (fade != null)
        {
            // 비활성화 상태에서도 다음 Show 때 자연스럽게 시작되도록 알파를 낮게 맞춰둠
            fade.SetAlphaImmediate(fade.inactiveAlpha);
        }

        go.SetActive(false);
    }
}