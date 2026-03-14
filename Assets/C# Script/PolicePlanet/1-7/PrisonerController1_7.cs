using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PrisonerController1_7 : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arriveX = 0f;
    [SerializeField] private float exitOffsetX = -3f;
    [SerializeField] private Ease enterEase = Ease.InOutSine;
    [SerializeField] private Ease exitEase = Ease.InSine;

    private GameObject prohibitedItem;
    private Tween moveTween;

    public Action OnArrived;

    public void EnterFromRight()
    {
        KillMoveTween();

        float distance = Mathf.Abs(transform.position.x - arriveX);
        float duration = (moveSpeed <= 0f) ? 0f : distance / moveSpeed;

        moveTween = transform.DOMoveX(arriveX, duration)
            .SetEase(enterEase)
            .OnComplete(() =>
            {
                moveTween = null;
                OnArrived?.Invoke();
            });
    }

    public IEnumerator ExitToLeftAndDestroy(float duration)
    {
        KillMoveTween();

        float targetX = transform.position.x + exitOffsetX;

        moveTween = transform.DOMoveX(targetX, duration)
            .SetEase(exitEase);

        yield return moveTween.WaitForCompletion();

        moveTween = null;
        float fadeTime = 0.25f;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

        Sequence fadeSeq = DOTween.Sequence();

        foreach (var sr in renderers)
        {
            fadeSeq.Join(sr.DOFade(0f, fadeTime));
        }

        yield return fadeSeq.WaitForCompletion();

        Destroy(gameObject);
    }

    public void SetProhibitedItem(GameObject item)
    {
        prohibitedItem = item;
        if (prohibitedItem == null) return;

        Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = prohibitedItem.AddComponent<Rigidbody2D>();

        rb.simulated = false;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Collider2D col = prohibitedItem.GetComponent<Collider2D>();
        if (col == null)
            col = prohibitedItem.AddComponent<BoxCollider2D>();

        col.isTrigger = false;
    }

    public void DropToBasket(Transform basket)
    {
        if (prohibitedItem == null || basket == null) return;

        Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        prohibitedItem.transform.SetParent(null);
        Destroy(prohibitedItem, 3f);

        rb.simulated = true;
        rb.gravityScale = 1.6f;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Collider2D itemCollider = prohibitedItem.GetComponent<Collider2D>();
        if (itemCollider != null)
        {
            Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
            foreach (Collider2D col in allColliders)
            {
                if (col != itemCollider && !col.isTrigger)
                    Physics2D.IgnoreCollision(col, itemCollider, true);
            }
        }

        Vector2 start = prohibitedItem.transform.position;
        Vector2 end = basket.position;

        float gravity = Physics2D.gravity.y * rb.gravityScale;

        // 기존보다 더 부드럽고 짧은 포물선
        float time = 0.65f;

        // "먼저 위로 튄다"는 느낌을 주는 추가 상승량
        float extraUp = 1.2f;

        float vx = (end.x - start.x) / time;

        // 기본 포물선 속도 + 위로 살짝 더 띄우기
        float vy = (end.y - start.y - 0.5f * gravity * time * time) / time + extraUp;

        rb.AddForce(new Vector2(vx, vy), ForceMode2D.Impulse);

        // 회전도 너무 과하지 않게
        rb.AddTorque(-80f, ForceMode2D.Impulse);

        ProhibitItemDestroy1_7 destroyScript = prohibitedItem.GetComponent<ProhibitItemDestroy1_7>();
        if (destroyScript == null)
            destroyScript = prohibitedItem.AddComponent<ProhibitItemDestroy1_7>();

        destroyScript.Init(basket);
    }

    public GameObject GetProhibitedItem()
    {
        return prohibitedItem;
    }

    private void KillMoveTween()
    {
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
            moveTween = null;
        }

        transform.DOKill();
    }

    private void OnDestroy()
    {
        KillMoveTween();
    }
}