using UnityEngine;

public class DirectionalDrag : DragAndDrop
{
    [Tooltip("�巡�� ���� ����/ ex) 0�� ����, 90�� ����, 45�� �밢��")]
    [Range(-180f, 180f)]
    public float angleInDegrees = 0f;

    protected override Vector3 GetConstrainedPosition(Vector3 current, Vector3 target)
    {
        float angleRad = (angleInDegrees + 90f) * Mathf.Deg2Rad;  // �� ���⸸ ������
        Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;

        Vector3 delta = target - current;
        float projection = Vector2.Dot(delta, dir);
        Vector3 constrainedDelta = new Vector3(dir.x, dir.y, 0f) * projection;

        return current + constrainedDelta;
    }
}
