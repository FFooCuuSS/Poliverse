using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_10 : MiniGameBase
{
    protected override float TimerDuration => 20f;
    protected override string MinigameExplain => "Left or Rigjt!";

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
