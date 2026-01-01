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

    public void DropProhibitedItem()
    {
        if (itemDropped || prohibitedItem == null) return;

        itemDropped = true;
        prohibitedItem.transform.SetParent(null);

        Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
        rb.gravityScale = 2f;    // 중력 적용
        rb.simulated = true;     // 물리 활성화
        rb.AddForce(Vector2.up * 2f, ForceMode2D.Impulse); // 튀는 효과
    }
}
