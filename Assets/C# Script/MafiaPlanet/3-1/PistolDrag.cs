using UnityEngine;

public class PistolDrag : DirectionalDrag
{
    public bool canPull = false;
    public float minY = -999f;
    public float maxWhenBlocked = 0.2f;
    public float maxWhenReleased = 1.0f;

    [Header("상승 시작 조건")]
    public float autoRiseThresholdY = 0.5f; // y값 기준

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

        // 최소 Y 제한
        newPos.y = Mathf.Max(newPos.y, minY);

        // 조건 만족 시 신호 + 자기 자신 꺼짐
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
