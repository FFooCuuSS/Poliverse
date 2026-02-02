using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProhibitItemDestroy1_7 : MonoBehaviour
{
    private Transform basket;

    public void Init(Transform targetBasket)
    {
        basket = targetBasket;

        // Collider2D가 있으면 Trigger로 설정
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (basket == null) return;

        if (collision.transform == basket)
        {
            Destroy(gameObject);
        }
    }
}
