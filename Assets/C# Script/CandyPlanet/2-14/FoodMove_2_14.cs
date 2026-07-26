using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodMove_2_14 : MonoBehaviour
{
    Tween moveTween;

    public void Init(Transform player, float shieldRadius, float moveTime)
    {
        Vector3 dir = (player.position - transform.position).normalized;

        // 방패 위치까지 이동
        Vector3 targetPos = player.position - dir * shieldRadius;

        moveTween = transform.DOMove(targetPos, moveTime)
                             .SetEase(Ease.Linear);
    }

    public void StopMovement()
    {
        moveTween?.Kill();
    }
}
