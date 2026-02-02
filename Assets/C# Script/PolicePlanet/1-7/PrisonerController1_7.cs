using DG.Tweening;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PrisonerController1_7 : MonoBehaviour
{
    public float moveSpeed = 2f;

    private GameObject prohibitedItem;

    void Start()
    {
        float targetX = 0;
        float distance = transform.position.x - targetX;
        float duration = distance / moveSpeed;

        transform.DOMoveX(targetX, duration)
                 .SetEase(Ease.InOutSine);
    }

    public void SetProhibitedItem(GameObject item)
    {
        prohibitedItem = item;
        if (prohibitedItem == null) return;

        Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
        if (rb == null) rb = prohibitedItem.AddComponent<Rigidbody2D>();

        rb.simulated = false;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;

        // Collider2D 확인
        Collider2D col = prohibitedItem.GetComponent<Collider2D>();
        if (col == null) col = prohibitedItem.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
    }

    public void DropToBasket(Transform basket)
    {
        if (prohibitedItem == null || basket == null) return;

        Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // 부모 해제
        prohibitedItem.transform.SetParent(null);

        // Rigidbody 활성화
        rb.simulated = true;
        rb.gravityScale = 1.5f;
        rb.velocity = Vector2.zero;

        // 다른 콜라이더 무시
        Collider2D itemCollider = prohibitedItem.GetComponent<Collider2D>();
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
        foreach (Collider2D col in allColliders)
        {
            if (col != itemCollider && !col.isTrigger)
                Physics2D.IgnoreCollision(col, itemCollider, true);
        }

        // 포물선 계산
        Vector2 start = prohibitedItem.transform.position;
        Vector2 end = basket.position;

        float gravity = Physics2D.gravity.y * rb.gravityScale;
        float time = 1f;              // 비행 시간
        float heightBoost = 1.8f;     // 포물선 높이

        float vx = (end.x - start.x) / time;
        float vy = (end.y - start.y - 0.5f * gravity * time * time) / time + heightBoost;

        rb.AddForce(new Vector2(vx, vy), ForceMode2D.Impulse);

        // 바구니 도착 감지 스크립트 추가
        ProhibitItemDestroy1_7 destroyScript = prohibitedItem.GetComponent<ProhibitItemDestroy1_7>();
        if (destroyScript == null)
            destroyScript = prohibitedItem.AddComponent<ProhibitItemDestroy1_7>();
        destroyScript.Init(basket);
    }

    public GameObject GetProhibitedItem()
    {
        return prohibitedItem;
    }
}
