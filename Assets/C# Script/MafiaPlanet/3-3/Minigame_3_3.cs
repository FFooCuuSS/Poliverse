using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_3 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "열쇠를 끼우세요!";

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
