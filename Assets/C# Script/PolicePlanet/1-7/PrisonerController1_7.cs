using DG.Tweening;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PrisonerController1_7 : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Tween moveTween;
    private bool isSuccess = false;
    private Collider2D[] childColliders;
    public PrisonerSpawner1_7 prisonerSpawner;

    private GameObject prohibitedItem;
    private bool itemDropped = false;

    public void SetProhibitedItem(GameObject item)
    {
        prohibitedItem = item;
    }
    void Start()
    {
        float targetX = 0; // 도달하면 Destroy 시킬 목표 위치
        float distance = transform.position.x - targetX;
        float duration = distance / moveSpeed;

        transform.DOMoveX(targetX, duration)
        .SetEase(Ease.InOutSine);
    }

    /*
    void Update()
    {
        if (isSuccess)
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

            if (transform.position.x < -15f)
            {
                GameManager1_7.instance.IncreaseSuccessCount();
                
                Destroy(gameObject);
                
            }
        }
        else if (transform.position.x > -0f)
        {
            SetChildCollidersActive(false);
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
            
        }
        else
        {
            SetChildCollidersActive(true);
        }
    }
    */

    //public void StartSuccessEscape()
    //{
    //    isSuccess = true;
    //}

    //private void SetChildCollidersActive(bool isActive)
    //{
    //    foreach (Collider2D col in childColliders)
    //    {
    //        if (col != null)
    //        {
    //            col.enabled = isActive;
    //        }
    //    }
    //}

    //public void DropProhibitedItem()
    //{
    //    if (itemDropped || prohibitedItem == null) return;

    //    itemDropped = true;

    //    prohibitedItem.transform.SetParent(null);

    //    Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
    //    if (rb == null)
    //        rb = prohibitedItem.AddComponent<Rigidbody2D>();

    //    rb.gravityScale = 2f;
    //    rb.AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
    //}

    public void DropProhibitedItem(Vector2 force)
    {
        if (prohibitedItem == null) return;

        Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        prohibitedItem.transform.SetParent(null); // 죄수에서 분리

        rb.simulated = true;
        rb.gravityScale = 1.5f;   // 중력 세기 (조절 가능)
        rb.velocity = Vector2.zero;

        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public void DropToBasket(Transform basket)
    {
        if (prohibitedItem == null) return;

        Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        prohibitedItem.transform.SetParent(null);

        rb.simulated = true;
        rb.gravityScale = 1.5f;
        rb.velocity = Vector2.zero;

        Vector2 start = prohibitedItem.transform.position;
        Vector2 end = basket.position;

        float gravity = Physics2D.gravity.y * rb.gravityScale;

        // 원하는 비행 시간 (짧을수록 빠름)
        float time = 1f;

        // 포물선 초기 속도 계산
        float vx = (end.x - start.x) / time;
        float heightBoost = 2.0f; // 원하는 만큼 높게 띄우기
        float vy = (end.y - start.y - 0.5f * gravity * time * time) / time + heightBoost;


        rb.AddForce(new Vector2(vx, vy), ForceMode2D.Impulse);
    }


}
