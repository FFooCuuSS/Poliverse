using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_10 : MiniGameBase
{
    protected override float TimerDuration => 20f;
    protected override string MinigameExplain => "Left or Rigjt!";

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
