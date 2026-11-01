using UnityEngine;

public class Minigame_3_14 : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 14f;
    protected override string MinigameTitle => "벽타기";
    protected override string MinigameExplain => "박자에 맞춰 다음 창문으로 넘어가세요.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "창문 불이 꺼지면 타이밍에 맞춰 버튼을 터치하세요."
    };

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    // 3라운드는 Input이 0.6초 간격이라 hitWindow를 반 박(0.3)으로 맞춰 노트 범위가 겹치지 않게 함
    public override float hitWindowOverride => 0.3f;

    [Header("Refs")]
    [SerializeField] private Manager_3_14 manager;

    private bool ended;
    private bool inputOpen;
    private bool awaitingJudge;

    private bool isDemoMode = false;
    public new bool IsDemoMode => isDemoMode;

    public override void SetDemoMode(bool isDemo)
    {
        base.SetDemoMode(isDemo);
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

        // 같은 입력이 판정 도중 다시 들어오는 것만 막는다.
        // 노트 시각 전(inputOpen 전)이라도 박자 범위 안이면 RhythmManager가 판정한다.
        if (awaitingJudge) return;

        awaitingJudge = true;
        Debug.Log($"[3-14 Judge] 입력 song={(rhythmManager is RhythmManager rm && rm.IsRunning ? rm.SongTime : -1):F3}");
        OnPlayerInput(action);   // 판정되면 OnJudgement가 동기로 호출됨

        // 판정 가능한 노트가 없었으면(박자 밖) 그냥 무시
        awaitingJudge = false;
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