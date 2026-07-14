using UnityEngine;

public class MovingObject_3_13 : MonoBehaviour
{
    // 현재 물체 종류
    private CaseManager_3_13.ObjectType objectType;

    // 현재 이동 방향
    private CaseManager_3_13.MoveDirection moveDirection;

    // 실제 이동 방향 벡터
    private Vector3 moveVector;

    // 일정하게 유지할 이동 속도
    private float moveSpeed;

    // X축 이동 가능 최대 범위
    private float maxRangeX;

    // Y축 이동 가능 최대 범위
    private float maxRangeY;

    // 이미 성공 처리되었거나 삭제 처리 중인지 확인
    private bool isProcessed;

    // 범위 이탈 실패 로그가 중복 출력되는 것을 방지
    private bool isRangeFailed;

    // 외부에서 물체 종류 확인
    public CaseManager_3_13.ObjectType ObjectType => objectType;

    // 외부에서 이동 방향 확인
    public CaseManager_3_13.MoveDirection MoveDirection => moveDirection;

    // 외부에서 처리 여부 확인
    public bool IsProcessed => isProcessed;

    /// <summary>
    /// CaseManager_3_11에서 생성 직후 호출한다.
    /// 시작 위치에서 목표 위치까지 travelTime초가 걸리도록 속도를 계산한다.
    /// 목표 위치에 도착한 뒤에도 같은 방향과 속도로 계속 이동한다.
    /// </summary>
    public void Initialize(
        Vector3 startPosition,
        Vector3 targetPosition,
        float travelTime,
        CaseManager_3_13.ObjectType type,
        CaseManager_3_13.MoveDirection direction)
    {
        objectType = type;
        moveDirection = direction;

        transform.position = startPosition;

        isProcessed = false;
        isRangeFailed = false;

        // 시작 위치에서 목표 위치를 향하는 이동 방향을 계산한다.
        Vector3 difference = targetPosition - startPosition;

        moveVector = difference.normalized;

        // 시작 위치에서 목표 위치까지의 거리
        float distance = difference.magnitude;

        // travelTime이 0이 되는 것을 방지한다.
        float safeTravelTime = Mathf.Max(0.01f, travelTime);

        // 정확히 travelTime초 뒤 목표 좌표를 지나도록 속도를 계산한다.
        moveSpeed = distance / safeTravelTime;

        SetMaximumRange();

        Debug.Log(
            "[3-13] 이동 시작 / 물체: " +
            objectType +
            " / 방향: " +
            moveDirection +
            " / 시작 위치: " +
            startPosition +
            " / 목표 위치: " +
            targetPosition +
            " / 속도: " +
            moveSpeed.ToString("F2")
        );
    }

    private void Update()
    {
        if (isProcessed)
        {
            return;
        }

        // 목표 위치에 도착해도 정지하지 않고 같은 속도로 계속 이동한다.
        transform.position +=
            moveVector * moveSpeed * Time.deltaTime;

        CheckMaximumRange();
    }

    /// <summary>
    /// 물체 종류에 따라 화면 밖 최대 이동 범위를 지정한다.
    /// 사용자가 알려준 시작 범위를 그대로 최대 범위로 사용한다.
    /// </summary>
    private void SetMaximumRange()
    {
        switch (objectType)
        {
            case CaseManager_3_13.ObjectType.Stick:

                // Stick
                // 좌우 최대 범위: +-12.5
                // 상하 최대 범위: +-6.1
                maxRangeX = 12.5f;
                maxRangeY = 6.1f;
                break;

            case CaseManager_3_13.ObjectType.Knife:

                // Knife
                // 좌우 최대 범위: +-12
                // 상하 최대 범위: +-7
                maxRangeX = 12f;
                maxRangeY = 7f;
                break;

            case CaseManager_3_13.ObjectType.Gun:

                // Gun
                // 좌우 최대 범위: +-12
                // 상하 최대 범위: +-7
                maxRangeX = 12f;
                maxRangeY = 7f;
                break;

            case CaseManager_3_13.ObjectType.Tie:

                // Tie
                // 좌우 최대 범위: +-11
                // 상하 최대 범위: +-8
                maxRangeX = 11f;
                maxRangeY = 8f;
                break;

            default:
                maxRangeX = 15f;
                maxRangeY = 10f;
                break;
        }
    }

    /// <summary>
    /// 현재 이동 방향에 따라 해당 축의 최대 범위를 검사한다.
    /// 시작 반대편 최대 범위를 넘어가면 실패 처리한다.
    /// </summary>
    private void CheckMaximumRange()
    {
        bool isOutOfRange = false;

        switch (moveDirection)
        {
            case CaseManager_3_13.MoveDirection.TopToBottom:

                // 위에서 아래로 이동하므로 아래쪽 최대 범위를 확인한다.
                if (transform.position.y < -maxRangeY)
                {
                    isOutOfRange = true;
                }

                break;

            case CaseManager_3_13.MoveDirection.BottomToTop:

                // 아래에서 위로 이동하므로 위쪽 최대 범위를 확인한다.
                if (transform.position.y > maxRangeY)
                {
                    isOutOfRange = true;
                }

                break;

            case CaseManager_3_13.MoveDirection.LeftToRight:

                // 왼쪽에서 오른쪽으로 이동하므로 오른쪽 최대 범위를 확인한다.
                if (transform.position.x > maxRangeX)
                {
                    isOutOfRange = true;
                }

                break;

            case CaseManager_3_13.MoveDirection.RightToLeft:

                // 오른쪽에서 왼쪽으로 이동하므로 왼쪽 최대 범위를 확인한다.
                if (transform.position.x < -maxRangeX)
                {
                    isOutOfRange = true;
                }

                break;
        }

        if (isOutOfRange)
        {
            ProcessRangeFail();
        }
    }

    /// <summary>
    /// 같은 종류의 컨테이너와 겹친 상태에서 클릭했을 때 호출한다.
    /// 성공 로그 출력 후 오브젝트를 제거한다.
    /// </summary>
    public void ProcessSuccess()
    {
        if (isProcessed)
        {
            return;
        }

        isProcessed = true;

        Debug.Log(
            "[3-13] 성공 / 물체: " +
            objectType +
            " / 같은 종류의 컨테이너에서 클릭 성공"
        );

        Destroy(gameObject);
    }

    /// <summary>
    /// 다른 종류의 컨테이너와 겹쳤거나
    /// 올바른 상태가 아닐 때 클릭하면 호출한다.
    /// 현재는 실패 로그만 출력하고 물체는 계속 이동한다.
    /// </summary>
    public void ProcessWrongClick()
    {
        if (isProcessed)
        {
            return;
        }

        Debug.Log(
            "[3-13] 실패 / 물체: " +
            objectType +
            " / 올바른 컨테이너와 겹친 상태가 아님"
        );
    }

    /// <summary>
    /// 최대 이동 범위를 벗어났을 때 호출한다.
    /// 실패 로그 출력 후 오브젝트를 제거한다.
    /// </summary>
    private void ProcessRangeFail()
    {
        if (isProcessed || isRangeFailed)
        {
            return;
        }

        isRangeFailed = true;
        isProcessed = true;

        Debug.Log(
            "[3-13] 실패 / 물체: " +
            objectType +
            " / 최대 이동 범위를 벗어남 / 현재 위치: " +
            transform.position
        );

        Destroy(gameObject);
    }
}