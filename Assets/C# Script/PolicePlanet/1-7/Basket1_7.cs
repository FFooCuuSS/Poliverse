using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Basket1_7 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Item")) return;

        Debug.Log("아이템이 바구니에 닿음 → 고정 후 페이드");

        GameObject root = other.transform.root.gameObject;

        // 1) 이동/물리 정지
        Rigidbody2D rb = root.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }

        // 2) 충돌 비활성화(중복 트리거 방지)
        Collider2D col = root.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 3) 현재 자리 고정 (부모 영향 방지)
        //root.transform.position = root.transform.position;

        // 4) 페이드 아웃 후 제거
        SpriteRenderer sr = root.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.DOFade(0f, 0.5f)
              .SetEase(Ease.OutQuad)
              .OnComplete(() => Destroy(root));
        }
        else
        {
            // 보험
            Destroy(root);
        }
    }
}
