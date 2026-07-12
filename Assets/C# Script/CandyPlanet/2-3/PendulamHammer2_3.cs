using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulamHammer2_3 : MonoBehaviour
{
    public float swingAngle = 45f;
    public float swingDuration = 1f;

    private bool goRight = true;

    void Start()
    {
        transform.localRotation = Quaternion.Euler(0, 0, -swingAngle);
        goRight = true;
    }

    public void Swing()
    {
        transform.DOKill();

        float targetAngle = goRight ? swingAngle : -swingAngle;
        goRight = !goRight;

        transform.DOLocalRotate(new Vector3(0, 0, targetAngle), swingDuration)
            .SetEase(Ease.InOutSine);
    }
}
