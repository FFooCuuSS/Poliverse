using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCream2_5 : MonoBehaviour
{
    private enum State
    {
        Fly,
        Drop,
        Stored
    }

    private State state = State.Fly;

    private Vector3 flyTarget;
    private Vector3 storeTarget;

    public float moveSpeed = 5f;
    public float fallSpeed = 5f;

    private bool landed = false;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void StartStoreMove()
    {
        state = State.Stored;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
        }
    }

    public void SetFlyTarget(Vector3 pos)
    {
        flyTarget = pos;
        state = State.Fly;
    }

    public void SetStoreTarget(Vector3 pos)
    {
        storeTarget = new Vector3(pos.x, transform.position.y, transform.position.z);
    }

    public void Drop()
    {
        state = State.Drop;
    }

    void Update()
    {
        switch (state)
        {
            case State.Fly:
                MoveTo(flyTarget);
                break;

            case State.Drop:
                transform.position += Vector3.down * fallSpeed * Time.deltaTime;
                break;

            case State.Stored:
                MoveTo(storeTarget);
                break;
        }
    }

    private void MoveTo(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            if (state == State.Stored)
            {
                enabled = false; // 여기서만 종료
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Floor")) return;
        if (landed) return;

        landed = true;

        FindAnyObjectByType<IceCreamFloor>()
            .OnIceCreamLanded(transform);
    }
}
