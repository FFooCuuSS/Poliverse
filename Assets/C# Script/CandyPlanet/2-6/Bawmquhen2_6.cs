using UnityEngine;

public class Bawmquhen2_6 : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float returnSpeed = 5f;
    public float maxX = 300f;

    private bool isDragging = false;
    private float targetX = 0f;

    void Update()
    {
        HandleInput();
        Move();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            float mouseX = Input.mousePosition.x;
            float normalized = (mouseX / Screen.width - 0.5f) * 2f;

            targetX = normalized * maxX;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            targetX = 0f;
        }
    }

    void Move()
    {
        Vector3 pos = transform.position;

        float speed = isDragging ? moveSpeed : returnSpeed;

        pos.x = Mathf.MoveTowards(pos.x, targetX, speed * Time.deltaTime);

        transform.position = pos;
    }
}
