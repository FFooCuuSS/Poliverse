using System;
using UnityEngine;

public class PoliceMover : MonoBehaviour
{
    public int laneIndex;

    [Header("Auto destroy when x <")]
    public float destroyX = -8f;

    // 미니게임에게 "나 화면 밖으로 나가서 제거됐다" 알려주기
    public event Action<PoliceMover> OnAutoDestroyed;

    private float speed;          // 왼쪽으로 가는 속도(양수)
    private bool locked = false;

    // startX -> targetX까지 정확히 travelTime(=1초)에 도착하도록 speed 계산
    public void InitMoveToTarget(float startX, float targetX, float travelTime)
    {
        float dist = startX - targetX; // startX가 더 크니까 양수여야 정상
        speed = (travelTime <= 0f) ? 0f : dist / travelTime;
        if (speed < 0f) speed = -speed; // 혹시 뒤집혀도 안전하게

        // 위치는 호출하는 쪽에서 이미 잡아준다고 가정
    }

    private void Update()
    {
        if (locked) return;

        // 항상 같은 속도로 왼쪽 진행(컨테이너 지나가도 속도 동일)
        transform.position += Vector3.left * speed * Time.deltaTime;

        // 화면 밖으로 나가면 제거
        if (transform.position.x < destroyX)
        {
            OnAutoDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
    }

    // 클릭 성공 시: 즉시 현재 위치에서 고정
    public void LockHere()
    {
        locked = true;

        // Animator가 transform을 덮어쓰는 경우 방지
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;
    }

    public bool IsLocked => locked;
}
