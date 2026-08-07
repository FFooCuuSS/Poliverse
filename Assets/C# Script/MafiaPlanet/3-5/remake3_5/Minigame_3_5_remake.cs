using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Minigame_3_5_remake : MiniGameBase
{
    public ScopeMover scopeMover;
   // public CameraFollow3_5 cameraFollow;
    bool moveStarted = false;

    private void Start()
    {
        StartGame();
        moveStarted = true;
    }
    public override void StartGame()
    {
        Debug.Log("[3-4] StartGame called");
        base.StartGame();

    }
    float timer = 0f;
    float timechecker = 0f; 

    void Update()
    {
        timer += Time.deltaTime;
        timechecker += Time.deltaTime;
        if (timer >= 0.5f) // 0.5초마다 출력
        {
            //Debug.Log("현재 시간: " + Time.time+"timechecker: "+timechecker);
            timer = 0f;
        }
        if(timechecker>=3.5f&&moveStarted)
        {

            scopeMover.StartMove();
            moveStarted = false; 
        }
        if(timechecker>=3f)
        {
            //cameraFollow.ResetCamera();
        }
    }
    
}
