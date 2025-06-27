using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_1 : MiniGameBase
{
    public GameObject CameraManager;
    private CameraController1_1 cameraController1_1;

    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "Find!";

    private void Start()
    {
        cameraController1_1 = CameraManager.GetComponent<CameraController1_1>();
    }

    public override void StartGame()
    {


        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
    }

    public override void Success()
    {
        cameraController1_1.isMoving = false;
        //cameraController1_1.mainCam.transform.position = new Vector3(0f, 0f, -10f);
        base.Success();
    }

    public override void Fail()
    {
        cameraController1_1.isMoving = false;
        //cameraController1_1.mainCam.transform.position = new Vector3(0f, 0f, -10f);
        base.Fail();
    }
}
