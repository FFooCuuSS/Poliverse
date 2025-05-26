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

    protected override void OnMouseDown()
    {
        minigame_3_3 = stage_3_3.GetComponent<Minigame_3_3>();
        base.OnMouseDown();
        lastValidPosition = transform.position;
        isBlocked = false;
    }

    protected override void Update()
    {
        base.Update();

        // 성공 트리거 조건
        if (!hasTriggered && transform.position.x >= successXThreshold)
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

        float angleRad = (angleInDegrees + 90f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;

        Vector3 delta = target - current;
        float projection = Vector2.Dot(delta, dir);
        Vector3 constrainedDelta = new Vector3(dir.x, dir.y, 0f) * projection;
        Vector3 constrainedTarget = current + constrainedDelta;

        lastValidPosition = constrainedTarget;
        return constrainedTarget;
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

