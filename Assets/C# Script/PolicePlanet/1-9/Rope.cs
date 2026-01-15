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

    public Transform ropeStart;  // 줄 시작점
    private LineRenderer lineRenderer;

    private Tween stretchTween;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;

            // 기본 색상, 너비 설정 꼭 해주기
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
        }
    }

    void Update()
    {
        if (lineRenderer == null || ropeStart == null) return;

        lineRenderer.SetPosition(0, ropeStart.position);
        // 끝점은 Tween으로 움직이므로 여기선 그냥 현재 위치 유지
        // 혹시 Tween 작동 전에는 끝점 위치를 초기화 해주세요:
        if (!DOTween.IsTweening(transform)) // DOTween 이용 중 아니면
            lineRenderer.SetPosition(1, transform.position);
    }


    void Start()
    {
        if (lineRenderer != null && ropeStart != null)
        {
            lineRenderer.SetPosition(0, ropeStart.position);
            lineRenderer.SetPosition(1, ropeStart.position); // 처음 끝점도 시작점
        }
    }


    public void PlayStretch(Vector3 stretchOffset, float duration)
    {
        if (lineRenderer == null) return;

        stretchTween?.Kill();

        Vector3 startPos = lineRenderer.GetPosition(1);        // 현재 끝점
        Vector3 targetPos = ropeStart.position + stretchOffset; // 목표 위치 (시작점 기준)

        stretchTween = DOTween.To(() => lineRenderer.GetPosition(1),
                                  x => lineRenderer.SetPosition(1, x),
                                  targetPos,
                                  duration / 2f)
            .OnComplete(() =>
            {
                DOTween.To(() => lineRenderer.GetPosition(1),
                           x => lineRenderer.SetPosition(1, x),
                           ropeStart.position,
                           duration / 2f);
            });
    }
}
