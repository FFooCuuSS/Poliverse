using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class SpawnedHandControler : MonoBehaviour
{
    public Minigame1_6_Manager_remake minigameManager1_6;

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("trigger with: " + collision.tag);
        if (collision.gameObject.tag == "Enemy")
        {

            minigameManager1_6.collideCnt++;
            Debug.Log("collideCnt"+minigameManager1_6.collideCnt);
        }
    }
    
}
