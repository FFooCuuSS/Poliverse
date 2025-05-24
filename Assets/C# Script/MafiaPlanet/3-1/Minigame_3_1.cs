using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_1 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "총을 뽑으세요!";

    public override void StartGame()
    {

        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
    }

    public void Succeed()
    {
        Success();
    }
}
