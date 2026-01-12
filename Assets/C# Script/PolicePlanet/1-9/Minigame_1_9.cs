using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Minigame_1_9 : MiniGameBase
{
    private bool hasMissed = false;

    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "가동시켜라!";

    private int touchCount = 0;
    private const int targetTouchCount = 3;

    public override void StartGame()
    {
        hasMissed = false;
    }

    public void Succeed()
    {
        Success();
    }
    public void Failure()
    {
        Fail();
    }


    public override void OnRhythmEvent(string action)
    {
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        // 이건 나중에 개별 미니게임에서 override하는 형태로
        switch (action)
        {
            case "Tap":
                Debug.Log("Tap");
                //ShowTapPrompt();
                break;

            case "Hold":
                Debug.Log("Hold");
                //ShowHoldPrompt();
                break;

            case "Swipe":
                Debug.Log("Swipe");
                //ShowSwipePrompt();
                break;
        }
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (hasMissed) return;

        if (judgement == JudgementResult.Perfect ||
            judgement == JudgementResult.Good)
        {
            Debug.Log("리듬 성공 → 미니게임 성공");
            Success();
        }
        else
        {
            //OnMiss();
        }
    }

    //public void OnMiss()
    //{
    //    if (hasMissed) return;

    //    hasMissed = true;
    //    Debug.Log("미스 발생 -> 실패");

    //    Fail();
    //}
}
