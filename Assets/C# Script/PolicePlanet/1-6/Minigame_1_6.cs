using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_6 : MiniGameBase
{
    protected override float TimerDuration => 15f;
    protected override string MinigameTitle => "배치하라!";

    protected override string MinigameExplain => "잠시 기다려 플랫폼 위치를 봐주세요.";

    public GameObject manager_1_6;
    Success_1_6 success_1_6;

    private void Start()
    {
        success_1_6 = manager_1_6.GetComponent<Success_1_6>();
    }

    public override void StartGame()
    {
        

        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
    }

    public void Succeed()
    {
        Success();
    }
    public override void Fail()
    {
        base.Fail();
        success_1_6.ApplyFailureSprites();
    }
}
