using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_6 : MiniGameBase
{
    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "배치해라!";

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
