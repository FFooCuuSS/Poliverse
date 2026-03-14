using UnityEngine;

public class DragDistanceJudge : MonoBehaviour
{
    [Header("미니게임 참조")]
    [SerializeField] private Minigame_1_9 minigame_1_9;

    [Header("드래그 판정")]
    [SerializeField] private float distanceThreshold = 2.0f;

    private Vector3 lastPosition;
    private float totalDistance;
    private bool isDragging;

    private void OnMouseDown()
    {
        lastPosition = transform.position;
        totalDistance = 0f;
        isDragging = true;

        Debug.Log("드래그 시작");
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 currentPos = transform.position;
        float delta = Vector3.Distance(currentPos, lastPosition);
        totalDistance += delta;
        lastPosition = currentPos;

        Debug.Log($"현재 누적 거리: {totalDistance}");
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        Debug.Log($"드래그 종료 / 최종 거리: {totalDistance}");

        if (totalDistance >= distanceThreshold)
        {
            Debug.Log("드래그 조건 성공 → 타이밍 판정 요청");
            minigame_1_9?.SubmitPlayerInput("Input");
        }
        else
        {
            Debug.Log("드래그 거리 부족 → 입력 실패");
            // 아무것도 안 함
            // End 이벤트에서 자동 Miss
        }
    }
}