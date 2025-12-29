using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlTrigger : MonoBehaviour
{
    public BowlController bowlController;

    private void Awake()
    {
        bowlController = GetComponentInParent<BowlController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ring"))
        {
            bowlController.curCount++;
            bowlController.OnRingEntered();
        }
    }
}
