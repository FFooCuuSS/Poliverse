using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMicroPhone3_5 : MonoBehaviour
{
    int attatchCnt;
    public GameObject Stage_3_5;
    GameObject EnemySpawnObj;
    Minigame_3_5 minigame_3_5;

    private void Start()
    {
        EnemySpawnObj = GameObject.Find("EnemySpawn");
        attatchCnt = EnemySpawnObj.GetComponent<EnemySpawner_3_5>().spawnCount;
        minigame_3_5 = Stage_3_5.GetComponent<Minigame_3_5>();

    }
    private void Update()
    {
        if (attatchCnt <= 0)
        {
            //Debug.Log("success");
            minigame_3_5.Succeed();
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            //Debug.Log("collided");
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
