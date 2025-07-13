using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class UpDownMove : MonoBehaviour
{
    [SerializeField] private float moveDistance = 20f;
    [SerializeField] private float moveDuration = 1f;

    private RectTransform rect;
    private Vector2 originalPos;
    private Tween moveTween;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;

        Vector2 endPos = originalPos + Vector2.up * moveDistance;

        moveTween = rect.DOAnchorPosY(endPos.y, moveDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // 외부에서 호출 가능한 정지 함수
    public void StopMoving()
    {
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill(); // 트윈 정지 및 제거
        }

        rect.anchoredPosition = originalPos; // 원래 위치로 복귀
    }
}
