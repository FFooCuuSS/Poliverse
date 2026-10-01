using UnityEngine;

public class Button_3_1 : MonoBehaviour
{
    public Transform startTarget;
    public Transform endTarget;
    public int segmentCount = 10;
    public float sagAmount = 0.1f;

    [Header("조건")]
    public float activateAngleThreshold = 45f;
    public float activateDistanceThreshold = 0.2f;
    public PistolDrag linkedPistol;

    private LineRenderer line;
    private Vector2 initialDirection;
    private Vector3 initialEndPosition;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.startWidth = 1f;
        line.endWidth = 1f;
        if (startTarget != null && endTarget != null)
        {
            initialDirection = (endTarget.position - startTarget.position).normalized;
            initialEndPosition = endTarget.position;
        }
    }

    void LateUpdate()
    {
        if (startTarget == null || endTarget == null) return;

        Vector3 p0 = startTarget.position;
        Vector3 p2 = endTarget.position;
        Vector3 p1 = (p0 + p2) / 2f + Vector3.down * sagAmount;

        line.positionCount = segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector3 point = Mathf.Pow(1 - t, 2) * p0 +
                            2 * (1 - t) * t * p1 +
                            Mathf.Pow(t, 2) * p2;
            line.SetPosition(i, point);
        }

        // 현재 줄 방향과 거리 계산
        Vector2 currentDir = (endTarget.position - startTarget.position).normalized;
        float angleDiff = Vector2.Angle(initialDirection, currentDir);
        float displacement = Vector3.Distance(endTarget.position, initialEndPosition);

        if (linkedPistol != null)
        {
            // 데모 모드일 때는 물리 계산에 의해 canPull이 꺼지지 않도록 고정
            if (Minigame_3_1.Instance != null && Minigame_3_1.Instance.IsDemoMode)
            {
                linkedPistol.canPull = true;
            }
            else
            {
                linkedPistol.canPull = angleDiff >= activateAngleThreshold &&
                                       displacement >= activateDistanceThreshold;
            }
        }
    }
}
