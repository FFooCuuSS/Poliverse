using UnityEngine;

public class test_click_manager : MonoBehaviour
{
    public Transform scope;

    private Rigidbody2D rb;
    private Vector2 targetPos;
    private bool hasTarget = false;

    void Awake()
    {
        rb = scope.GetComponent<Rigidbody2D>();
        targetPos = rb.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            targetPos = new Vector2(mouseWorld.x, mouseWorld.y);
            hasTarget = true;
        }
    }

    void FixedUpdate()
    {
        if (!hasTarget) return;

        rb.MovePosition(targetPos);
        hasTarget = false;
    }
}
