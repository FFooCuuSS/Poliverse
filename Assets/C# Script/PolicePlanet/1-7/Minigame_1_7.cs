using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_7 : MiniGameBase
{
    public GameObject ProhibitSpawner;
    private ProhibitedItemSpawner1_7 prohibitedItemSpawner1_7;


    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "������!";


    private void Start()
    {
        prohibitedItemSpawner1_7 = ProhibitSpawner.GetComponent<ProhibitedItemSpawner1_7>();
    }

    public override void StartGame()
    {
        

        // �߰� �ʱ�ȭ
        // ��: instructionText.text = MinigameExplain;
    }

    public override void Success()
    {
        base.Success();
    }

    public override void Fail()
    {
        base.Fail();
    }
}
