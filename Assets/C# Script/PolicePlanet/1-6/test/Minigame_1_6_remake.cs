using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_6_remake : MiniGameBase
{
    protected override float TimerDuration => 12f;
    protected override string MinigameExplain => "타이밍에 맞춰 배치해라!";

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

    [Header("Move Timing")]
    public float travelTime = 1f; // startX->containerX까지 1초

    [Header("Judgement Windows (seconds)")]
    public float perfectWindow = 0.0f; // "정확히 1초"를 원하면 0, 현실적으로는 0.03 추천
    public float goodWindow = 0.2f;

    [Header("Auto destroy X")]
    public float destroyX = -8f;

    private ContainerTarget[] containers;
    private List<int> laneOrder;

    private PoliceMover currentPolice;

    private bool inputEnabled = false;     // Input 이벤트로 켜짐
    private bool clickLocked = false;      // 클릭 후 다음 Input까지 잠금

    private float inputOnTime = 0f;        // Input 켜진 실제 시간(Time.time)
    private float targetClickTime = 0f;    // inputOnTime + 1초

    private int spawnCount = 0;
    private int judgedCount = 0;           // 3회 판정 완료 수(Perfect/Good/Miss)
    private int hitCount = 0;              // Perfect/Good 성공 횟수

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

        // Input이 켜져 있고, 클릭 잠금이 아닐 때만 좌클릭 허용
        if (inputEnabled && !clickLocked && Input.GetMouseButtonDown(0))
        {
            JudgeByTimingAndLock();
        }
    }

    // RhythmManagerTest에서 CSV 이벤트가 들어오는 곳
    public override void OnRhythmEvent(string action)
    {
        if (finished) return;

        if (action == "Spawn")
        {
            SpawnPolice();
        }
        else if (action == "Input")
        {
            EnableInputNow();
        }
    }

    private void SpawnPolice()
    {
        if (spawnCount >= 3) return;

        int lane = laneOrder[spawnCount];
        spawnCount++;

        // 경찰 생성
        PoliceMover p = Instantiate(policePrefab);
        p.gameObject.SetActive(true);
        p.laneIndex = lane;
        p.destroyX = destroyX;

        // y는 lane, x는 startX 고정
        p.transform.position = new Vector3(policeStartX, laneYs[lane], 0f);

        // 목표 컨테이너 x
        float targetX = containers[lane].transform.position.x;

        // 1초에 정확히 도착하도록 속도 세팅
        p.InitMoveToTarget(policeStartX, targetX, travelTime);

        // 화면 밖 자동 제거 시 Miss 처리하려고 이벤트 연결
        p.OnAutoDestroyed += OnPoliceAutoDestroyed;

        currentPolice = p;

        // 스폰 시점엔 클릭 막아두고(Input 이벤트가 와야 풀림)
        inputEnabled = false;
        clickLocked = true;
    }

    private void EnableInputNow()
    {
        if (currentPolice == null) return;

        inputEnabled = true;
        clickLocked = false;

        inputOnTime = Time.time;
        targetClickTime = inputOnTime + 1f;
    }

    private void JudgeByTimingAndLock()
    {
        if (currentPolice == null) return;

        // 클릭하면 즉시 고정 + 다음 Input까지 클릭 불가
        clickLocked = true;
        inputEnabled = false;

        currentPolice.LockHere();

        float now = Time.time;
        float delta = Mathf.Abs(now - targetClickTime);

        // 판정: Perfect / Good / Miss
        if (delta <= perfectWindow)
        {
            hitCount++;
            judgedCount++;
            Debug.Log("Perfect");
        }
        else if (delta <= goodWindow)
        {
            hitCount++;
            judgedCount++;
            Debug.Log("Good");
        }
        else
        {
            judgedCount++;
            Debug.Log("Miss");
        }

        CheckFinishIfDone();
    }

    // 클릭 없이 지나가서 삭제되면 Miss 처리
    private void OnPoliceAutoDestroyed(PoliceMover p)
    {
        // 현재 대상 경찰이 아닌 경우는 무시(안전)
        if (p != currentPolice) return;

        // 이미 클릭 판정으로 처리된 상태면 무시
        if (clickLocked == true && inputEnabled == false && judgedCount > 0)
        {
            // 완전 엄격히 하려면 "이번 회차가 처리됐는지" 플래그로 관리하는게 더 정확함
        }

        // "클릭이 없어서 x<-8"이면 Miss 1회
        judgedCount++;
        Debug.Log("Miss");

        // 다음 Input 전까지 클릭 불가 상태 유지
        inputEnabled = false;
        clickLocked = true;

        CheckFinishIfDone();
    }

    private void CheckFinishIfDone()
    {
        // 3회 판정이 끝나야 결과 표시
        if (judgedCount < 3) return;

        finished = true;

        // Perfect/Good이 3번이면 성공, 아니면 실패
        if (hitCount >= 3) Success();
        else Fail();
    }

    private void SetupContainers()
    {
        if (containerPrefab == null)
        {
            Debug.LogError("[1-6] containerPrefab is NULL");
            enabled = false;
            return;
        }

        containers = new ContainerTarget[laneYs.Length];

        for (int i = 0; i < laneYs.Length; i++)
        {
            float x = Random.Range(containerXMin, containerXMax);
            float y = laneYs[i];

            ContainerTarget c = Instantiate(containerPrefab);
            c.gameObject.SetActive(true);
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
        judgedCount = 0;
        hitCount = 0;

        inputEnabled = false;
        clickLocked = false;
        finished = false;
    }
}
