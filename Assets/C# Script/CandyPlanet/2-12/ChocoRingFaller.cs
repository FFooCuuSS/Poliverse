using UnityEngine;

/// <summary>
/// Show 단계(0~2초)에서 스폰된 프리뷰 초코링에 붙는 컴포넌트.
/// 스폰 즉시 일정 속도로 계속 아래로 낙하하며, 화면 밖 트리거(ChocoRingOffscreenCleanup)에
/// 닿으면 그쪽에서 자동으로 Destroy 처리한다. 별도의 duration 계산이 필요 없어서
/// 화면 크기/레이아웃이 달라져도 항상 정확히 "화면을 벗어났을 때" 사라진다.
/// 캐치 단계(Input) 링에는 붙이지 않는다 - 그쪽은 DOTween으로 정확한 목표 지점(그릇)까지 이동해야 하므로.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ChocoRingFaller : MonoBehaviour
{
    public float speed = 8f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + Vector2.down * speed * Time.fixedDeltaTime);
    }
}