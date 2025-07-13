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

    // �ܺο��� ȣ�� ������ ���� �Լ�
    public void StopMoving()
    {
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill(); // Ʈ�� ���� �� ����
        }

        rect.anchoredPosition = originalPos; // ���� ��ġ�� ����
    }
}
