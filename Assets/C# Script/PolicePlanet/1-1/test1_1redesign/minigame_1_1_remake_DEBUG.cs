using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MiniGameBase;

public class minigame_1_1_remake_DEBUG : MiniGameBase
{
    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "분류해라!";

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
        StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();

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
        pendingInputs.Clear();

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

        // manager가 null이어도 에러 안 띄움.
        // 리듬매니저 분리/전환 중에는 정상적으로 null일 수 있음.
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended || string.IsNullOrEmpty(action)) return;

        base.OnRhythmEvent(action);
        action = action.Trim();

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
        if (!Input.GetMouseButtonDown(0)) return;
        if (cam == null) return;

        Vector3 w = cam.ScreenToWorldPoint(Input.mousePosition);
        w.z = 0;

        if (targetScope != null)
            targetScope.position = w;

        Collider2D col = Physics2D.OverlapPoint(w);

        if (col == null)
        {
            if (canClick)
                missCount++;

            return;
        }

        var clicked = col.GetComponent<enemy_1_1_test>();

        if (clicked == null)
        {
            if (canClick)
                missCount++;

            return;
        }

        if (!canClick || pendingInputs.Count == 0)
        {
            missCount++;
            return;
        }

        int expected = pendingInputs.Peek();

        if (!IsValidEnemyIndex(expected))
        {
            pendingInputs.Dequeue();
            missCount++;
            return;
        }

        if (clicked != enemies[expected])
        {
            missCount++;
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

        StopAutoOff(showIndex);
        autoOffJobs[showIndex] = StartCoroutine(AutoOffRoutine(showIndex));

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
        missCount++;

        yield return EnemyHideThenSetActiveFalse(enemies[idx]);

        if (idx == inputIndex)
        {
            inputIndex++;
            TryEndRound();
        }
    }

    void ResolveSuccess(int idx)
    {
        if (!IsValidEnemyIndex(idx)) return;
        StartCoroutine(ResolveSuccessRoutine(idx));
    }

    IEnumerator ResolveSuccessRoutine(int idx)
    {
        resolvedThisRound[idx] = true;
        successCount++;

        StopAutoOff(idx);
        CloseInputWindow();

        yield return EnemySuccessHideThenSetActiveFalse(enemies[idx]);

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

        if (round >= TOTAL_ROUND)
        {
            Log($"[1-1] FINAL success={successCount}, miss={missCount}");

            if (successCount >= 7)
                Succeed();
            else
                Failure();

            return;
        }

        canClick = false;
        showIndex = 0;
        inputIndex = 0;
        pendingInputs.Clear();

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
}