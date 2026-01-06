using System.Collections.Generic;
using UnityEngine;

public class RhythmStopMinigame : MiniGameBase
{
    protected override float TimerDuration => 30f;
    protected override string MinigameExplain => "Input 타이밍에 좌클릭! 컨테이너 위면 성공";

    [Header("Prefabs")]
    public ContainerTarget containerPrefab;
    public PoliceMover policePrefab;

    [Header("Lane")]
    public float[] laneYs = { 3f, 0f, -3f };

    [Header("Positions")]
    public float policeStartX = 10f;
    public float containerXMin = -7f;
    public float containerXMax = 7f;

    [Header("Timing")]
    public float travelTime = 1f;          // Input 1초 전에 Spawn
    public float inputWindowSeconds = 0.25f;

    private ContainerTarget[] containers;
    private List<int> laneOrder;

    private PoliceMover currentPolice;
    private Collider2D currentPoliceCol;

    private bool inputWindowOpen = false;
    private bool pressedThisWindow = false;

    private int spawnCount = 0;
    private int resolveCount = 0;
    private int missCount = 0;

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

        if (inputWindowOpen && Input.GetMouseButtonDown(0))
        {
            OnClick();
        }
    }

    //  CSV에서 Input 이벤트만 받음
    public override void OnRhythmEvent(string action)
    {
        if (finished) return;

        if (action == "Input")
        {
            SpawnPolice();
            OpenInputWindow();
        }
    }

    private void SpawnPolice()
    {
        if (spawnCount >= 3) return;

        int lane = laneOrder[spawnCount];
        spawnCount++;

        var target = containers[lane];

        var p = Instantiate(policePrefab);
        p.laneIndex = lane;
        p.transform.position = new Vector3(policeStartX, laneYs[lane], 0f);

        float now = Time.time;
        p.StartMoveTimed(
            policeStartX,
            target.transform.position.x,
            now,
            now + travelTime
        );

        currentPolice = p;
        currentPoliceCol = p.GetComponent<Collider2D>();
    }

    private void OpenInputWindow()
    {
        CancelInvoke(nameof(CloseInputWindow));

        inputWindowOpen = true;
        pressedThisWindow = false;

        Invoke(nameof(CloseInputWindow), inputWindowSeconds);
    }

    private void CloseInputWindow()
    {
        inputWindowOpen = false;

        if (!pressedThisWindow)
        {
            missCount++;
            resolveCount++;
            CheckFinal();
        }
    }

    private void OnClick()
    {
        if (!inputWindowOpen) return;
        if (pressedThisWindow) return;

        pressedThisWindow = true;

        if (currentPolice == null || currentPoliceCol == null)
        {
            missCount++;
            resolveCount++;
            CheckFinal();
            return;
        }

        bool onTarget = containers[currentPolice.laneIndex]
            .IsPoliceOver(currentPoliceCol);

        if (onTarget)
        {
            currentPolice.LockHere();
        }
        else
        {
            missCount++;
        }

        resolveCount++;
        CheckFinal();
    }

    private void CheckFinal()
    {
        if (resolveCount < 3) return;

        finished = true;

        if (missCount > 0) Fail();
        else Success();
    }

    private void SetupContainers()
    {
        containers = new ContainerTarget[laneYs.Length];

        for (int i = 0; i < laneYs.Length; i++)
        {
            var c = Instantiate(containerPrefab);
            c.laneIndex = i;
            c.transform.position = new Vector3(
                Random.Range(containerXMin, containerXMax),
                laneYs[i],
                0f
            );
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
    }
}
