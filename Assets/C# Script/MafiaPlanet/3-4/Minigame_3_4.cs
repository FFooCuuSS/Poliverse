using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_4 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "ī�带 ������";

    public override void StartGame()
    {

        // �߰� �ʱ�ȭ
        // ��: instructionText.text = MinigameExplain;
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
