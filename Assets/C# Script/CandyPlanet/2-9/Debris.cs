using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float destroyY = -3f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bowl"))
        {
            Bowl bowl = collision.GetComponent<Bowl>();
            if (bowl != null)
            {
                bowl.OnDebrisCaught();
            }

            Destroy(gameObject);
        }
    }
}