using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_10 : MiniGameBase
{
    protected override float TimerDuration => 15f;
    protected override string MinigameTitle => "분류해라!";

    protected override string MinigameExplain => " 가운데서 경찰이나 죄수가 나옵니다. 1초 후 죄수면 왼쪽 경찰이면 오른쪽을 터치해주세요.";

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