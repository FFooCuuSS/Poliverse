using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_6_remake : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 12f;
    protected override string MinigameTitle => "경찰 인력 배치";

    protected override string MinigameExplain => "잠시 기다려 플랫폼 위치를 봐주세요.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "경찰 아이콘이 플랫폼 위에 도착하면 화면을 터치해주세요."
    };

    [Header("Prefabs")]
    public ContainerTarget containerPrefab;
    public PoliceMover policePrefab;

    [Header("Container Sprites")]
    public Sprite blueSprite;
    public Sprite greenSprite;
    public Sprite whiteSprite;

    [Header("Police Base Sprites")]
    public Sprite bluePolice;
    public Sprite greenPolice;
    public Sprite whitePolice;

    [Header("Police Result Sprites")]
    public Sprite bluePoliceSuccess;
    public Sprite bluePoliceFail;
    public Sprite greenPoliceSuccess;
    public Sprite greenPoliceFail;
    public Sprite whitePoliceSuccess;
    public Sprite whitePoliceFail;

    [Header("Set Parent")]
    public Transform mainParent;

    [Header("Lane Y Positions")]
    public float[] laneYs = { 3f, 0f, -3f };

    [Header("Container Random X Range")]
    public float containerXMin = -6f;
    public float containerXMax = 5f;

    [Header("Container Move")]
    public float containerStartX = 12f;
    public float containerMoveTime = 1f;

    [Header("Police Start X")]
    public float policeStartX = 10f;

    [Header("Police Move Timing")]
    public float travelTime = 1f;

    [Header("Judgement Windows (seconds)")]
    public float perfectWindow = 0.0f;
    public float goodWindow = 0.2f;

    [Header("Auto destroy X")]
    public float destroyX = -8f;

    private ContainerTarget[] containers;
    private float[] containerTargetXs;          // 각 컨테이너 최종 목표 x 저장
    private int nextContainerMoveIndex = 0;    // 다음에 움직일 컨테이너 인덱스

    private List<int> laneOrder;

    private PoliceMover currentPolice;

    private PoliceMover police1;
    private PoliceMover police2;
    private PoliceMover police3;

    private readonly Dictionary<PoliceMover, int> policeTypeByObj = new Dictionary<PoliceMover, int>();

    private bool inputEnabled = false;
    private bool clickLocked = false;

    private float inputOnTime = 0f;
    private float targetClickTime = 0f;

    private int spawnCount = 0;
    private int judgedCount = 0;
    private int hitCount = 0;

    private bool finished = false;

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[1-6] SetDemoMode = {isDemoMode}");
    }

    private void Start()
    {
        SetupContainers();
        BuildLaneOrder();
        StartGame();
    }

    private void Update()
    {
        if (IsDemoMode) return;

        if (finished) return;
        if (IsInputLocked) return;

        if (inputEnabled && !clickLocked && Input.GetMouseButtonDown(0))
        {
            JudgeByTimingAndLock();
        }
    }

    public override void OnRhythmEvent(string action)
    {
        if (finished) return;

        if (action == "Container")
        {
            MoveNextContainer();
        }
        else if (action == "Spawn")
        {
            SpawnPolice();
        }
        else if (action == "Input")
        {
            EnableInputNow();
        }
    }

    private void MoveNextContainer()
    {
        // 0,1,2 순서대로 한 번씩만 이동
        if (nextContainerMoveIndex >= containers.Length) return;

        ContainerTarget c = containers[nextContainerMoveIndex];
        if (c == null)
        {
            nextContainerMoveIndex++;
            return;
        }

        MoveContainer1_6 mover = c.GetComponent<MoveContainer1_6>();
        if (mover != null)
        {
            float y = laneYs[nextContainerMoveIndex];
            float targetX = containerTargetXs[nextContainerMoveIndex];

            mover.InitMove(containerStartX, targetX, y, containerMoveTime);
        }
        else
        {
            Debug.LogWarning("[1-6] MoveContainer1_6 컴포넌트가 containerPrefab에 없음");
        }

        nextContainerMoveIndex++;
    }

    private void SpawnPolice()
    {
        if (spawnCount >= 3) return;

        int lane = laneOrder[spawnCount];

        int spawnIndex = spawnCount + 1;
        spawnCount++;

        PoliceMover p = Instantiate(policePrefab, mainParent);
        p.gameObject.SetActive(true);
        p.laneIndex = lane;
        p.destroyX = destroyX;

        p.transform.position = new Vector3(policeStartX, laneYs[lane], 0f);

        ApplyPoliceBaseSpriteAndCacheType(p);

        MoveContainer1_6 mover = containers[lane].GetComponent<MoveContainer1_6>();
        float targetX = (mover != null) ? mover.GetTargetX() : containers[lane].transform.position.x;

        p.InitMoveToTarget(policeStartX, targetX, travelTime);
        p.OnAutoDestroyed += OnPoliceAutoDestroyed;

        currentPolice = p;

        if (spawnIndex == 1) police1 = p;
        else if (spawnIndex == 2) police2 = p;
        else if (spawnIndex == 3) police3 = p;

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

        clickLocked = true;
        inputEnabled = false;

        currentPolice.LockHere();

        float now = Time.time;
        float delta = Mathf.Abs(now - targetClickTime);

        if (delta <= perfectWindow)
        {
            hitCount++;
            judgedCount++;
        }
        else if (delta <= goodWindow)
        {
            hitCount++;
            judgedCount++;
        }
        else
        {
            judgedCount++;
        }

        CheckFinishIfDone();
    }

    private void OnPoliceAutoDestroyed(PoliceMover p)
    {
        if (p != currentPolice) return;

        judgedCount++;

        inputEnabled = false;
        clickLocked = true;

        CheckFinishIfDone();
    }

    private void CheckFinishIfDone()
    {
        if (judgedCount < 3) return;

        finished = true;

        bool isSuccess = (hitCount >= 3);

        ApplyPoliceResultSprites(isSuccess);

        if (isSuccess) Success();
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
        containerTargetXs = new float[laneYs.Length];

        for (int i = 0; i < laneYs.Length; i++)
        {
            float targetX = Random.Range(containerXMin, containerXMax);
            float y = laneYs[i];

            ContainerTarget c = Instantiate(containerPrefab, mainParent);

            // 처음엔 모두 시작 위치에서 대기
            c.transform.position = new Vector3(containerStartX, y, 0f);

            SpriteRenderer sr = c.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (Mathf.Approximately(y, -3f))
                {
                    sr.sprite = blueSprite;
                }
                else if (Mathf.Approximately(y, 3f))
                {
                    sr.sprite = greenSprite;
                }
                else
                {
                    sr.sprite = whiteSprite;
                }
            }

            c.gameObject.SetActive(true);
            c.laneIndex = i;
            containers[i] = c;

            // 나중에 Container 액션 때 쓰려고 목표 x 저장만 해둠
            containerTargetXs[i] = targetX;
        }

        nextContainerMoveIndex = 0;
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

        police1 = null;
        police2 = null;
        police3 = null;
        currentPolice = null;

        policeTypeByObj.Clear();
    }

    private void ApplyPoliceBaseSpriteAndCacheType(PoliceMover p)
    {
        if (p == null) return;

        SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        float y = p.transform.position.y;
        int type;

        if (Mathf.Approximately(y, -3f))
        {
            sr.sprite = bluePolice;
            type = 0;
        }
        else if (Mathf.Approximately(y, 3f))
        {
            sr.sprite = greenPolice;
            type = 2;
        }
        else
        {
            sr.sprite = whitePolice;
            type = 1;
        }

        policeTypeByObj[p] = type;
    }

    private void ApplyPoliceResultSprites(bool success)
    {
        ApplyOne(police1, success);
        ApplyOne(police2, success);
        ApplyOne(police3, success);
    }

    private void ApplyOne(PoliceMover p, bool success)
    {
        if (p == null) return;

        SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (!policeTypeByObj.TryGetValue(p, out int type))
        {
            float y = p.transform.position.y;

            if (Mathf.Approximately(y, -3f)) type = 0;
            else if (Mathf.Approximately(y, 3f)) type = 2;
            else type = 1;
        }

        switch (type)
        {
            case 0:
                sr.sprite = success ? bluePoliceSuccess : bluePoliceFail;
                break;

            case 2:
                sr.sprite = success ? greenPoliceSuccess : greenPoliceFail;
                break;

            default:
                sr.sprite = success ? whitePoliceSuccess : whitePoliceFail;
                break;
        }
    }

    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim();

        if (actionType != "Input")
            return;

        Debug.Log($"[1-6 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}");

        if (currentPolice != null)
        {
            // 컨테이너의 목표 X 좌표를 가져옴
            MoveContainer1_6 mover = containers[currentPolice.laneIndex].GetComponent<MoveContainer1_6>();
            if (mover != null)
            {
                float targetX = mover.GetTargetX();
                float currentX = currentPolice.transform.position.x;

                // 현재 위치에서 목표 위치까지 남은 거리를 구함 (왼쪽으로 이동하므로 currentX - targetX)
                float remainingDist = currentX - targetX;

                // PoliceMover의 이동 속도를 역산하거나 travelTime을 활용해 남은 도달 시간을 계산
                // travelTime 동안 (policeStartX - targetX)를 이동하므로 속도 = totalDist / travelTime
                float totalDist = policeStartX - targetX;
                float speed = (travelTime <= 0f) ? 0f : totalDist / travelTime;

                float timeToReach = (speed > 0f) ? remainingDist / speed : 0f;

                // 뱃지가 플랫폼에 딱 도달하는 시점을 targetClickTime으로 정확히 설정
                inputEnabled = true;
                clickLocked = false;
                targetClickTime = Time.time + timeToReach;

                // 지연 시간(timeToReach) 뒤에 정확히 판정 함수가 실행되도록 코루틴 사용
                StartCoroutine(DemoJudgeCo(timeToReach));
                return;
            }
        }

        // 혹시 모를 예외 상황 처리
        inputEnabled = true;
        clickLocked = false;
        targetClickTime = Time.time;
        JudgeByTimingAndLock();
    }

    private IEnumerator DemoJudgeCo(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        // 경찰이 정확히 플랫폼 위치에 도달했을 때 판정 및 멈춤 실행
        JudgeByTimingAndLock();
    }
}