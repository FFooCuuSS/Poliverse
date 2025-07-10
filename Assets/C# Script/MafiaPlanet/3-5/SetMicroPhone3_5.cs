using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMicroPhone3_5 : MonoBehaviour
{
    int attatchCnt=7;

    private void Update()
    {
        if (attatchCnt <= 0)
        {
            Debug.Log("success");
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            Debug.Log("collided");
            if (Input.GetMouseButton(0))
            {
                var checkFalse = collision.collider.GetComponent<CheckFalse3_5>();
                if (checkFalse != null && checkFalse.isSpyers != true) 
                {
                    Debug.Log("attatched");
                    checkFalse.isSpyers = true;
                    attatchCnt--;
                }
                
                
            }
        }
    }
}
