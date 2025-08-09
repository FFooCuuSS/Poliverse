using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerCollision2_3 : MonoBehaviour
{
    public GameObject stage_2_5;
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
            Debug.Log("��ġ�� �浹�Ͽ����ϴ�");
            minigame_2_3.Failure();
        }
    }

}
