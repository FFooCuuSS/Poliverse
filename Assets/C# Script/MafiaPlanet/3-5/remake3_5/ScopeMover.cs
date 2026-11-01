using UnityEngine;

// 3-5 스코프 이동. 시간(초)을 넣으면 그 시점 위치를 계산한다 (노래 시간 기준으로 미니게임이 호출).
// movePoints[i]에 elapsed = moveTimePerStep * (i + 1) 순간 도착.
public class ScopeMover : MonoBehaviour
{
    [Header("이동할 좌표들 순서대로 입력")]
    public Vector3[] movePoints;

    [Header("한 구간당 걸리는 시간")]
    public float moveTimePerStep = 0.6f;

    [Header("(구버전)")]
    public bool playOnStart;
    public bool loop = false;
    public CameraFollow3_5 cameraFollow;

    private Vector3 startPos;
    private bool startCaptured;

    private void Awake()
    {
        startPos = transform.position;
        startCaptured = true;
    }

    // movePoints[i] 도착 시각 (이동 시작 기준 경과 시간)
    public float ArrivalTime(int i)
    {
        return moveTimePerStep * (i + 1);
    }

    public void SetByElapsed(float elapsed)
    {
        if (movePoints == null || movePoints.Length == 0) return;
        if (!startCaptured) { startPos = transform.position; startCaptured = true; }

        if (elapsed <= 0f)
        {
            transform.position = startPos;
            return;
        }

        int seg = Mathf.FloorToInt(elapsed / moveTimePerStep);

        if (seg >= movePoints.Length)
        {
            transform.position = movePoints[movePoints.Length - 1];
            return;
        }

        Vector3 from = seg == 0 ? startPos : movePoints[seg - 1];
        Vector3 to = movePoints[seg];
        float k = (elapsed - seg * moveTimePerStep) / moveTimePerStep;

        transform.position = Vector3.Lerp(from, to, k);
    }

    // 구버전 호환 (미니게임이 SetByElapsed로 직접 움직이므로 아무것도 안 함)
    public void StartMove()
    {
    }
}