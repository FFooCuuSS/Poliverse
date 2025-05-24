using UnityEngine;

public class DirectionalDrag : DragAndDrop
{
    [Tooltip("드래그 제한 방향/ ex) 0은 수평, 90은 수직, 45는 대각선")]
    [Range(-180f, 180f)]
    public float angleInDegrees = 0f;

    protected override Vector3 GetConstrainedPosition(Vector3 current, Vector3 target)
    {
        float angleRad = (angleInDegrees + 90f) * Mathf.Deg2Rad;  // ← 여기만 고쳤음
        Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;

        Vector3 delta = target - current;
        float projection = Vector2.Dot(delta, dir);
        Vector3 constrainedDelta = new Vector3(dir.x, dir.y, 0f) * projection;

        return current + constrainedDelta;
    }
}
