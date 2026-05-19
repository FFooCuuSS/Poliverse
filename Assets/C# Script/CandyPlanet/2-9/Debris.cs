using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float destroyY = -3f;
    public int debrisCount = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bowl"))
        {
            Destroy(gameObject);
            debrisCount++;
        }
    }
}
