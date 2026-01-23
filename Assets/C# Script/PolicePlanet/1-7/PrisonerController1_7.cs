using DG.Tweening;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PrisonerController1_7 : MonoBehaviour
{
    public float moveSpeed = 2f;
    public PrisonerSpawner1_7 prisonerSpawner;

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
        if (item == null)
        {
            Debug.LogWarning("[SetProhibitedItem] item이 null입니다!");
            return;
        }

        prohibitedItem = item;

        // 부모를 범인 아래로 붙이고 위치 초기화
        prohibitedItem.transform.SetParent(transform);
        prohibitedItem.transform.localPosition = Vector3.zero;
        prohibitedItem.transform.localRotation = Quaternion.identity;

        // Rigidbody2D 세팅 (포물선 전엔 중력 OFF)
        Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
        if (rb == null) rb = prohibitedItem.AddComponent<Rigidbody2D>();

        rb.simulated = false;
        rb.gravityScale = 1.5f;
        rb.velocity = Vector2.zero;
    }

    public void DropToBasket(Transform basket)
    {
        if (prohibitedItem == null || basket == null) return;

        Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        prohibitedItem.transform.SetParent(null); // 범인에서 분리

        rb.simulated = true;
        rb.gravityScale = 1.5f;
        rb.velocity = Vector2.zero;

        Vector2 start = prohibitedItem.transform.position;
        Vector2 end = basket.position;

        float gravity = Physics2D.gravity.y * rb.gravityScale;
        float time = 1f;              // 원하는 비행 시간
        float heightBoost = 2.0f;     // 포물선 높이

        // 기존 포물선 계산 그대로
        float vx = (end.x - start.x) / time;
        float vy = (end.y - start.y - 0.5f * gravity * time * time) / time + heightBoost;

        rb.AddForce(new Vector2(vx, vy), ForceMode2D.Impulse);
    }
}
