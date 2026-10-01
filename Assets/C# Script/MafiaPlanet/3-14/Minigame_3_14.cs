using UnityEngine;

public class Minigame_3_14 : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 14f;
    protected override string MinigameTitle => "젤리 블록 철거!";
    protected override string MinigameExplain => "죄수가 순서대로 나타나 타이밍을 알려줍니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "배경이 어두워지면 타이밍에 맞게 죄수를 터치해주세요."
    };

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;

    [Header("Refs")]
    [SerializeField] private Manager_3_14 manager;

    private bool ended;
    private bool inputOpen;
    private bool awaitingJudge;

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[3-14] SetDemoMode = {isDemoMode}");
    }

    // --- 데모 모드 (IPracticeDemoInput) ---
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(actionType)) return;

        actionType = actionType.Trim();
        Debug.Log($"[3-14 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}");

        switch (actionType)
        {
            case "Show":
                inputOpen = false;
                awaitingJudge = false;
                manager?.ShowNextCall();
                break;

            case "Input":
                inputOpen = true;
                awaitingJudge = false;
                manager?.OnInputWindowOpened();

                // 데모 모드일 때 이미 판정 대기 중이 아니라면 딱 1번만 입력 실행
                if (!awaitingJudge)
                {
                    SubmitPlayerInput("Input");
                }
                break;

            case "Move":
                inputOpen = false;
                awaitingJudge = false;
                manager?.OnMoveSignal();
                break;

            case "End":
                inputOpen = false;
                awaitingJudge = false;
                manager?.OnEndSignal();
                break;
        }
    }

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        inputOpen = false;
        awaitingJudge = false;

        manager?.OnMinigameStart(this);
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        action = action.Trim();

        // 데모 모드일 때는 리듬 이벤트로 인한 입력 처리를 무시하고 ExecutePracticeAction에 전적으로 맡김
        if (isDemoMode && action == "Input") return;

        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        switch (action)
        {
            case "Show":
                inputOpen = false;
                awaitingJudge = false;
                manager?.ShowNextCall();
                break;

            case "Input":
                inputOpen = true;
                awaitingJudge = false;
                manager?.OnInputWindowOpened();
                break;

            case "Move":
                inputOpen = false;
                awaitingJudge = false;
                manager?.OnMoveSignal();
                break;

            case "End":
                inputOpen = false;
                awaitingJudge = false;
                manager?.OnEndSignal();
                break;
        }
    }

    public void SubmitPlayerInput(string action = "Input")
    {
        if (ended) return;

        // 데모 모드가 아닐 때의 입력 잠금 체크
        if (!isDemoMode)
        {
            if (!inputOpen) return;
            if (awaitingJudge) return;
        }
        else
        {
            // 데모 모드에서는 이미 판정 대기 중이면 중복 실행 차단
            if (awaitingJudge) return;
        }

        awaitingJudge = true;
        OnPlayerInput(action);
    }

    // 판정 처리
    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended) return;

        base.OnJudgement(judgement);
        awaitingJudge = false; // 판정이 끝났으므로 대기 상태 해제

        Debug.Log($"[3-14 Judgement] {judgement}");

        switch (judgement)
        {
            case JudgementResult.Miss:
                manager?.OnMiss();
                break;

            case JudgementResult.Good:
            case JudgementResult.Perfect:
                manager?.OnAccepted(judgement);
                break;
        }
    }
}