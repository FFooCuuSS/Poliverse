using DG.Tweening;
using UnityEngine;

public class Hole_3_3 : MonoBehaviour
{
    [Header("Snap")]
    [Tooltip("(구버전) 각도 판정 범위. 지금은 Manager_3_3의 시간 판정 결과로 맞춤 여부를 정한다")]
    [SerializeField] private float snapAngleRange = 10f;
    [SerializeField] private float snapDuration = 0.1f;

    private bool clockwise;
    private int index;

    private float speed;
    private bool locked = false;
    private bool aligned = false;

    private float targetTime;

    private System.Func<float> getTime;

    public bool IsLocked => locked;

    public void Init(float time, bool clockwise, int index, System.Func<float> timeProvider)
    {
        this.clockwise = clockwise;
        this.index = index;
        this.targetTime = time;
        this.getTime = timeProvider;

        locked = false;
        aligned = false;

        transform.DOKill();

        speed = 180f + (index * 40f);

        ApplyAngle();
    }

    // 각도를 매 프레임 '라운드 시간'으로 직접 계산 → 프레임 지연이 쌓이지 않고,
    // targetTime 순간에 정확히 0도(정렬)가 된다.
    private void ApplyAngle()
    {
        if (getTime == null) return;

        float dir = clockwise ? -1f : 1f;
        float angle = dir * speed * (getTime() - targetTime);

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        if (locked) return;
        ApplyAngle();
    }

    /// <summary>
    /// 판정 결과로 잠근다. 성공이면 0도로 맞춰주고, 실패면 그 자리에 멈춘다.
    /// </summary>
    public void LockWithResult(bool success)
    {
        if (locked) return;

        locked = true;
        aligned = success;

        if (success)
        {
            transform.DOKill();
            transform.DORotate(Vector3.zero, snapDuration).SetEase(Ease.OutBack);
        }
    }

    // 구버전 호환: 각도로 판정해서 잠금
    public void Lock()
    {
        float diff = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, 0f));
        LockWithResult(diff <= snapAngleRange);
    }

    public bool IsAligned()
    {
        return aligned;
    }
}