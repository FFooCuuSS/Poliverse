using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_14 : MiniGameBase
{
    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "건너가세요!";

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
