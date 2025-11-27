using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlController : MonoBehaviour
{
    public event Action OnAllReceived;

    int targetCount = 0;
    public int curCount = 0;

    public void SetTargetCount(int count)
    {
        targetCount = count;
    }

    public void OnRingEntered()
    {
        if (curCount == targetCount)
        {
            OnAllReceived?.Invoke();
        }
    }

}


