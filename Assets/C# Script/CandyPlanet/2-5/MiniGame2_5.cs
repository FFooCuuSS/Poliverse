using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_5 : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 5f;
    protected override string MinigameTitle => "쌓아라!";
    protected override string MinigameExplain => "죄수가 순서대로 나타나 타이밍을 알려줍니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "배경이 어두워지면 타이밍에 맞게 죄수를 터치해주세요."
    };

    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.45f;
    public override float hitWindowOverride => 0.6f;

    private bool ended;
    private int pendingInputCount;
    public bool IsInputOpen => pendingInputCount > 0;

    private IceCreamSpawner2_5 spawner;
    private Queue<IceCream2_5> iceCreamQueue = new Queue<IceCream2_5>();
    private IceCreamFloor floor;
    private Transform inlet;

    private IceCreamPipe pipe1;
    private IceCreamPipe pipe2;
    private IceCreamPipe pipe3;

    // 겹치는 세트 수를 세는 참조 카운터
    private int pipe1Count;
    private int pipe2Count;
    private int pipe3Count;

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[2-5] SetDemoMode = {isDemoMode}");
    }

    void Update()
    {
        if (IsDemoMode) return;

        if (Input.GetMouseButtonDown(0))
            OnPlayerInput();
    }

    public override void StartGame()
    {
        base.StartGame();

        spawner = FindAnyObjectByType<IceCreamSpawner2_5>();
        floor = FindAnyObjectByType<IceCreamFloor>();

        pipe1 = spawner.GetPipe(0);
        pipe2 = spawner.GetPipe(1);
        pipe3 = spawner.GetPipe(2);
        inlet = spawner.InletPoint;

        ended = false;
        pendingInputCount = 0;
        pipe1Count = pipe2Count = pipe3Count = 0;

        pipe1?.SetHighlight(false);
        pipe2?.SetHighlight(false);
        pipe3?.SetHighlight(false);
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        action = action.Trim();

        switch (action)
        {
            case "Show":
                IceCream2_5 iceCream = spawner.SpawnIceCream();
                iceCreamQueue.Enqueue(iceCream);

                pipe1Count++;
                pipe1?.SetHighlight(true);
                break;

            case "Step2":
                pipe1Count = Mathf.Max(0, pipe1Count - 1);
                pipe1?.SetHighlight(pipe1Count > 0);

                pipe2Count++;
                pipe2?.SetHighlight(true);
                break;

            case "Step3":
                pipe2Count = Mathf.Max(0, pipe2Count - 1);
                pipe2?.SetHighlight(pipe2Count > 0);

                pipe3Count++;
                pipe3?.SetHighlight(true);
                break;

            case "Input":
                pendingInputCount++;
                break;
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        if (IsDemoMode) return;
        if (ended) return;
        if (pendingInputCount <= 0) return;

        rhythmManager?.ReceivePlayerInput("Input");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);

        if (pendingInputCount > 0)
            pendingInputCount--;

        pipe3Count = Mathf.Max(0, pipe3Count - 1);
        pipe3?.SetHighlight(pipe3Count > 0);

        if (iceCreamQueue.Count == 0) return;

        IceCream2_5 iceCream = iceCreamQueue.Dequeue();

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                if (inlet != null)
                    iceCream.transform.position = inlet.position;
                iceCream.Drop();
                break;
            case JudgementResult.Miss:
                Destroy(iceCream.gameObject);
                break;
        }
    }

    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim();

        // 입력 타이밍 시그널(Input)이 들어올 때 자동 시뮬레이션 실행
        if (actionType != "Input")
            return;

        if (ended)
            return;

        Debug.Log($"[2-5 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}");

        // 데모 모드에서는 시스템이 직접 입력을 받은 것처럼 처리하여 아이스크림 투입 및 판정 실행
        if (rhythmManager != null)
        {
            rhythmManager.ReceivePlayerInput("Input");
        }
        else
        {
            // fallback 처리
            OnJudgement(JudgementResult.Perfect);
        }
    }
}