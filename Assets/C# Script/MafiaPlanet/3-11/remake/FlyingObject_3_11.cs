using System.Collections;
using UnityEngine;

public class FlyingObject_3_11 : MonoBehaviour
{
    // 시작 위치
    private Vector3 startPosition;

    // 반드시 지나가야 하는 중앙 위치
    private Vector3 centerPosition;

    // 중앙을 지난 뒤 이동할 끝 위치
    private Vector3 endPosition;

    // 시작 위치에서 중앙까지 걸리는 시간
    private float travelTime;

    // 중앙에서 끝 위치까지 걸리는 시간
    private float afterCenterTime;

    // 포물선의 높이
    private float arcHeight;

    // 곡선 방향
    // 1이면 한쪽 방향, -1이면 반대 방향
    private float arcDirection;

    // 회전 속도
    private float rotationSpeed;

    // 현재 실행 중인 이동 코루틴
    private Coroutine moveCoroutine;

    // 이미 클릭 처리된 물체인지 확인
    public bool IsProcessed { get; private set; }

    /// <summary>
    /// ObjectSpawner_3_11에서 생성 직후 호출한다.
    /// 이동에 필요한 값을 전달받고 포물선 이동을 시작한다.
    /// </summary>
    public void Initialize(
        Vector3 startPos,
        Vector3 centerPos,
        Vector3 endPos,
        float centerTravelTime,
        float afterTravelTime,
        float curveHeight,
        float curveDirection,
        float rotateSpeed)
    {
        startPosition = startPos;
        centerPosition = centerPos;
        endPosition = endPos;

        // 0 이하의 시간이 들어오지 않도록 최소값을 지정한다.
        travelTime = Mathf.Max(0.01f, centerTravelTime);
        afterCenterTime = Mathf.Max(0.01f, afterTravelTime);

        arcHeight = Mathf.Abs(curveHeight);

        // 전달된 값이 양수면 1, 음수면 -1로 통일한다.
        arcDirection = curveDirection >= 0f ? 1f : -1f;

        rotationSpeed = rotateSpeed;

        IsProcessed = false;

        transform.position = startPosition;

        // 기존 코루틴이 실행 중이면 중지한다.
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveRoutine());
    }

    /// <summary>
    /// 시작 위치에서 중앙을 지나 끝 위치까지 이동한다.
    /// </summary>
    private IEnumerator MoveRoutine()
    {
        float elapsedTime = 0f;

        // 시작 위치에서 중앙 위치까지 이동한다.
        while (elapsedTime < travelTime)
        {
            // 이미 클릭 처리된 물체라면 이동을 종료한다.
            if (IsProcessed)
            {
                yield break;
            }

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsedTime / travelTime
            );

            // 시작 위치에서 중앙 위치까지 곡선으로 이동한다.
            transform.position = GetCurvePosition(
                startPosition,
                centerPosition,
                t,
                arcHeight
            );

            RotateObject();

            yield return null;
        }

        // 프레임 오차와 관계없이
        // 정확히 중앙 위치에 도착하도록 보정한다.
        transform.position = centerPosition;

        elapsedTime = 0f;

        // 중앙 위치에서 끝 위치까지 이동한다.
        while (elapsedTime < afterCenterTime)
        {
            if (IsProcessed)
            {
                yield break;
            }

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsedTime / afterCenterTime
            );

            // 중앙을 지난 뒤에는 곡선 크기를 조금 줄인다.
            transform.position = GetCurvePosition(
                centerPosition,
                endPosition,
                t,
                arcHeight * 0.5f
            );

            RotateObject();

            yield return null;
        }

        // 끝 위치까지 이동한 후 제거한다.
        Destroy(gameObject);
    }

    /// <summary>
    /// 두 위치 사이의 곡선 좌표를 계산한다.
    /// </summary>
    private Vector3 GetCurvePosition(
        Vector3 from,
        Vector3 to,
        float t,
        float height)
    {
        // 먼저 직선 이동 위치를 계산한다.
        Vector3 position = Vector3.Lerp(
            from,
            to,
            t
        );

        // 현재 이동 방향을 구한다.
        Vector3 direction = to - from;

        // 이동 방향과 수직인 방향을 구한다.
        // 어느 방향에서 생성되더라도 이동 경로 옆으로 휘게 된다.
        Vector3 perpendicular = new Vector3(
            -direction.y,
            direction.x,
            0f
        ).normalized;

        // 시작점과 끝점에서는 0,
        // 중간 지점에서는 가장 큰 곡선 값을 가진다.
        float curveOffset =
            Mathf.Sin(t * Mathf.PI) *
            height *
            arcDirection;

        // 직선 위치에 곡선 방향 값을 더한다.
        position += perpendicular * curveOffset;

        return position;
    }

    /// <summary>
    /// 이동 중 오브젝트를 회전시킨다.
    /// </summary>
    private void RotateObject()
    {
        transform.Rotate(
            0f,
            0f,
            rotationSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Target 또는 Bomb이 클릭 처리되었을 때 호출한다.
    /// </summary>
    public void ProcessObject()
    {
        // 이미 처리된 물체라면 다시 처리하지 않는다.
        if (IsProcessed)
        {
            return;
        }

        IsProcessed = true;

        // 이동 코루틴을 중지한다.
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        Debug.Log(
            "[3-11] 오브젝트 처리: " +
            gameObject.name +
            " / Tag: " +
            tag
        );

        // 현재는 효과 없이 바로 제거한다.
        Destroy(gameObject);
    }

    /// <summary>
    /// 오브젝트가 파괴될 때 코루틴 참조를 정리한다.
    /// </summary>
    private void OnDestroy()
    {
        moveCoroutine = null;
    }
}