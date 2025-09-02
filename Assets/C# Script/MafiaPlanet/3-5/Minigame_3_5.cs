using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_5 : MiniGameBase 
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "도청장치를 설치하세요";
    public Shoot3_5 shoot;

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
