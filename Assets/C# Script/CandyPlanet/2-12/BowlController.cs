using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlController : MonoBehaviour
{
    public event Action OnAllReceived;

    int targetCount = 0;
    int curCount = 0;

    public void SetTargetCount(int count)
    {
        targetCount = count;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ring"))
        {
            curCount++;
            if(curCount == targetCount)
            {
                OnAllReceived?.Invoke();
            }
        }
    }
}
