using UnityEngine;
using DG.Tweening;

public class PrisonController_1_8 : MonoBehaviour
{
    [Header("Delta Move")]
    public float downDeltaY = 3f;      
    public float downDuration = 0.12f; 
    public float staySeconds = 0.15f;  
    public float upDuration = 0.25f;   

    [Header("Tweens")]
    public float slamOvershoot = 0.25f;
    public float upBounce = 0.8f;

    private bool isMoving = false;
    public bool IsActive { get; private set; }

    private Vector3 basePos;
    private bool basePosCaptured = false;

    private Tween moveTween;

    public void ActivatePrison()
    {
        if (isMoving) return;

        if (!basePosCaptured)
        {
            basePos = transform.position;
            basePosCaptured = true;
        }

        PlaySlamAndReturn();
    }

    private void PlaySlamAndReturn()
    {
        isMoving = true;

        // 안전: 기존 트윈 제거
        moveTween?.Kill(false);

        Vector3 downPos = basePos + Vector3.down * downDeltaY;

        IsActive = true;

        moveTween = DOTween.Sequence()
            // 쾅 내려감
            .Append(transform.DOMove(downPos, downDuration)
                .SetEase(Ease.InBack, slamOvershoot))
            // 잠깐 대기
            .AppendInterval(staySeconds)
            // 올라올 때 살짝 바운스
            .AppendCallback(() => IsActive = false)
            .Append(transform.DOMove(basePos, upDuration)
                .SetEase(Ease.OutBack, upBounce))
            .OnComplete(() =>
            {
                isMoving = false;
                transform.position = basePos;
            });
    }

    private void OnDisable()
    {
        // 비활성화 시 정리
        moveTween?.Kill(false);
        moveTween = null;
        isMoving = false;
        IsActive = false;
        // basePos는 유지(원하면 reset 가능)
    }
}
