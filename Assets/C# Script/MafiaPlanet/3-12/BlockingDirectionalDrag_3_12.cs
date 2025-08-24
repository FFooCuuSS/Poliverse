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
    [SerializeField] private float nudgeDistance = 0.12f;   // �ݴ������� �о �Ÿ�

    private Collider2D realCollider;
    private Rigidbody2D rb;
    private readonly Collider2D[] _hits = new Collider2D[8];

    protected override void OnMouseDown()
    {
        if (isLast) return;

        base.OnMouseDown();

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        realCollider = GetComponentInChildren<Collider2D>();

        // Ŭ�� ���� �� ���� üũ + �ݴ������� ��¦ �б�
        Vector2 contactNormal;
        if (TryGetWallContactNormal(out contactNormal))
        {
            // �巡�� ���� �÷���
            banDragging = true;
            isBlocked = true;
            blockDir = contactNormal;

            // �ݴ� �������� ��¦ �̵� (�浹 �ؼ�)
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

        // ���� �߿��� ���� ����
        Vector2 contactNormal;
        bool touchingWall = TryGetWallContactNormal(out contactNormal);
        isBlocked = touchingWall;
        if (touchingWall) blockDir = contactNormal; else blockDir = Vector2.zero;

        Vector3 mouseWorldPos = GetMouseWorldPos();
        Vector3 targetPos = mouseWorldPos + offset;

        if (isBlocked)
        {
            Vector2 moveDir = (targetPos - transform.position).normalized;
            // �� ����(������ ���� ����)���� ���� �ϸ� ����
            if (Vector2.Dot(moveDir, blockDir) > 0f)
            {
                targetPos = transform.position;
            }
        }

        Vector3 constrainedTarget = GetConstrainedPosition(transform.position, targetPos);
        transform.position = Vector3.Lerp(transform.position, constrainedTarget, followSpeed * Time.deltaTime);
    }

    // ������ ������ �˻��ϰ�, ���� normal(�� �ݶ��̴� -> ��) ��ȯ
    private bool TryGetWallContactNormal(out Vector2 avgNormal)
    {
        avgNormal = Vector2.zero;
        if (realCollider == null) return false;

        // wallLayer ���ͷ� ��ġ�� �ݶ��̴� ����
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

            // �Ÿ�/��ħ �������� normal ȹ��
            ColliderDistance2D dist = realCollider.Distance(other);
            if (dist.isOverlapped)
            {
                // normal: realCollider -> other ����
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
        Debug.Log("��Ŵ");
    }

    public void SetBlocked(Vector2 collisionNormal, bool blocked)
    {
        isBlocked = blocked;
        blockDir = blocked ? collisionNormal : Vector2.zero;
    }
}
