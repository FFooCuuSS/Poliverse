using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    [SerializeField] protected float maxX = 7f;
    [SerializeField] protected float maxY = 4f;

    public bool isDragging = false;
    public bool banDragging = false;
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
        if (!isDragging || banDragging) return;

        Vector3 mouseWorldPos = GetMouseWorldPos();
        Vector3 targetPos = mouseWorldPos + offset;
        transform.position = GetConstrainedPosition(transform.position, targetPos);
    }

    protected virtual Vector3 GetConstrainedPosition(Vector3 current, Vector3 target)
    {
        float clampedX = Mathf.Clamp(target.x, -maxX, maxX);
        float clampedY = Mathf.Clamp(target.y, -maxY, maxY);
        return new Vector3(clampedX, clampedY, target.z);
    }

    protected Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}
