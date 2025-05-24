using UnityEngine;

public class CircleRotator : MonoBehaviour
{
    public Transform pivot; // 회전 중심점
    private bool isDragging = false;
    private Vector3 lastMouseDir;

    void OnMouseDown()
    {
        isDragging = true;
        lastMouseDir = GetMouseDirection();
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (!isDragging) return;

        if (IsMouseObstructedByWall())
        {
            Debug.Log("마우스 경로에 벽 있음 → 회전 차단");
            return;
        }

        Vector3 currentDir = GetMouseDirection();
        float angle = Vector3.SignedAngle(lastMouseDir, currentDir, Vector3.forward);
        transform.RotateAround(pivot.position, Vector3.forward, angle);
        lastMouseDir = currentDir;
    }
    bool IsMouseObstructedByWall()
    {
        Vector2 origin = pivot.position;
        Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(origin, target - origin, Vector2.Distance(origin, target));
        return hit.collider != null && hit.collider.CompareTag("Wall");
    }

    Vector3 GetMouseDirection()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        return (mouseWorld - pivot.position).normalized;
    }
}
