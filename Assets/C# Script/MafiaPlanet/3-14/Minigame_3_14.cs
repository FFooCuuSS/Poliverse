using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_14 : MiniGameBase
{
    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "�ǳʰ�����!";

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
