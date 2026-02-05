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

    private bool ended;
    private bool inputOpen;
    private bool awaitingJudge;
    private bool hasAnySuccess;

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        inputOpen = false;
        awaitingJudge = false;
        hasAnySuccess = false;
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        action = action.Trim();
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        switch (action)
        {
            case "Input":
                inputOpen = true;
                awaitingJudge = false;
                break;

            case "InputEnd":
                if (inputOpen && !awaitingJudge)
                {
                    OnJudgement(JudgementResult.Miss);
                }
                inputOpen = false;
                break;
        }
    }

    /// <summary>
    /// Player2_3에서 호출
    /// </summary>
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

        awaitingJudge = false;
        inputOpen = false;

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                Debug.Log("판정 성공");
                hasAnySuccess = true;
                break;

            case JudgementResult.Miss:
                Debug.Log("판정 실패 → 즉시 실패");
                ended = true;
                Fail();
                break;
        }
    }

    /// <summary>
    /// 리듬 차트 종료 시 호출
    /// </summary>
    public void FinalJudge()
    {
        if (ended) return;
        ended = true;

        if (hasAnySuccess)
            Success();
        else
            Fail();
    }
}
