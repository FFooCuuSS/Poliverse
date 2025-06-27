using UnityEngine;

public class Rope : MonoBehaviour
{
    public Transform target; // 참조할 타겟
    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2; // 시작점, 끝점
        }
    }

    void Update()
    {
        if (target == null || lineRenderer == null)
            return;

        Vector3 myPosition = transform.position;
        Vector3 targetPosition = target.position;

        lineRenderer.SetPosition(0, myPosition);
        lineRenderer.SetPosition(1, targetPosition);
    }
}
