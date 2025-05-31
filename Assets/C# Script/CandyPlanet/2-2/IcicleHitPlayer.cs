using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcicleHitPlayer : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            Debug.Log("게임오버");
        }
        else if (coll.collider.tag == "Floor")
        {
            Debug.Log("바닥에 닿음");
            Destroy(gameObject, 0.2f);

        }
    }


    // 제한시간 추가
}