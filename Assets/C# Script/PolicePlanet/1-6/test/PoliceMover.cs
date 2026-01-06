using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PoliceMover : MonoBehaviour
{
    [Header("Lane (0,1,2)")]
    public int laneIndex;

    [Header("After reaching target X")]
    public float passSpeed = 6f;

    [Header("Destroy when X <")]
    public float destroyX = -7f;

    private float startX;
    private float targetX;
    private float startTime;
    private float arriveTime;

    private bool isMoving = false;
    private bool isLocked = false;

    // Spawn 시점에 호출:
    // startTime~arriveTime 동안 targetX에 정확히 도착하고,
    // 이후에는 passSpeed로 계속 왼쪽 이동
    public void StartMoveTimed(float _startX, float _targetX, float _startTime, float _arriveTime)
    {
        startX = _startX;
        targetX = _targetX;
        startTime = _startTime;
        arriveTime = _arriveTime;

        isLocked = false;
        isMoving = true;

        Vector3 pos = transform.position;
        pos.x = startX;
        transform.position = pos;
    }

    private void Update()
    {
        if (isLocked) return;
        if (!isMoving) return;

        float now = Time.time;

        // 1) 도착 전: 시간 보간으로 정확히 목표 X까지 이동
        if (now <= arriveTime)
        {
            float t = Mathf.InverseLerp(startTime, arriveTime, now);
            float x = Mathf.Lerp(startX, targetX, t);

            Vector3 pos = transform.position;
            pos.x = x;
            transform.position = pos;
        }
        // 2) 도착 후: 계속 왼쪽으로 지나감
        else
        {
            transform.position += Vector3.left * passSpeed * Time.deltaTime;
        }

        // 3) 화면 밖 제거
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }

    // 성공 시: 현재 transform 위치 그대로 고정
    public void LockHere()
    {
        isLocked = true;
        isMoving = false;

        // Animator가 위치를 덮어쓰는 케이스가 종종 있어서 있으면 끔
        Animator anim = GetComponent<Animator>();
        if (anim != null)
            anim.enabled = false;
    }
}
