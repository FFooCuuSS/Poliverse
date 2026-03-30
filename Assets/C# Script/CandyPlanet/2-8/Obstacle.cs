using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerRotate playerRotate;
    private float direction;

    public void Init(PlayerRotate player, float dir)
    {
        playerRotate = player;
        direction = dir;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // 처음엔 안 떨어짐
    }

    public void Drop()
    {
        rb.gravityScale = 1f; // 이 순간부터 떨어짐
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Floor")) // 발판 콜라이더
        {
            playerRotate.AddImpactAngle(direction * 15f);
        }
    }
}
