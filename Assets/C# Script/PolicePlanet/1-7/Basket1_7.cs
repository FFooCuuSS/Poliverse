using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basket1_7 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            Debug.Log("아이템이 바구니에 닿음 → 제거");

            GameObject root = other.transform.root.gameObject;
            Destroy(root);
        }
    }

}
