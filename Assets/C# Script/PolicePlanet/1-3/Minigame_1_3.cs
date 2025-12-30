using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_3 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "조심해라!";

    public override void StartGame()
    {
        

        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
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
                break;

            case "Hold":
                //ShowHoldPrompt();
                break;

            case "Swipe":
                //ShowSwipePrompt();
                break;
        }
    }
}
