using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerCollision2_3 : MonoBehaviour
{
    private Minigame_2_3 minigame_2_3;


    private void Awake()
    {
        minigame_2_3 = FindAnyObjectByType<Minigame_2_3>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("...");
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("망치와 충돌하였습니다");
        }
    }

}
