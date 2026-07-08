using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_3 : MiniGameBase
{
    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 1f;

    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "가동시켜라!";

    [SerializeField]
    private PendulamHammer2_3 hammer;

    private int inputCount = 0;

    private bool inputOpen = false;

    public bool IsInputTiming { get; private set; }

    public bool IsInputOpen => inputOpen;

    public override void StartGame()
    {
        base.StartGame();

        inputCount = 0;

        inputOpen = false;
        IsInputTiming = false;
    }

    public override void OnRhythmEvent(string action)
    {
        if (string.IsNullOrEmpty(action))
            return;


        Debug.Log($"{gameObject.name} 리듬메세지: {action}");


        action = action.Trim();


        switch (action)
        {
            case "Show":
                hammer.Swing();
                break;
            case "Input":
                inputOpen = true;
                break;
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        if (!inputOpen)
            return;

        inputOpen = false;

        rhythmManager?.ReceivePlayerInput("Input");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                Debug.Log("성공");
                break;
            case JudgementResult.Miss:
                Debug.Log("실패");
                break;
        }
    }
}
