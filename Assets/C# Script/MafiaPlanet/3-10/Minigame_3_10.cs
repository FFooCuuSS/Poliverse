using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_10 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "자동차를 이동하세요";

    public override void StartGame()
    {

        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
    }

    public void Succeed()
    {
        base.Success();
    }
    public void MinigameFailed()
    {
        base.Fail();
    }
}
