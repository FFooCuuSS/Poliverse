using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_6_remake : MiniGameBase
{
    protected override float TimerDuration => 30f;
    protected override string MinigameExplain => "Spawn에 경찰 출발, Input 타이밍에 Space! 컨테이너 위면 성공";

    [Header("Prefabs")]
    public ContainerTarget containerPrefab;
    public PoliceMover policePrefab;

    [Header("Lane Y Positions")]
    public float[] laneYs = { 3f, 0f, -3f };

    [Header("Container Random X Range")]
    public float containerXMin = -7f;
    public float containerXMax = 7f;

    [Header("Police Start X")]
    public float policeStartX = 10f;

    [Header("Timing")]
    public float travelTime = 1f;          // Spawn 후 1초에 컨테이너 X 도착
    public float inputWindowSeconds = 0.25f; // Input 타이밍에서 Space 허용 구간

    [Header("Result Rule")]
    public bool acceptGood = true; // Good도 성공 처리

    private ContainerTarget[] containers;
    private List<int> laneOrder;

    private PoliceMover currentPolice;
    private Collider2D currentPoliceCol;

    private int spawnCount = 0;      // Spawn 몇 번 했는지 (0~3)
    private int resolvedCount = 0;   // 경찰 3명에 대해 (성공 or 미스) 처리한 횟수
    private int missCount = 0;
    private int successCount = 0;

    // 현재 경찰이 “Input 기회를 받은 상태인지”
    private bool inputStartedForCurrent = false;

    // Input 윈도우 상태
    private bool inputWindowOpen = false;

    // 리듬매니저 판정 기다리는 중인지 (내가 Space 눌러서 만든 판정만 처리하려고)
    private bool pendingJudge = false;

    private bool finished = false;

    private void Start()
    {
        SetupContainers();
        BuildLaneOrder();
        StartGame();
    }

    private void Update()
    {
        if (finished) return;
        if (IsInputLocked) return;

        // Input 창 열려있을 때만 Space 받음
        if (inputWindowOpen && !pendingJudge && Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpacePress();
        }
    }

    public override void OnRhythmEvent(string action)
    {
        if (finished) return;

        if (action == "Spawn")
        {
            // 조건 2) 다음 경찰이 움직이는데도 입력이 안 되어있으면 Miss
            // (현재 경찰이 Input을 한 번이라도 받았고, 아직 해결(성공) 못했으면 miss)
            TryResolveAsMissBecauseNextSpawn();

            SpawnPolice();
        }
        else if (action == "Input")
        {
            // 현재 경찰에게 Input 기회를 부여 (윈도우 열기)
            OpenInputWindow();
        }
        else
        {
            // 다른 액션이면 그냥 닫음
            CloseInputWindowImmediate();
        }
    }

    // =========================
    // Input Window
    // =========================
    private void OpenInputWindow()
    {
        if (currentPolice == null) return;

        // Input을 받았다는 플래그
        inputStartedForCurrent = true;

        // 연속 호출 대비
        CancelInvoke(nameof(CloseInputWindowTimeUp));

        inputWindowOpen = true;
        pendingJudge = false;

        Invoke(nameof(CloseInputWindowTimeUp), inputWindowSeconds);
    }

    private void CloseInputWindowTimeUp()
    {
        inputWindowOpen = false;

        // 여기서는 Miss 처리 안 함.
        // 왜냐면 너 규칙이 "다음 Spawn 때까지 입력이 안 되어있으면 Miss" 이거라서.
        // 마지막 경찰은 다음 Spawn이 없으니, 마지막만 예외로 여기서 처리해줘야 함.
        if (spawnCount >= 3)
        {
            // 마지막 경찰인데 Input 기회를 받았고 아직 해결 안 됐다면 -> Miss 1회
            TryResolveAsMissBecauseNoMoreSpawn();
        }
    }

    private void CloseInputWindowImmediate()
    {
        CancelInvoke(nameof(CloseInputWindowTimeUp));
        inputWindowOpen = false;
        pendingJudge = false;
    }

    // =========================
    // Space Press
    // =========================
    private void HandleSpacePress()
    {
        if (currentPolice == null || currentPoliceCol == null) return;

        // 조건 1) collide 안 된 시점에서 눌렀으면 Miss
        bool onTarget = containers[currentPolice.laneIndex].IsPoliceOver(currentPoliceCol);

        if (!onTarget)
        {
            // 미스 처리하고 현재 경찰은 “해결 완료”로 넘김(더 이상 입력 못하게)
            missCount++;
            ResolveCurrentPolice();
            return;
        }

        // collide는 됐으니 이제 리듬 타이밍 판정만 요청
        pendingJudge = true;
        OnPlayerInput("Input"); // CSV에서 type이 Input이면 action도 Input으로 맞춤
    }

    // =========================
    // Rhythm judgement (from manager)
    // =========================
    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);

        // 리듬매니저가 자동으로 뿌리는 Miss(CheckMisses)는 전부 무시
        if (!pendingJudge) return;
        pendingJudge = false;

        bool rhythmOK =
            (judgement == JudgementResult.Perfect) ||
            (acceptGood && judgement == JudgementResult.Good);

        if (!rhythmOK)
        {
            // 타이밍 미스는 "미스 카운트"로 치지 않음(너 규칙이 2개만이라서)
            // 대신, 같은 Input 윈도우 안에서 재시도 가능하게 그냥 유지
            // inputWindowOpen이 true인 동안 다시 Space 눌러도 됨
            return;
        }

        // 리듬도 OK면 성공 확정 (collide는 Space 누를 때 이미 체크했음)
        if (currentPolice != null)
        {
            currentPolice.LockHere();
            successCount++;
        }

        ResolveCurrentPolice();
    }

    // =========================
    // Spawn / Resolve
    // =========================
    private void SpawnPolice()
    {
        if (spawnCount >= 3) return;

        int lane = laneOrder[spawnCount];
        spawnCount++;

        // 경찰 생성
        PoliceMover p = Instantiate(policePrefab);
        p.laneIndex = lane;
        p.transform.position = new Vector3(policeStartX, laneYs[lane], 0f);

        float targetX = containers[lane].transform.position.x;
        float now = Time.time;

        p.StartMoveTimed(policeStartX, targetX, now, now + travelTime);

        currentPolice = p;
        currentPoliceCol = p.GetComponent<Collider2D>();

        // 새 경찰 시작하면 상태 초기화
        inputStartedForCurrent = false;
        CloseInputWindowImmediate();
    }

    private void ResolveCurrentPolice()
    {
        // 현재 경찰 1명에 대한 결과가 확정된 순간
        resolvedCount++;

        // 다음 입력 못하게
        inputStartedForCurrent = false;
        CloseInputWindowImmediate();

        // 마지막까지 다 처리했으면 최종 판정
        CheckFinish();
    }

    private void TryResolveAsMissBecauseNextSpawn()
    {
        // "다음 경찰이 움직이는데도 입력이 안 되어있는 경우" -> Miss
        // 즉, 현재 경찰이 Input 기회를 받았는데도(=inputStartedForCurrent),
        // 성공 확정(ResolveCurrentPolice)이 안 된 상태에서 다음 Spawn이 오면 miss.
        if (currentPolice == null) return;

        if (inputStartedForCurrent && resolvedCount < spawnCount)
        {
            missCount++;
            ResolveCurrentPolice();
        }
    }

    private void TryResolveAsMissBecauseNoMoreSpawn()
    {
        // 마지막 경찰은 다음 Spawn이 없으니까,
        // Input 기회를 받았는데 해결이 안 되었으면 여기서 miss 처리
        if (currentPolice == null) return;

        if (inputStartedForCurrent && resolvedCount < spawnCount)
        {
            missCount++;
            ResolveCurrentPolice();
        }
    }

    private void CheckFinish()
    {
        // 경찰 3명 다 결과가 확정됐으면 끝
        if (resolvedCount < 3) return;

        finished = true;

        if (missCount > 0) Fail();
        else Success();
    }

    // =========================
    // Setup
    // =========================
    private void SetupContainers()
    {
        if (containerPrefab == null)
        {
            Debug.LogError("[Minigame_1_6_remake] containerPrefab is NULL");
            enabled = false;
            return;
        }

        containers = new ContainerTarget[laneYs.Length];

        for (int i = 0; i < laneYs.Length; i++)
        {
            float x = Random.Range(containerXMin, containerXMax);
            float y = laneYs[i];

            ContainerTarget c = Instantiate(containerPrefab);
            c.laneIndex = i;
            c.transform.position = new Vector3(x, y, 0f);

            containers[i] = c;
        }
    }

    private void BuildLaneOrder()
    {
        laneOrder = new List<int> { 0, 1, 2 };

        for (int i = 0; i < laneOrder.Count; i++)
        {
            int r = Random.Range(i, laneOrder.Count);
            (laneOrder[i], laneOrder[r]) = (laneOrder[r], laneOrder[i]);
        }

        spawnCount = 0;
        resolvedCount = 0;
        missCount = 0;
        successCount = 0;

        inputStartedForCurrent = false;
        inputWindowOpen = false;
        pendingJudge = false;
        finished = false;
    }
}
