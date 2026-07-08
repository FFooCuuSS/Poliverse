using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundInputController_2_11 : MonoBehaviour
{
    private Minigame_2_11 minigame;
    private Fork_2_11 fork;

    void Start()
    {
        minigame = FindObjectOfType<Minigame_2_11>();
        fork = FindObjectOfType<Fork_2_11>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("화면 클릭");


            minigame?.OnPlayerInput();


            fork?.GrabMacaron();
        }
    }
}
