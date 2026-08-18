using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 큰 냄비를 국자로 젓는 느낌의 드래그 모션.
///
///  - 국자는 항상 centerPoint를 중심으로 한 "타원" 궤적 위에서만 움직인다.
///    (마우스 방향을 그대로 따라가되, 좌우 반경(radiusX)과 상하 반경(radiusY)을 다르게 주면
///     납작한 타원 모양으로 저어지는 느낌을 낼 수 있다)
///
///  - 아트 리소스가 좌/우 어느 한쪽을 바라보게 고정으로 그려져 있는 경우를 위해,
///    회전 대신 가로 반전(스프라이트 좌우 뒤집기)만 사용한다.
///
///  - 국자가 중앙(centerPoint.x)을 지나 좌<->우로 넘어갈 때마다,
///    그 순간을 하나의 "스와이프" 입력으로 간주해 OnSwipeDetected를 발생시킨다.
///    (이 스와이프 이벤트는 Minigame_2_10의 판정 입력으로 그대로 이어진다)
/// </summary>
public class ScoopDrag : MonoBehaviour
{
    [Header("타원 궤적")]
    public Transform centerPoint;

    [FormerlySerializedAs("radius")]
    [Tooltip("좌우 방향 반경 (냄비를 젓는 폭)")]
    public float radiusX = 1.5f;

    [Tooltip("상하 방향 반경. radiusX보다 작게 주면 옆으로 납작한 타원이 된다")]
    public float radiusY = 0.6f;

    [Header("움직임 부드러움")]
    [Tooltip("목표 위치(마우스 방향의 타원 위 지점)를 얼마나 즉각적으로 따라갈지. " +
             "값이 클수록 마우스를 딱 붙어서 따라가고, 작을수록 국자가 관성이 있는 것처럼 천천히 따라간다.")]
    [SerializeField] private float followSpeed = 12f;

    [Header("좌우 반전")]
    [Tooltip("원본 아트(스프라이트)가 기본적으로 오른쪽을 바라보고 그려져 있으면 체크, 왼쪽을 바라보고 있으면 해제")]
    [SerializeField] private bool artFacesRightByDefault = true;

    public float CurSpeed { get; private set; }

    [Header("스와이프 판정")]
    [Tooltip("좌<->우 전환을 스와이프로 인정하기 위한 최소 속도")]
    [SerializeField] private float minSwipeSpeed = 1f;
    [Tooltip("스와이프 판정 후 다음 스와이프까지 최소 대기 시간(연속 트리거 방지, 초)")]
    [SerializeField] private float swipeCooldown = 0.15f;

    /// <summary>좌 또는 우로 스와이프가 감지될 때마다 호출된다.</summary>
    public event Action OnSwipeDetected;

    private bool isDragging = false;
    private Vector3 baseScale;
    private Vector2 lastPosition;
    private float lastTime;

    private int lastSide = 0;      // -1: 왼쪽, 1: 오른쪽, 0: 아직 판정 전
    private float lastSwipeTime = -999f;

    // 마우스 입력으로 계산된, 국자가 향해 가야 할 타원 위의 목표 지점
    private Vector2 targetPos;

    private void Start()
    {
        baseScale = transform.localScale;
        lastPosition = transform.localPosition;
        lastTime = Time.time;
        targetPos = transform.position;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D col = GetComponent<Collider2D>();

            if (col != null && col.OverlapPoint(mouseWorld))
            {
                isDragging = true;
                lastPosition = transform.position;
                lastSide = 0;
            }
        }

        if (Input.GetMouseButtonUp(0)) isDragging = false;

        if (isDragging)
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 center = centerPoint.position;

            Vector2 direction = mouseWorld - center;
            if (direction.sqrMagnitude > 0.0001f)
                direction.Normalize();

            // 단위 원 방향 벡터를 축마다 다른 반경(radiusX, radiusY)으로 스케일하면
            // 정확히 그 타원(x²/radiusX² + y²/radiusY² = 1) 위의 점이 된다.
            targetPos = center + new Vector2(direction.x * radiusX, direction.y * radiusY);
        }

        // 드래그 중이 아니어도 마지막 목표 지점까지는 부드럽게 이동을 마무리한다.
        Vector2 currentPos = transform.position;
        Vector2 smoothed = Vector2.Lerp(currentPos, targetPos, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
        transform.position = smoothed;

        int currentSide = 0;

        if (transform.position.x < centerPoint.position.x)
            currentSide = -1;
        else if (transform.position.x > centerPoint.position.x)
            currentSide = 1;

        // 회전 대신 가로 반전만 사용한다.
        if (currentSide != 0)
        {
            bool shouldFaceRight = currentSide == 1;
            bool mirrored = shouldFaceRight != artFacesRightByDefault;

            float xSign = mirrored ? -1f : 1f;
            transform.localScale = new Vector3(xSign * Mathf.Abs(baseScale.x), baseScale.y, baseScale.z);
        }

        float distance = Vector2.Distance(transform.position, lastPosition);
        float deltaTime = Time.time - lastTime;
        if (deltaTime > 0)
        {
            CurSpeed = distance / deltaTime;
        }

        // 좌<->우 방향이 바뀌는 순간(=국자가 중앙을 지나며 반전되는 순간) = 스와이프 한 번
        if (isDragging && currentSide != 0 && lastSide != 0 && currentSide != lastSide)
        {
            if (CurSpeed >= minSwipeSpeed && Time.time - lastSwipeTime >= swipeCooldown)
            {
                lastSwipeTime = Time.time;
                OnSwipeDetected?.Invoke();
            }
        }

        if (currentSide != 0)
            lastSide = currentSide;

        lastPosition = transform.position;
        lastTime = Time.time;
    }
}