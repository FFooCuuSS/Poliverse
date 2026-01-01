using System.Collections;
using UnityEngine;
using static MiniGameBase;

public class minigame_1_1_remake : MiniGameBase
{
    protected override float TimerDuration => 30f;
    protected override string MinigameExplain => "분류해라!";

    private IRhythmManager rhythmManager;

    [Header("Enemies (0~3 정답 순서)")]
    public enemy_1_1_test[] enemies;

    [Header("Show Positions (4개)")]
    public Transform[] showPositions;

    [Header("Target Scope")]
    public Transform targetScope;

    [Header("Auto Off Time (seconds)")]
    public float autoOffSeconds = 2.5f;

    private const int ENEMY_COUNT = 4;
    private const int TOTAL_ROUND = 2;

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

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        StartGame();
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

        PrepareShowPositions();
        ResetRoundObjects();

        Debug.Log("[1-1] StartGame");
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

        if (action == "Show") HandleShow();
        else if (action == "Input") HandleInput();
    }

    void Update()
    {
        if (ended) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (cam == null) return;

        Vector3 w = cam.ScreenToWorldPoint(Input.mousePosition);
        w.z = 0;

        // 스코프 연출
        if (targetScope != null)
            targetScope.position = w;

        // 클릭 위치 레이캐스트
        RaycastHit2D hit = Physics2D.Raycast(w, Vector2.zero);

        // 아무것도 못 찍었으면: 입력 구간일 때만 Miss로 처리
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

        // 적이 아닌 것을 클릭했으면: 입력 구간일 때만 Miss
        if (clicked == null)
        {
            if (canClick)
            {
                missCount++;
                Debug.Log("[1-1] MISS: Clicked Non-Enemy");
            }
            return;
        }

        // miss 판정
        if (!canClick)
        {
            missCount++;
            Debug.Log("[1-1] MISS: Clicked Enemy Outside Input Time");
            return;
        }

        if (inputIndex >= ENEMY_COUNT)
            return;

        // 이미 처리된 입력이면 무시
        if (resolvedThisRound[inputIndex])
            return;

        // 순서가 틀린 클릭 Miss
        if (clicked != enemies[inputIndex])
        {
            missCount++;
            Debug.Log("[1-1] MISS: Wrong Enemy (expected=" + inputIndex + ")");
            return;
        }

        // 정답 성공 처리
        ResolveSuccess(inputIndex);
    }

    void HandleShow()
    {
        if (showIndex >= ENEMY_COUNT) return;
        if (enemies == null || enemies.Length < ENEMY_COUNT) return;
        if (showPositions == null || showPositions.Length < ENEMY_COUNT) return;

        if (shuffledPosIndex == null || shuffledPosIndex.Length != ENEMY_COUNT)
            PrepareShowPositions();

        int posIdx = shuffledPosIndex[showIndex];

        var e = enemies[showIndex];
        var p = showPositions[posIdx];
        if (e == null || p == null) return;

        e.transform.position = p.position;
        e.gameObject.SetActive(true);
        e.Highlight(true);

        StopAutoOff(showIndex);

        resolvedThisRound[showIndex] = false;
        autoOffJobs[showIndex] = StartCoroutine(AutoOffRoutine(showIndex));

        Debug.Log("[1-1] SHOW idx=" + showIndex + " posIdx=" + posIdx);

        showIndex++;
    }

    void HandleInput()
    {
        canClick = true;
        Debug.Log("[1-1] INPUT START expected=" + inputIndex);
    }

    IEnumerator AutoOffRoutine(int idx)
    {
        yield return new WaitForSeconds(autoOffSeconds);

        if (ended) yield break;
        if (resolvedThisRound[idx]) yield break;

        resolvedThisRound[idx] = true;
        missCount++;

        if (enemies[idx] != null)
            enemies[idx].Clear();

        Debug.Log("[1-1] MISS: AutoOff Timeout idx=" + idx);

        // 자동 off가 현재 입력 대상이면 다음으로 진행
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
            enemies[idx].Clear();

        Debug.Log("[1-1] SUCCESS: idx=" + idx);

        inputIndex++;
        TryEndRound();
    }

    void TryEndRound()
    {
        if (inputIndex < ENEMY_COUNT) return;
        EndRound();
    }

    void EndRound()
    {
        round++;

        Debug.Log("[1-1] ROUND END round=" + round + " success=" + successCount + " miss=" + missCount);

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
                enemies[i].gameObject.SetActive(false);
            }
        }
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
}
