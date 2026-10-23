using UnityEngine;
using System.Collections;

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
    private bool isDemoAnimating = false;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        if (line != null)
        {
            line.startWidth = 1f;
            line.endWidth = 1f;
        }
        if (startTarget != null && endTarget != null)
        {
            initialDirection = (endTarget.position - startTarget.position).normalized;
            initialEndPosition = endTarget.position;
        }
    }

    void LateUpdate()
    {
        if (startTarget == null || endTarget == null) return;

        // 데모 모드일 때 플레이어 조작 차단 및 위치 고정
        if (Minigame_3_1.Instance != null && Minigame_3_1.Instance.IsDemoMode)
        {
            if (!isDemoAnimating)
            {
                endTarget.position = initialEndPosition;
            }
            if (linkedPistol != null)
            {
                linkedPistol.canPull = false;
            }
        }

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

        // 일반 모드일 때만 플레이어 입력 조건 체크
        if (Minigame_3_1.Instance == null || !Minigame_3_1.Instance.IsDemoMode)
        {
            Vector2 currentDir = (endTarget.position - startTarget.position).normalized;
            float angleDiff = Vector2.Angle(initialDirection, currentDir);
            float displacement = Vector3.Distance(endTarget.position, initialEndPosition);

            if (linkedPistol != null)
            {
                linkedPistol.canPull = angleDiff >= activateAngleThreshold &&
                                      displacement >= activateDistanceThreshold;
            }
        }
    }

    // 데모 모드에서 좌우 위치에 맞춰 당겨지는 애니메이션 실행
    public void PlayDemoPullAnimation(float duration, float pullDistance, System.Action onComplete)
    {
        StartCoroutine(DemoPullRoutine(duration, pullDistance, onComplete));
    }

    private IEnumerator DemoPullRoutine(float duration, float pullDistance, System.Action onComplete)
    {
        isDemoAnimating = true;
        Vector3 startPos = initialEndPosition;

        Vector2 dir = initialDirection;
        if (transform.position.x < 0f)
        {
            dir = -initialDirection; // 왼쪽 버튼은 방향 반전
        }

        Vector3 targetPos = initialEndPosition + (Vector3)dir * pullDistance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            endTarget.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        endTarget.position = targetPos;

        isDemoAnimating = false;
        onComplete?.Invoke();
    }
}
