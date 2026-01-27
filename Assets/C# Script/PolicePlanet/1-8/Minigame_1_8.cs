using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_8 : MiniGameBase
{
    public GameObject manager;
    private Manager_1_8 manager_1_8;

    private bool hasMissed = false;

    private bool canTap = false;
    private PrisonController_1_8 prisonController;


    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "모두 가둬라!";

    private void Start()
    {
        manager_1_8 = manager.GetComponent<Manager_1_8>();
        prisonController = manager_1_8.prisonObj.GetComponent<PrisonController_1_8>();
    }

    public override void StartGame()
    {
        hasMissed = false;

    // 추가 초기화
    // 예: instructionText.text = MinigameExplain;
    }

    public override void Fail()
    {
        if (manager_1_8.hasAnySuccess)
        {
            Debug.Log("게임 성공");
            Success();
            return;
        }

        Debug.Log("전부 실패 → 게임 실패");
        manager_1_8.DestroyAllPrisoners();
        base.Fail();
    }

    public bool CanTap => canTap;

    public void UseTap()
    {
        canTap = false;
    }

    public override void OnRhythmEvent(string action)
    {
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        // 이건 나중에 개별 미니게임에서 override하는 형태로
        switch (action)
        {
            case "Tap":
                canTap = true;
                Debug.Log("Tap");
                manager_1_8.SpawnPrisoner();
                //ShowTapPrompt();
                break;

            case "Hold":
                Debug.Log("Hold");
                manager_1_8.SpawnPrisoner();
                //ShowHoldPrompt();
                break;

            case "Swipe":
                Debug.Log("Swipe");
                //ShowSwipePrompt();
                break;
        }
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (hasMissed) return;

        if (judgement == JudgementResult.Perfect ||
            judgement == JudgementResult.Good)
        {
            Debug.Log("판정 성공 → 감옥 작동");
            prisonController.ActivatePrison();
        }
        else
        {
            //OnMiss();
            //Fail();
        }
    }

    //public void OnMiss()
    //{
    //    if (hasMissed) return;

    //    hasMissed = true;
    //    Debug.Log("미스 발생 -> 실패");

    //    Fail();
    //}
}
