using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_15 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "�Ծ��!";

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
