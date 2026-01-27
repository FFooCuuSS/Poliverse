using System.Collections;
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
        Debug.Log("[1-1] Start() called");
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

        PrepareShowPositions();
        ResetRoundObjects();

        Debug.Log("[1-1] StartGame");
    }

    public override void BindRhythmManager(IRhythmManager manager)
    {
        Debug.Log("[1-1] BindRhythmManager called manager=" + (manager != null));
        if (rhythmManager != null)
            rhythmManager.OnEventTriggered -= OnRhythmEvent;

        rhythmManager = manager;

        if (rhythmManager != null)
            rhythmManager.OnEventTriggered += OnRhythmEvent;
    }

    public override void OnRhythmEvent(string action)
    {
        Debug.Log("[1-1] OnRhythmEvent action=" + action);
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

        if (inputIndex >= ENEMY_COUNT)
            return;

        if (resolvedThisRound[inputIndex])
            return;

        if (clicked != enemies[inputIndex])
        {
            missCount++;
            Debug.Log("[1-1] MISS: Wrong Enemy (expected=" + inputIndex + ")");
            return;
        }

        ResolveSuccess(inputIndex);
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

        resolvedThisRound[showIndex] = false;
        autoOffJobs[showIndex] = StartCoroutine(AutoOffRoutine(showIndex));

        Debug.Log("[1-1] HandleShow BEFORE showIndex++");
        showIndex++;
        Debug.Log("[1-1] HandleShow AFTER showIndex++ showIndex=" + showIndex);

        if (showIndex > 3)
        {
            if (Scope != null) Scope.SetActive(true);
        }
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
        {
            // 원하는 흐름: FadeOut(Hide) -> SetActive(false)
            StartCoroutine(EnemyHideThenSetActiveFalse(enemies[idx]));
        }

        Debug.Log("[1-1] SUCCESS: idx=" + idx);

        inputIndex++;
        TryEndRound();
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

                // 라운드 리셋은 즉시 정리: 알파를 낮춰두고 비활성화
                EnemySetInactiveImmediate(enemies[i]);
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

    // 아래부터 enemy 표시/숨김 유틸(요구한 순서 그대로 구현)

    void EnemySetActiveTrueThenShow(enemy_1_1_test e)
    {
        if (e == null) return;

        GameObject go = e.gameObject;

        // 먼저 켜기
        go.SetActive(true);
        Debug.Log("AFTER SetActive(true): activeSelf=" + go.activeSelf + " activeInHierarchy=" + go.activeInHierarchy);


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
