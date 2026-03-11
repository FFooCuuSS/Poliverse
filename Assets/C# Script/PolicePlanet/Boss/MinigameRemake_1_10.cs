using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameRemake_1_10 : MiniGameBase
{
    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "분류해라!";

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;

    [Header("Refs")]
    [SerializeField] private Manager_1_10 manager;

    private bool ended;
    private bool inputOpen;
    private bool awaitingJudge;

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        inputOpen = false;
        awaitingJudge = false;

        if (manager != null)
            manager.OnMinigameStart(this);
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
                manager?.SpawnPersonForShow();
                break;

            case "Input":
                inputOpen = true;
                awaitingJudge = false;
                manager?.OnInputWindowOpened();
                break;

            case "Move":
                inputOpen = false;
                awaitingJudge = false;
                manager?.MoveBothPlatforms();
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
}