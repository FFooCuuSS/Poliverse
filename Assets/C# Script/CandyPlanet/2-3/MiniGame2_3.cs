using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_3 : MiniGameBase, IPracticeDemoInput
{
    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 1f;

    protected override float TimerDuration => 5f;

    protected override string MinigameTitle => "가동시켜라!";
    protected override string MinigameExplain => "죄수가 순서대로 나타나 타이밍을 알려줍니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "배경이 어두워지면 타이밍에 맞게 죄수를 터치해주세요."
    };

    [SerializeField] private PendulamHammer2_3 hammer;
    [SerializeField] private Player2_3 player;
    private bool isHammerAtRight = false;

    private bool inputOpen = false;

    public bool IsInputTiming { get; private set; }

    public bool IsInputOpen => inputOpen;

    private RhythmManager rhythmManagerRef;

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[2-3] SetDemoMode = {isDemoMode}");
    }

    private double GetSongTime()
    {
        if (rhythmManagerRef == null)
            rhythmManagerRef = FindObjectOfType<RhythmManager>();

        return rhythmManagerRef != null
            ? rhythmManagerRef.SongTime
            : -1;
    }

    public override void StartGame()
    {
        base.StartGame();

        inputOpen = false;
        IsInputTiming = false;

        isHammerAtRight = false;

        if (player != null)
        {
            player.UpdateDirectionByHammerPosition(isHammerAtRight);
        }
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
                if (player != null)
                {
                    player.UpdateDirectionByHammerPosition(isHammerAtRight);
                }

                hammer.Swing();

                isHammerAtRight = !isHammerAtRight;
                inputOpen = true;
                break;
            case "Input":

                break;
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        if (IsDemoMode) return;

        Debug.Log($"[Minigame_2_3] 클릭 수신 @ SongTime {GetSongTime():F3}, inputOpen={inputOpen}");

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
                if (player != null)
                {
                    player.SetJudgementResult(false);
                }
                break;
        }
    }

    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim();

        if (actionType != "Show" && actionType != "Input")
            return;

        Debug.Log($"[2-3 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}");

        inputOpen = true;

        if (player != null)
        {
            player.TriggerDemoAction();
        }

        if (rhythmManager != null)
        {
            rhythmManager.ReceivePlayerInput("Input");
        }
        else
        {
            OnJudgement(JudgementResult.Perfect);
        }
    }
}