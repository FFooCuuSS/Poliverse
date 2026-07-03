using UnityEngine;

public class Minigame_3_14 : MiniGameBase
{
    protected override float TimerDuration => 14f;
    protected override string MinigameExplain => "건너가세요!";

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;

    [Header("Refs")]
    [SerializeField] private Manager_3_14 manager;

    private bool ended;
    private bool inputOpen;
    private bool awaitingJudge;

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
        if (!inputOpen) return;
        if (awaitingJudge) return;

        awaitingJudge = true;
        OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended) return;

        base.OnJudgement(judgement);
        awaitingJudge = false;

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