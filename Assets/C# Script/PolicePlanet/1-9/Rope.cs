using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    [Header("줄 시작점")]
    [SerializeField] private Transform ropeStart;

    [Header("줄 끝점(핸들)")]
    [SerializeField] private Transform ropeEnd;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        if (lineRenderer.startWidth == 0f) lineRenderer.startWidth = 0.1f;
        if (lineRenderer.endWidth == 0f) lineRenderer.endWidth = 0.1f;
    }

    private void LateUpdate()
    {
        if (lineRenderer == null || ropeStart == null || ropeEnd == null) return;

        lineRenderer.SetPosition(0, ropeStart.position);
        lineRenderer.SetPosition(1, ropeEnd.position);
    }

    public void ResetRope()
    {
        if (lineRenderer == null || ropeStart == null || ropeEnd == null) return;

        lineRenderer.SetPosition(0, ropeStart.position);
        lineRenderer.SetPosition(1, ropeEnd.position);
    }
}