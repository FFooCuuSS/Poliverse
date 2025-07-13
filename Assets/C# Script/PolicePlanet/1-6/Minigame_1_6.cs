using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_6 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "��ġ�ض�!";

    public GameObject manager_1_6;
    Success_1_6 success_1_6;

    private void Start()
    {
        success_1_6 = manager_1_6.GetComponent<Success_1_6>();
    }

    public override void StartGame()
    {
        

        // �߰� �ʱ�ȭ
        // ��: instructionText.text = MinigameExplain;
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
