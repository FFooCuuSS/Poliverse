using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kettle2_4 : MonoBehaviour
{
    [SerializeField] private Transform kettleTransform; // 회전시킬 주전자 오브젝트
    private bool isPouring = false;

    public void Pour()
    {
        if (isPouring) return; // 이미 기울어지는 중이면 중복 실행 방지
        isPouring = true;

        Sequence seq = DOTween.Sequence();

        // Z축을 기준으로 -40도 회전 (앞으로 숙이기)
        seq.Append(kettleTransform.DORotate(new Vector3(0, 0, -40), 0.15f).SetEase(Ease.OutQuad));

        // 물을 따르는 잠시동안의 대기 시간
        seq.AppendInterval(0.2f);

        // 다시 원래 각도(0도)로 복귀
        seq.Append(kettleTransform.DORotate(Vector3.zero, 0.15f).SetEase(Ease.InQuad));

        // 애니메이션 끝나면 플래그 해제
        seq.OnComplete(() => isPouring = false);
    }
}
