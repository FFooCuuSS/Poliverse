using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MiniGameBase;

public class minigame_1_1_remake_DEBUG : MiniGameBase
{
    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "분류해라!";

    private IRhythmManager rhythmManager;

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
        Debug.Log("[1-1] START");

        Scope.SetActive(false);
        cam = Camera.main;
        StartGame();
    }

    public override void StartGame()
    {
        Debug.Log("[1-1] StartGame");

        ended = false;
        canClick = false;

        round = 0;
        showIndex = 0;
        inputIndex = 0;

        successCount = 0;
        missCount = 0;

        resolvedThisRound = new bool[ENEMY_COUNT];
        autoOffJobs = new Coroutine[ENEMY_COUNT];

        lastRhythmAction = null;

        PrepareShowPositions();
        ResetRoundObjects();
    }

    public override void BindRhythmManager(IRhythmManager manager)
    {
        Debug.Log("[1-1] BindRhythmManager");

        if (rhythmManager != null)
            rhythmManager.OnEventTriggered -= OnRhythmEvent;

        rhythmManager = manager;

        if (rhythmManager != null)
        {
            rhythmManager.OnEventTriggered += OnRhythmEvent;
            Debug.Log("[1-1] RhythmManager bind complete");
        }
        else
        {
            Debug.LogError("[1-1] RhythmManager NULL");
        }
    }

    public override void OnRhythmEvent(string action)
    {
        Debug.Log("[1-1] RhythmEvent = " + action);

        if (ended || string.IsNullOrEmpty(action)) return;

        action = action.Trim();

        if (lastRhythmAction == "Show" && action == "Input")
            Scope.SetActive(true);

        else if (lastRhythmAction == "Input" && action == "Show")
            Scope.SetActive(false);

        lastRhythmAction = action;

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

        if (targetScope != null)
            targetScope.position = w;

        Collider2D col = Physics2D.OverlapPoint(w);

        if (col == null)
        {
            Debug.Log("[1-1] Click empty");

            if (canClick) missCount++;
            return;
        }

        var clicked = col.GetComponent<enemy_1_1_test>();

        if (clicked == null)
        {
            Debug.Log("[1-1] Click non enemy");

            if (canClick) missCount++;
            return;
        }

        Debug.Log("[1-1] Click enemy = " + clicked.name);

        if (!canClick || pendingInputs.Count == 0)
        {
            Debug.Log("[1-1] Click outside input window");
            missCount++;
            return;
        }

        int expected = pendingInputs.Peek();

        Debug.Log("[1-1] Expected enemy index = " + expected);

        if (clicked != enemies[expected])
        {
            Debug.Log("[1-1] Wrong enemy");
            missCount++;
            return;
        }

        // 먼저 큐에서 제거
        pendingInputs.Dequeue();

        // 그 다음 성공 처리
        ResolveSuccess(expected);
    }

    void HandleShow()
    {
        Debug.Log("[1-1] HandleShow index=" + showIndex);

        if (showIndex >= ENEMY_COUNT) return;

        int posIdx = shuffledPosIndex[showIndex];

        var e = enemies[showIndex];
        var p = showPositions[posIdx];

        e.transform.position = p.position;

        EnemySetActiveTrueThenShow(e);

        StopAutoOff(showIndex);
        autoOffJobs[showIndex] = StartCoroutine(AutoOffRoutine(showIndex));

        showIndex++;
    }

    void HandleInput()
    {
        Debug.Log("[1-1] HandleInput");

        if (canClick)
            CloseInputWindow();

        pendingInputs.Clear();

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
            Debug.LogWarning("[1-1] Input ignored (queue empty)");
            return;
        }

        canClick = true;

        Debug.Log("[1-1] Input start expected=" + pendingInputs.Peek());

        if (inputWindowJob != null) StopCoroutine(inputWindowJob);
        inputWindowJob = StartCoroutine(InputWindowRoutine());
    }

    IEnumerator InputWindowRoutine()
    {
        yield return new WaitForSeconds(inputWindowSeconds);

        Debug.Log("[1-1] Input window close");

        canClick = false;
        inputWindowJob = null;
    }

    IEnumerator AutoOffRoutine(int idx)
    {
        yield return new WaitForSeconds(autoOffSeconds);

        if (resolvedThisRound[idx]) yield break;

        resolvedThisRound[idx] = true;
        missCount++;

        Debug.Log("[1-1] Auto miss idx=" + idx);

        yield return EnemyHideThenSetActiveFalse(enemies[idx]);

        if (idx == inputIndex)
        {
            inputIndex++;
            TryEndRound();
        }
    }

    void ResolveSuccess(int idx)
    {
        Debug.Log("[1-1] SUCCESS idx=" + idx);

        resolvedThisRound[idx] = true;
        successCount++;

        StopAutoOff(idx);

        StartCoroutine(EnemyHideThenSetActiveFalse(enemies[idx]));

        CloseInputWindow();

        inputIndex++;
        TryEndRound();
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

        Debug.Log("[1-1] Round End");

        EndRound();

        Scope.SetActive(false);
        targetScope.position = Vector2.zero;
    }

    void EndRound()
    {
        round++;

        Debug.Log("[1-1] Round = " + round);

        if (round >= TOTAL_ROUND)
        {
            Debug.Log("[1-1] FINAL success=" + successCount + " miss=" + missCount);

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

            enemies[i].ResetEnemy();
            EnemySetInactiveImmediate(enemies[i]);
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

    void EnemySetActiveTrueThenShow(enemy_1_1_test e)
    {
        GameObject go = e.gameObject;

        go.SetActive(true);

        FadeActiveToggle fade = go.GetComponent<FadeActiveToggle>();

        if (fade != null)
        {
            fade.SetAlphaImmediate(fade.inactiveAlpha);
            fade.FadeIn();
        }
    }

    IEnumerator EnemyHideThenSetActiveFalse(enemy_1_1_test e)
    {
        FadeActiveToggle fade = e.GetComponent<FadeActiveToggle>();

        if (fade != null)
        {
            fade.FadeOut();
            yield return new WaitForSeconds(fade.GetFadeTime());
        }

        e.gameObject.SetActive(false);
    }

    void EnemySetInactiveImmediate(enemy_1_1_test e)
    {
        FadeActiveToggle fade = e.GetComponent<FadeActiveToggle>();

        if (fade != null)
            fade.SetAlphaImmediate(fade.inactiveAlpha);

        e.gameObject.SetActive(false);
    }
}