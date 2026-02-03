using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;

public class DropCake : MonoBehaviour
{
    public int processedCount = 0;

    [SerializeField] private float targetY = 2.5f;
    [SerializeField] private Transform cake;

    public void MoveDownAndBack(float duration)
    {
        processedCount++;

        float startY = cake.position.y;

        cake.DOMoveY(targetY, duration)
            .OnComplete(() =>
            {
                cake.DOMoveY(startY, duration);
            });
    }

}