using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_15 : MiniGameBase
{
    protected override float TimerDuration => 500f;
    protected override string MinigameExplain => "무기를 파괴하세요!";

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
