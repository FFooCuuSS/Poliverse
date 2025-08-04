using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_10 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "저어라!";

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
