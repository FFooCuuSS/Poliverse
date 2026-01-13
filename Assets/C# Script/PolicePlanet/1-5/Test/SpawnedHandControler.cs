using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class SpawnedHandControler : MonoBehaviour
{
    public float speed = 3f;
    public Minigame1_6_ManagerTest minigameManager1_6;

    void Update()
    {
        // 아래 방향(-Y)으로 이동
       // transform.position += Vector3.down * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("trigger with: " + collision.tag);
        if (collision.gameObject.tag == "Enemy")
        {

            minigameManager1_6.collideCnt++;
            Debug.Log("collideCnt"+minigameManager1_6.collideCnt);
            speed = 0f;
        }
    }
    
}
