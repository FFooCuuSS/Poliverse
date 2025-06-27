using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_5 : MiniGameBase
{
    public GameObject CameraManager;
    private CameraManager1_5 cameraManager1_5;

    protected override float TimerDuration => 8f;
    protected override string MinigameExplain => "Find them!";

    private void Start()
    {
        cameraManager1_5 = CameraManager.GetComponent<CameraManager1_5>();
    }

    public override void StartGame()
    {
        

        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
    }

    public void Succeed()
    {
        cameraManager1_5.isMoving = false;
        //cameraManager1_5.mainCam.transform.position = new Vector3(0f, 0f, -10f);
        Success();
    }
    public override void Fail()
    {
        cameraManager1_5.isMoving = false;
        //cameraManager1_5.mainCam.transform.position = new Vector3(0f, 0f, -10f);
        base.Fail();
    }
}
