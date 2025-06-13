using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCream2_5 : MonoBehaviour
{
    public float fallSpeed = 5f; // ¶³¾îÁö´Â ¼Óµµ
    private bool isStopped = false;
    
    

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

        }
    }
}
