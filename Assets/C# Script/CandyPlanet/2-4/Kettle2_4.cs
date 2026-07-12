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

        Sequence seq = DOTween.Sequence();

        seq.Append(kettleTransform.DORotate(new Vector3(0, 0, -40), 0.15f).SetEase(Ease.OutQuad));
        seq.AppendInterval(0.2f);
        seq.Append(kettleTransform.DORotate(Vector3.zero, 0.15f).SetEase(Ease.InQuad));

        seq.OnComplete(() => isPouring = false);
    }
}
