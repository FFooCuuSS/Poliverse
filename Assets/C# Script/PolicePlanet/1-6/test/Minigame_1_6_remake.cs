using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_6_remake : MiniGameBase
{
    protected override float TimerDuration => 12f;
    protected override string MinigameExplain => "배치해라!";

    [Header("Prefabs")]
    public ContainerTarget containerPrefab;
    public PoliceMover policePrefab;
    public Sprite blueSprite;
    public Sprite greenSprite;
    public Sprite whiteSprite;
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

    [Header("set parent")]
    public Transform mainParent;

    [Header("Lane Y Positions")]
    public float[] laneYs = { 3f, 0f, -3f };

    [Header("Container Random X Range")]
    public float containerXMin = -6f;
    public float containerXMax = 5f;

    [Header("Police Start X")]
    public float policeStartX = 10f;

    [Header("Move Timing")]
    public float travelTime = 1f;

    [Header("Judgement Windows (seconds)")]
    public float perfectWindow = 0.0f;
    public float goodWindow = 0.2f;

    [Header("Auto destroy X")]
    public float destroyX = -8f;

    private ContainerTarget[] containers;
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

        if (inputEnabled && !clickLocked && Input.GetMouseButtonDown(0))
        {
            JudgeByTimingAndLock();
        }
    }

    public override void OnRhythmEvent(string action)
    {
        if (finished) return;

        if (action == "Spawn") SpawnPolice();
        else if (action == "Input") EnableInputNow();
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

        float targetX = containers[lane].transform.position.x;
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

        for (int i = 0; i < laneYs.Length; i++)
        {
            float x = Random.Range(containerXMin, containerXMax);
            float y = laneYs[i];

            ContainerTarget c = Instantiate(containerPrefab, mainParent);
            c.transform.position = new Vector3(x, y, 0f);

            SpriteRenderer sr = c.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (Mathf.Approximately(y, -3f)) sr.sprite = blueSprite;
                else if (Mathf.Approximately(y, 3f)) sr.sprite = greenSprite;
                else sr.sprite = whiteSprite;
            }

            c.gameObject.SetActive(true);
            c.laneIndex = i;
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

        // y 기준으로 타입 결정 (laneYs가 3,0,-3인 구조니까)
        int type;
        if (Mathf.Approximately(y, -3f))
        {
            sr.sprite = bluePolice;
            type = 0; // Blue
        }
        else if (Mathf.Approximately(y, 3f))
        {
            sr.sprite = greenPolice;
            type = 2; // Green
        }
        else
        {
            sr.sprite = whitePolice;
            type = 1; // White
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

        // 혹시 캐시가 안 잡힌 예외 상황 대비(보험)
        if (!policeTypeByObj.TryGetValue(p, out int type))
        {
            float y = p.transform.position.y;
            if (Mathf.Approximately(y, -3f)) type = 0;
            else if (Mathf.Approximately(y, 3f)) type = 2;
            else type = 1;
        }

        switch (type)
        {
            case 0: // Blue
                sr.sprite = success ? bluePoliceSuccess : bluePoliceFail;
                break;
            case 2: // Green
                sr.sprite = success ? greenPoliceSuccess : greenPoliceFail;
                break;
            default: // White
                sr.sprite = success ? whitePoliceSuccess : whitePoliceFail;
                break;
        }
    }
}
