using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_15 : MiniGameBase
{
    protected override float TimerDuration => 500f;
    protected override string MinigameExplain => "���⸦ �ı��ϼ���!";

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
