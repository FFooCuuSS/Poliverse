using DG.Tweening;
using UnityEngine;

public class Rope : MonoBehaviour
{
    //public Transform target; // 참조할 타겟
    //private LineRenderer lineRenderer;

    //void Awake()
    //{
    //    lineRenderer = GetComponent<LineRenderer>();

    //    if (lineRenderer != null)
    //    {
    //        lineRenderer.positionCount = 2; // 시작점, 끝점
    //    }
    //}

    //void Update()
    //{
    //    if (target == null || lineRenderer == null)
    //        return;

    //    Vector3 myPosition = transform.position + new Vector3(0.1f, 0f, 0f);
    //    Vector3 targetPosition = target.position;

    //    lineRenderer.SetPosition(0, myPosition);
    //    lineRenderer.SetPosition(1, targetPosition);
    //}

    [Header("줄 시작점")]
    public Transform ropeStart;

    private LineRenderer lineRenderer;
    private Tween stretchTween;

    [Header("Tween 세팅")]
    public float stretchDuration = 0.2f;   // 늘어날 때 걸리는 시간
    public Ease stretchEase = Ease.OutQuad; // 자연스러운 Ease

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;

            // 인스펙터에서 지정한 색상 그대로 사용
            if (lineRenderer.startWidth == 0) lineRenderer.startWidth = 0.1f;
            if (lineRenderer.endWidth == 0) lineRenderer.endWidth = 0.1f;
        }
    }

    void Start()
    {
        if (lineRenderer != null && ropeStart != null)
        {
            // 초기 끝점은 시작점과 동일
            lineRenderer.SetPosition(0, ropeStart.position);
            lineRenderer.SetPosition(1, ropeStart.position);
        }
    }

    void Update()
    {
        if (lineRenderer == null || ropeStart == null) return;

        // 시작점 항상 갱신
        lineRenderer.SetPosition(0, ropeStart.position);

        // Tween 중이 아니면 끝점 현재 위치 유지
        if (stretchTween == null || !stretchTween.IsActive() || !stretchTween.IsPlaying())
        {
            lineRenderer.SetPosition(1, transform.position);
        }
    }

    public void PlayStretch(Vector3 stretchOffset)
    {
        if (lineRenderer == null || ropeStart == null) return;

        stretchTween?.Kill();

        Vector3 startPos = lineRenderer.GetPosition(1);
        Vector3 targetPos = ropeStart.position + stretchOffset;

        stretchTween = DOTween.Sequence()
            .Append(DOTween.To(() => lineRenderer.GetPosition(1),
                               x => lineRenderer.SetPosition(1, x),
                               targetPos,
                               stretchDuration)  // Rope 인스펙터에서 설정한 duration 사용
                   .SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => lineRenderer.GetPosition(1),
                               x => lineRenderer.SetPosition(1, x),
                               ropeStart.position,
                               stretchDuration)
                   .SetEase(Ease.OutQuad));
    }
}
