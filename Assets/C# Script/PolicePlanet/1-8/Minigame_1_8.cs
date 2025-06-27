using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_8 : MiniGameBase
{
    public GameObject manager;
    private Manager_1_8 manager_1_8;

    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "Get in!";

    private void Start()
    {
        manager_1_8 = manager.GetComponent<Manager_1_8>();
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
        manager_1_8.DestroyAllPrisoners();
        base.Fail();
    }
}
