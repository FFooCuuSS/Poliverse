using UnityEngine;

public class DirectionalDrag : DragAndDrop
{
    [Tooltip("드래그 제한 방향/ ex) 0은 수평, 90은 수직, 45는 대각선")]
    [Range(-180f, 180f)]
    public float angleInDegrees = 0f;

    protected override Vector3 GetConstrainedPosition(Vector3 current, Vector3 target)
    {
        // 방향 제한 먼저 적용
        float angleRad = angleInDegrees * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;

        Vector3 delta = target - current;
        float projection = Vector2.Dot(delta, dir);
        Vector3 constrainedDelta = new Vector3(dir.x, dir.y, 0f) * projection;
        Vector3 directionallyConstrained = current + constrainedDelta;

        // 방향 제한된 결과를 다시 박스 제한에 넣어 최종값 계산
        return base.GetConstrainedPosition(current, directionallyConstrained);
    }
}
