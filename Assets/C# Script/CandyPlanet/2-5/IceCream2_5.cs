using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCream2_5 : MonoBehaviour
{
    public float fallSpeed = 10f; // ¶³¾îÁö´Â ¼Óµµ
    private bool isStopped = false;
    public GameObject stage_2_5;
    private Minigame_2_5 minigame_2_5;

    void Awake()
    {
        
        minigame_2_5 = FindAnyObjectByType<Minigame_2_5>();

    }
    void Update()
    {
        if (isStopped)
        {

            
        }
    }

    // ¹Ù±¸´Ï¿¡ ´êÀ¸¸é ¸ØÃã
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Basket"))
        {
            isStopped = true;
            
        }
        if(other.CompareTag("Floor"))
        {
            minigame_2_5.Failure();
            
        }
    }
}
