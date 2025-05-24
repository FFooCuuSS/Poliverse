using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    protected bool isDragging = false;
    protected Vector3 offset;

    protected virtual void OnMouseDown()
    {
        isDragging = true;
        Vector3 mouseWorldPos = GetMouseWorldPos();
        offset = transform.position - mouseWorldPos;
    }

    protected virtual void OnMouseUp()
    {
        isDragging = false;
    }

    protected virtual void Update()
    {
        if (!isDragging) return;

        Vector3 mouseWorldPos = GetMouseWorldPos();
        Vector3 targetPos = mouseWorldPos + offset;
        transform.position = GetConstrainedPosition(transform.position, targetPos);
    }

    protected virtual Vector3 GetConstrainedPosition(Vector3 current, Vector3 target)
    {
        return target; // 기본: 아무 제약 없음
    }

    protected Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}
