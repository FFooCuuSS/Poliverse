using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BlockingDirectionalDrag : DirectionalDrag
{
    public GameObject stage_3_3;
    private Minigame_3_3 minigame_3_3;

    public BoxCollider2D matchCollider;  // 열쇠 앞부분 (isTrigger = true)
    public float successXThreshold = 5f; // 이 값을 넘으면 성공
    private bool hasTriggered = false;

    private Vector3 lastValidPosition;
    private bool isBlocked = false;

    private void Start()
    {
        minigame_3_3 = stage_3_3.GetComponent<Minigame_3_3>();
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();
        lastValidPosition = transform.position;
        isBlocked = false;
    }

    protected override void Update()
    {
        if (!isDragging || banDragging || isBlocked) return;

        Vector3 mouseWorldPos = GetMouseWorldPos();
        Vector3 targetPos = mouseWorldPos + offset;

        // 위치 제한 계산
        Vector3 constrainedTarget = GetConstrainedPosition(transform.position, targetPos);

        // 느리게 따라오기
        float followSpeed = 10f;
        transform.position = Vector3.Lerp(transform.position, constrainedTarget, followSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (!hasTriggered && !isBlocked && transform.position.x >= successXThreshold)
        {
            hasTriggered = true;
            minigame_3_3.Succeed();
        }
    }

    protected override Vector3 GetConstrainedPosition(Vector3 current, Vector3 target)
    {
        if (isBlocked)
        {
            return lastValidPosition;
        }

        Vector3 constrained = base.GetConstrainedPosition(current, target);

        lastValidPosition = constrained;
        return constrained;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            isBlocked = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            isBlocked = false;
        }
    }
}

