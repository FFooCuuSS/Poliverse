using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_1 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "���� ��������!";

    public override void StartGame()
    {

        // �߰� �ʱ�ȭ
        // ��: instructionText.text = MinigameExplain;
    }

    public void Succeed()
    {
        Success();
    }
}
