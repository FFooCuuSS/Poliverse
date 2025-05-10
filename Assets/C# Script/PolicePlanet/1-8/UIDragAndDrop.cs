using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool IsDragging { get; private set; }

    private Vector2 offset;
    private RectTransform parentRect;

    void Awake()
    {
        parentRect = (RectTransform)transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        IsDragging = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out offset
        );
        offset = (Vector2)transform.localPosition - offset;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            Vector2 newPos = localPoint + offset;

            // 부모 영역 내로 위치 제한
            Vector2 halfSize = ((RectTransform)transform).sizeDelta * 0.5f;
            newPos.x = Mathf.Clamp(newPos.x, -parentRect.rect.width / 2 + halfSize.x, parentRect.rect.width / 2 - halfSize.x);
            newPos.y = Mathf.Clamp(newPos.y, -parentRect.rect.height / 2 + halfSize.y, parentRect.rect.height / 2 - halfSize.y);

            transform.localPosition = newPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;
    }
}
