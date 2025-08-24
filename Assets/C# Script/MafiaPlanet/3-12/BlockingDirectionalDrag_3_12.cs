using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class BlockingDirectionalDrag_3_12 : DragAndDrop
{
    public bool isLast = false;
    public bool isBlocked = false;
    private Vector2 blockDir;

    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float nudgeDistance = 0.12f;   // 반대쪽으로 밀어낼 거리

    private Collider2D realCollider;
    private Rigidbody2D rb;
    private readonly Collider2D[] _hits = new Collider2D[8];

    protected override void OnMouseDown()
    {
        if (isLast) return;

        base.OnMouseDown();

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        realCollider = GetComponentInChildren<Collider2D>();

        // 클릭 순간 벽 접촉 체크 + 반대쪽으로 살짝 밀기
        Vector2 contactNormal;
        if (TryGetWallContactNormal(out contactNormal))
        {
            // 드래그 금지 플래그
            banDragging = true;
            isBlocked = true;
            blockDir = contactNormal;

            // 반대 방향으로 살짝 이동 (충돌 해소)
            Vector2 nudge = -contactNormal.normalized * nudgeDistance;
            rb.position = rb.position + nudge;
        }
        else
        {
            isBlocked = false;
            banDragging = false;
            blockDir = Vector2.zero;
        }
    }

    protected override void Update()
    {
        if (!isDragging || banDragging) return;

        // 진행 중에도 접촉 갱신
        Vector2 contactNormal;
        bool touchingWall = TryGetWallContactNormal(out contactNormal);
        isBlocked = touchingWall;
        if (touchingWall) blockDir = contactNormal; else blockDir = Vector2.zero;

        Vector3 mouseWorldPos = GetMouseWorldPos();
        Vector3 targetPos = mouseWorldPos + offset;

        if (isBlocked)
        {
            Vector2 moveDir = (targetPos - transform.position).normalized;
            // 벽 방향(법선과 같은 방향)으로 가려 하면 고정
            if (Vector2.Dot(moveDir, blockDir) > 0f)
            {
                targetPos = transform.position;
            }
        }

        Vector3 constrainedTarget = GetConstrainedPosition(transform.position, targetPos);
        transform.position = Vector3.Lerp(transform.position, constrainedTarget, followSpeed * Time.deltaTime);
    }

    // 벽과의 접촉을 검사하고, 접촉 normal(내 콜라이더 -> 벽) 반환
    private bool TryGetWallContactNormal(out Vector2 avgNormal)
    {
        avgNormal = Vector2.zero;
        if (realCollider == null) return false;

        // wallLayer 필터로 겹치는 콜라이더 수집
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = wallLayer;
        filter.useTriggers = false;

        int count = Physics2D.OverlapCollider(realCollider, filter, _hits);
        if (count <= 0) return false;

        int overlapped = 0;
        Vector2 sum = Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            Collider2D other = _hits[i];
            if (other == null || other == realCollider) continue;

            // 거리/겹침 정보에서 normal 획득
            ColliderDistance2D dist = realCollider.Distance(other);
            if (dist.isOverlapped)
            {
                // normal: realCollider -> other 방향
                sum += dist.normal;
                overlapped++;
            }
        }

        if (overlapped > 0)
        {
            avgNormal = (sum / overlapped).normalized;
            return true;
        }

        return false;
    }

    public void Revealed()
    {
        isLast = true;
        banDragging = true;
        Debug.Log("들킴");
    }

    public void SetBlocked(Vector2 collisionNormal, bool blocked)
    {
        isBlocked = blocked;
        blockDir = blocked ? collisionNormal : Vector2.zero;
    }
}
