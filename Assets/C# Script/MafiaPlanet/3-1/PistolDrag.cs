using UnityEngine;

public class PistolDrag : DirectionalDrag
{
    public bool canPull = false;
    public float minY = -999f;
    public float maxWhenBlocked = 0.2f;
    public float maxWhenReleased = 1.0f;

    [Header("��� ���� ����")]
    public float autoRiseThresholdY = 0.5f; // y�� ����

    protected override Vector3 GetConstrainedPosition(Vector3 current, Vector3 target)
    {
        if (!canPull)
            return current;

        float angleRad = (angleInDegrees + 90f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;

        Vector3 delta = target - current;
        float projection = Vector2.Dot(delta, dir);

        float maxProjection = maxWhenReleased;
        projection = Mathf.Clamp(projection, 0, maxProjection);

        Vector3 constrainedDelta = new Vector3(dir.x, dir.y, 0f) * projection;
        Vector3 newPos = current + constrainedDelta;

        // �ּ� Y ����
        newPos.y = Mathf.Max(newPos.y, minY);

        // ���� ���� �� ��ȣ + �ڱ� �ڽ� ����
        if (newPos.y >= autoRiseThresholdY)
        {
            PistolUp up = GetComponent<PistolUp>();
            if (up != null)
            {
                up.goingUp = true;
            }

            this.enabled = false;
        }

        return newPos;
    }
}
