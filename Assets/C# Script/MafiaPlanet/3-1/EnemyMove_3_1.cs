using UnityEngine;
using DG.Tweening;

public class EnemyMove_3_1 : MonoBehaviour
{
    public Transform targetPos;
    public GameObject pistolObject;
    public float moveDuration = 1.5f;
    public float pistolAngle = 4.2f;
    public float pistolMoveDistance = 1.0f;

    [Header("라인 연결")]
    public Transform startTarget;
    public Transform endTarget;
    public int segmentCount = 10;
    public float sagAmount = 0.1f;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        if (line != null)
        {
            line.startWidth = 1f;
            line.endWidth = 1f;
        }
    }

    void Start()
    {
        transform.DOMove(targetPos.position, moveDuration)
                 .SetEase(Ease.InOutSine)
                 .OnComplete(MovePistolObject);
    }

    void Update()
    {
        if (line == null || startTarget == null || endTarget == null) return;

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
            point.z = 0f; // z 좌표 고정
            line.SetPosition(i, point);
        }
    }

    void MovePistolObject()
    {
        if (pistolObject == null) return;

        float rad = (pistolAngle + 90f) * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f).normalized;
        Vector3 target = pistolObject.transform.position + direction * pistolMoveDistance;

        pistolObject.transform.DOMove(target, moveDuration)
                              .SetEase(Ease.OutSine);
    }
}
