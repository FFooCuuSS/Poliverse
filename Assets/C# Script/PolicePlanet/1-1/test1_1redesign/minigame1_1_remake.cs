using UnityEngine;
using static MiniGameBase;

public class Minigame_1_1_remake : MiniGameBase
{
    protected override float TimerDuration => 30f;
    protected override string MinigameExplain => "분류해라!";

    enum Phase
    {
        Show,
        Input
    }

    private Phase currentPhase;
    private IRhythmManager rhythmManager;

    [Header("Enemies (정답 순서 0~3)")]
    public enemy_1_1_test[] enemies;

    [Header("Show Positions (랜덤 배치용, 4개)")]
    public Transform[] showPositions;

    [Header("Target Scope (연출용)")]
    public Transform targetScope;

    private int showIndex;
    private int inputIndex;
    private int round;

    private int[] shuffledPosIndex;

    private enemy_1_1_test clickedEnemy;

    // 클릭으로 판정을 요청한 경우에만 OnJudged를 처리하기 위한 게이트
    private bool awaitingJudge;

    // 한 번 성공/실패 처리되면 더 이상 진행되지 않게 막는 락
    private bool ended;

    private const int ENEMY_COUNT = 4;
    private const int TOTAL_ROUND = 2;

    public override void StartGame()
    {
        ended = false;
        awaitingJudge = false;

        showIndex = 0;
        inputIndex = 0;
        round = 0;
        currentPhase = Phase.Show;

        // 시작할 때 enemy 전부 숨김
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].ResetEnemy();
            enemies[i].gameObject.SetActive(false);
        }

        PrepareShowPositions();

        Debug.Log("[1-1] StartGame");
    }

    public override void BindRhythmManager(IRhythmManager manager)
    {
        if (rhythmManager != null)
        {
            rhythmManager.OnEventTriggered -= OnRhythmEvent;
            rhythmManager.OnPlayerJudged -= OnJudged;
        }

        rhythmManager = manager;

        if (rhythmManager != null)
        {
            rhythmManager.OnEventTriggered += OnRhythmEvent;
            rhythmManager.OnPlayerJudged += OnJudged;
        }
    }

    public override void OnRhythmEvent(string action)
    {
        Debug.Log("[1-1] OnRhythmEvent = " + action);

        if (ended) return;

        if (action == "Show") HandleShow();
        else if (action == "Input") currentPhase = Phase.Input;
    }

    void Update()
    {
        // 클릭 연출(스코프 이동)은 ended여도 움직이게 해도 되고,
        // 지금은 ended면 아예 아무것도 안 하게 막음
        if (ended) return;

        if (Input.GetMouseButtonDown(0))
        {
            // 스코프 이동(연출)
            if (targetScope != null && Camera.main != null)
            {
                Vector3 w = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                w.z = 0;
                targetScope.position = w;
            }

            // Input 구간이 아니면 판정 요청 안 함
            if (currentPhase != Phase.Input) return;

            // 리듬매니저가 없으면 진행 불가
            if (rhythmManager == null) return;

            // 클릭 위치에서 enemy 레이캐스트
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickedEnemy = GetClickedEnemy(new Vector2(wp.x, wp.y));

            // 이번 클릭에 대한 판정을 기다리겠다는 표시
            awaitingJudge = true;

            // CSV의 Input 노트와 매칭
            rhythmManager.ReceivePlayerInput("Input");
        }
    }

    void HandleShow()
    {
        currentPhase = Phase.Show;

        // 라운드의 첫 Show에서 위치를 다시 섞음
        if (showIndex == 0)
            PrepareShowPositions();

        if (showIndex >= ENEMY_COUNT) return;

        // showIndex번째 enemy를 랜덤 위치에 배치
        int posIdx = shuffledPosIndex[showIndex];

        if (showPositions != null && showPositions.Length >= ENEMY_COUNT)
            enemies[showIndex].transform.position = showPositions[posIdx].position;

        enemies[showIndex].gameObject.SetActive(true);
        enemies[showIndex].Highlight(true);

        showIndex++;
    }

    void OnJudged(JudgementResult result)
    {
        if (ended) return;

        // Show 구간에서 들어오는 판정(자동 Miss 포함)은 무시
        if (currentPhase != Phase.Input) return;

        // 이번 클릭으로 판정을 요청한 경우에만 처리
        if (!awaitingJudge) return;

        // 이 판정은 이번 클릭에 대응하는 것으로 확정 -> 바로 게이트 해제
        awaitingJudge = false;

        if (result == JudgementResult.Miss)
        {
            Debug.Log("[1-1] Rhythm Miss");
            Failure();
            return;
        }

        // 클릭한 게 없거나(enemy가 아닌 곳 클릭)면 실패
        if (clickedEnemy == null)
        {
            Debug.Log("[1-1] Click Miss");
            Failure();
            return;
        }

        // 순서가 틀리면 실패
        if (clickedEnemy != enemies[inputIndex])
        {
            Debug.Log("[1-1] Wrong Enemy");
            Failure();
            return;
        }

        // 성공 처리
        clickedEnemy.Clear();
        clickedEnemy = null;
        inputIndex++;

        if (inputIndex >= ENEMY_COUNT)
            EndRound();
    }

    void EndRound()
    {
        round++;

        if (round >= TOTAL_ROUND)
        {
            Debug.Log("[1-1] Game Success");
            Succeed();
            return;
        }

        // 다음 라운드 준비
        showIndex = 0;
        inputIndex = 0;
        currentPhase = Phase.Show;

        clickedEnemy = null;
        awaitingJudge = false;

        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].ResetEnemy();
            enemies[i].gameObject.SetActive(false);
        }

        PrepareShowPositions();
    }

    enemy_1_1_test GetClickedEnemy(Vector2 clickPos)
    {
        // 클릭한 지점에서 Collider2D를 찾는다
        RaycastHit2D hit = Physics2D.Raycast(clickPos, Vector2.zero);
        if (!hit) return null;

        return hit.collider.GetComponent<enemy_1_1_test>();
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
            int temp = shuffledPosIndex[i];
            shuffledPosIndex[i] = shuffledPosIndex[r];
            shuffledPosIndex[r] = temp;
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
