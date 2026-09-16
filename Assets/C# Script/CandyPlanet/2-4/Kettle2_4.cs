using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kettle2_4 : MonoBehaviour
{
    [SerializeField] private Transform kettleTransform; 
    [SerializeField] private Transform pourPoint;

    public Transform PourPoint => pourPoint;

    private bool isPouring = false;

    public void Pour()
    {
        if (isPouring) return;
        isPouring = true;

        Vector3 originalRotation = kettleTransform.eulerAngles;

        Sequence seq = DOTween.Sequence();

        seq.Append(kettleTransform.DORotate(new Vector3(originalRotation.x, originalRotation.y, 40f), 0.15f).SetEase(Ease.OutQuad)); seq.AppendInterval(0.2f);
        seq.AppendInterval(0.2f);
        seq.Append( kettleTransform.DORotate( originalRotation, 0.15f ).SetEase(Ease.InQuad) );
        seq.OnComplete(() => isPouring = false);
    }
}
