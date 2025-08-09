using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Success_3_12 : MonoBehaviour
{
    public GameObject stage_3_12;
    private Minigame_3_12 minigame_3_12;

    private void Start()
    {
        minigame_3_12 = stage_3_12.GetComponent<Minigame_3_12>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            minigame_3_12.Succeed();
        }
    }
}
