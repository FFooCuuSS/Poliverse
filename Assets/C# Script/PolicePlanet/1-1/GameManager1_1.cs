using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager1_1 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "Arrest!";

    public override void StartGame()
    {


        // �߰� �ʱ�ȭ
        // ��: instructionText.text = MinigameExplain;
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
