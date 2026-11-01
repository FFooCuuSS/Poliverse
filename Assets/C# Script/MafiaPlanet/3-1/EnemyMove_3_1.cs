using UnityEngine;
using DG.Tweening;

// 3-1 상대 연출. 이 스크립트가 붙은 오브젝트 = 상대 단추 끈 끝.
// 기존엔 Start()에서 자동으로 움직였지만, 이제 CSV의 CallTap / CallSwipe 박자에 맞춰 움직인다.
public class EnemyMove_3_1 : MonoBehaviour
{
    public Transform targetPos;
    public GameObject pistolObject;

    [Tooltip("(구버전) Start 자동 재생 시 이동 시간")]
    public float moveDuration = 1.5f;
    public float pistolAngle = 4.2f;
    public float pistolMoveDistance = 1.0f;

    [Header("Call 연출 시간 (박자 간격보다 짧게)")]
    public float unbuttonDuration = 0.2f;
    public float drawDuration = 0.5f;

    [Tooltip("켜면 예전처럼 Start에서 자동 재생 (리듬 동기화 안 됨)")]
    public bool autoPlayOnStart = false;

    [Header("라인 연결")]
    public Transform startTarget;
    public Transform endTarget;
    public int segmentCount = 10;
    public float sagAmount = 0.1f;

    private LineRenderer line;
    private bool unbuttoned;
    private bool drawn;

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
        if (!autoPlayOnStart) return;

        transform.DOMove(targetPos.position, moveDuration)
                 .SetEase(Ease.InOutSine)
                 .OnComplete(() => MovePistolObject(moveDuration));
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
            point.z = 0f;
            line.SetPosition(i, point);
        }
    }

    // CallTap: 단추 떼기
    public void PlayUnbutton()
    {
        if (unbuttoned || targetPos == null) return;
        unbuttoned = true;

        transform.DOKill();
        transform.DOMove(targetPos.position, unbuttonDuration)
                 .SetEase(Ease.OutQuad);
    }

    // CallSwipe: 총 뽑기 (단추를 아직 안 뗐으면 바로 떼고 뽑는다)
    public void PlayDraw()
    {
        if (drawn) return;
        drawn = true;

        if (!unbuttoned && targetPos != null)
        {
            unbuttoned = true;
            transform.DOKill();
            transform.position = targetPos.position;
        }

        MovePistolObject(drawDuration);
    }

    void MovePistolObject(float duration)
    {
        if (pistolObject == null) return;

        float rad = (pistolAngle + 90f) * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f).normalized;
        Vector3 target = pistolObject.transform.position + direction * pistolMoveDistance;

        pistolObject.transform.DOKill();
        pistolObject.transform.DOMove(target, duration)
                              .SetEase(Ease.OutQuad);
    }
}