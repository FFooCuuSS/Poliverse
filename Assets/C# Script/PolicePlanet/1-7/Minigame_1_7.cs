using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_7 : MiniGameBase
{
    public GameObject ProhibitSpawner;
    private ProhibitedItemSpawner1_7 prohibitedItemSpawner1_7;

    private bool hasMissed = false;

    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "금지야!";


    private void Start()
    {
        prohibitedItemSpawner1_7 = ProhibitSpawner.GetComponent<ProhibitedItemSpawner1_7>();
    }

    public override void StartGame()
    {
        hasMissed = false;

        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
    }

    public override void Success()
    {
        base.Success();
        
    }

    public override void Fail()
    {
        base.Fail();
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
            GameManager1_7.instance.IncreaseSuccessCount();
        }
        else
        {
            OnMiss();
        }
    }

    public void OnMiss()
    {
        if (hasMissed) return;

        hasMissed = true;
        Debug.Log("미스 발생 -> 실패");

        Fail();
    }
}
