using UnityEngine;

public class IceCream2_5 : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;

    private bool hasPlayedClickSound = false;

    private enum State
    {
        Fly,
        Drop,
        Conveyor
    }

    private State state = State.Fly;

    private Vector3 flyTarget;

    public float moveSpeed = 5f;
    public float fallSpeed = 5f;
    public float conveyorSpeed = 2f;

    public Transform conveyorPoint;
    public Transform destroyPoint;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetFlyTarget(Vector3 pos)
    {
        flyTarget = pos;
        state = State.Fly;
    }

    public void Drop()
    {
        state = State.Drop;

        // 새로운 Drop이 시작되면 효과음 재생 가능하도록 초기화
        hasPlayedClickSound = false;
    }

    private void StartConveyorMove()
    {
        state = State.Conveyor;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
        }
    }

    private void Update()
    {
        switch (state)
        {
            case State.Fly:
                MoveTo(flyTarget);
                break;

            case State.Drop:

                transform.position +=
                    Vector3.down * fallSpeed * Time.deltaTime;

                // 효과음
                if (clickSound != null && !hasPlayedClickSound)
                {
                    // GameRoot가 없어도 게임이 멈추지 않도록 체크
                    if (GameRoot.Instance != null &&
                        GameRoot.Instance.Audio != null)
                    {
                        GameRoot.Instance.Audio.PlaySfx(clickSound);
                    }

                    hasPlayedClickSound = true;
                }

                // 컨베이어 높이에 도착
                if (conveyorPoint != null &&
                    transform.position.y <= conveyorPoint.position.y)
                {
                    transform.position = new Vector3(
                        transform.position.x,
                        conveyorPoint.position.y,
                        transform.position.z
                    );

                    StartConveyorMove();
                }

                break;

            case State.Conveyor:

                transform.position +=
                    Vector3.left * conveyorSpeed * Time.deltaTime;

                // 파괴 지점 도착
                if (destroyPoint != null &&
                    transform.position.x <= destroyPoint.position.x)
                {
                    Destroy(gameObject);
                }

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
    }
}