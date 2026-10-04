using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;

public class DropCake : MonoBehaviour
{
    public int processedCount = 0;

    [Header("È¿°úÀ½")]
    [SerializeField] private AudioClip dropSound;

    [SerializeField] private float targetY;
    [SerializeField] private Transform cake;

    public void MoveDownAndBack(float duration)
    {
        processedCount++;
        if (dropSound != null)
        {
            GameRoot.Instance.Audio.PlaySfx(dropSound);
        }



        float startY = cake.position.y;

        cake.DOMoveY(targetY, duration)
            .OnComplete(() =>
            {
                cake.DOMoveY(startY, duration);
            });
    }

}