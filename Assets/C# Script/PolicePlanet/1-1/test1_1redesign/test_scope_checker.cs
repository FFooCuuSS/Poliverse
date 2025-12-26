using UnityEngine;

public class test_scope_checker : MonoBehaviour
{
    public test1_1game_manager manager;

    private Rigidbody2D rb;
    private Vector2 target;
    private bool move;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        target = rb.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 w = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target = new Vector2(w.x, w.y);
            move = true;
        }
    }

    void FixedUpdate()
    {
        if (!move) return;
        rb.MovePosition(target);
        move = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        manager.CheckHit(other);
    }
}
