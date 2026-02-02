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
        Debug.Log($"[SetProhibitedItem] 호출됨: {item?.name}");

        if (item == null)
        {
            Debug.LogWarning("SetProhibitedItem: item null");
            return;
        }

        prohibitedItem = item;

        Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
        if (rb == null) rb = prohibitedItem.AddComponent<Rigidbody2D>();

        // 들고 있는 상태
        rb.simulated = false;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }

    public void DropToBasket(Transform basket)
    {
        Debug.Log($"[DropToBasket] prohibitedItem = {prohibitedItem}");

        if (prohibitedItem == null || basket == null)
        {
            Debug.LogWarning("DropToBasket: null 참조");
            return;
        }

        Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        prohibitedItem.transform.SetParent(null);

        rb.simulated = true;
        rb.gravityScale = 1.5f;
        rb.velocity = Vector2.zero;

        Vector2 start = prohibitedItem.transform.position;
        Vector2 end = basket.position;

        float gravity = Physics2D.gravity.y * rb.gravityScale;
        float time = 1f;
        float heightBoost = 2.0f;

        float vx = (end.x - start.x) / time;
        float vy = (end.y - start.y - 0.5f * gravity * time * time) / time + heightBoost;

        rb.AddForce(new Vector2(vx, vy), ForceMode2D.Impulse);
    }

    public GameObject GetProhibitedItem()
    {
        return prohibitedItem;
    }
}
