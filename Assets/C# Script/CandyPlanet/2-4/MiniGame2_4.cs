using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame2_4 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "같은 색끼리 옮겨담아라!";

    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.45f;
    public override float hitWindowOverride => 0.6f;

    private bool ended;
    private bool inputOpen;
    public bool IsInputOpen => inputOpen;

    [SerializeField] private BottleSpawner2_4 spawner;
    [SerializeField] private Kettle2_4 kettle;

    private Bottle2_4 currentBottle;

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        inputOpen = false;
        currentBottle = null;
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        action = action.Trim();

        switch (action)
        {
            case "Show":
                spawner.SpawnBottle();
                break;
            case "Input":
                inputOpen = true;
                break;
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        if (ended) return;
        if (!inputOpen) return;

        inputOpen = false;

        kettle.Pour();

        rhythmManager?.ReceivePlayerInput("Input");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                currentBottle.FillBottle();
                break;

            case JudgementResult.Miss:
                break;
        }
    }
}
