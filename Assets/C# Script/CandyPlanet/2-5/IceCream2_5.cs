using UnityEngine;

public class IceCream2_5 : MonoBehaviour
{
    private enum State { Fly, Drop, Conveyor }
    private State state = State.Fly;

    private Vector3 flyTarget;

    public float moveSpeed = 5f;
    public float fallSpeed = 5f;
    public float conveyorSpeed = 2f;

    public Transform conveyorPoint; // 이 y좌표에 도달하면 컨베이어 이동 시작
    public Transform destroyPoint;  // 이 x좌표 이하로 가면 파괴

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetFlyTarget(Vector3 pos)
    {
        flyTarget = pos;
        state = State.Fly;
    }

    public void Drop() => state = State.Drop;

    private void StartConveyorMove()
    {
        state = State.Conveyor;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
        }
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

                if (conveyorPoint != null && transform.position.y <= conveyorPoint.position.y)
                {
                    transform.position = new Vector3(transform.position.x, conveyorPoint.position.y, transform.position.z);
                    StartConveyorMove();
                }
                break;

            case State.Conveyor:
                transform.position += Vector3.left * conveyorSpeed * Time.deltaTime;

                if (destroyPoint != null && transform.position.x <= destroyPoint.position.x)
                    Destroy(gameObject);
                break;
        }
    }

    private void MoveTo(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }
}