using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class IcicleHitPlayer : MonoBehaviour
{
    private MiniGame2_2 minigame_2_2;
    public GameObject stage_2_2;

    private void Start()
    {
        minigame_2_2 = stage_2_2.GetComponent<MiniGame2_2>();
    }
    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Icicle")
        {
            minigame_2_2.Failure();
        }
    }


    // 제한시간 추가
}