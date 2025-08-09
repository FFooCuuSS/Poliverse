using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_3_12 : MonoBehaviour
{
    public GameObject stage_3_12;
    private GameObject enemySight;

    private Minigame_3_12 minigame_3_12; 


    void Start()
    {
        minigame_3_12 = stage_3_12.GetComponent<Minigame_3_12>();
        enemySight = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
