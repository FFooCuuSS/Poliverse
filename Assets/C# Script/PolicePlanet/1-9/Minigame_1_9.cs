using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_9 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "가동시켜라!";

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
}
