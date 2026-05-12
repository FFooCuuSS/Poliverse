using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AutoMoveKey_3_3 : MonoBehaviour
{
    public float speed = -15f;

    private bool move = false;
    private bool blocked = false;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ResetState()
    {
        move = false;
        blocked = false;
    }

    public void StartMove()
    {
        move = true;
        blocked = false;
    }
    void FixedUpdate()
    {
        if (!move)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (blocked)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = Vector2.right * speed;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            blocked = true;
            rb.velocity = Vector2.zero;
        }
    }
}