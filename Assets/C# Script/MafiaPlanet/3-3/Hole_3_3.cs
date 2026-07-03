using DG.Tweening;
using UnityEngine;

public class Hole_3_3 : MonoBehaviour
{
    [Header("Snap")]
    [SerializeField] private float snapAngleRange = 10f;
    [SerializeField] private float snapDuration = 0.1f;

    private bool clockwise;
    private int index;

    private float speed;
    private bool locked = false;

    private float targetTime;

    private System.Func<float> getTime;

    public void Init(float time, bool clockwise, int index, System.Func<float> timeProvider)
    {
        this.clockwise = clockwise;
        this.index = index;
        this.targetTime = time;
        this.getTime = timeProvider;

        locked = false;

        transform.DOKill();

        speed = 180f + (index * 40f);

        float dir = clockwise ? -1f : 1f;
        float startAngle = 0f - (dir * speed * targetTime);

        transform.rotation = Quaternion.Euler(0, 0, startAngle);
    }

    void Update()
    {
        if (locked) return;

        float dir = clockwise ? -1f : 1f;
        transform.Rotate(0, 0, dir * speed * Time.deltaTime);
    }

    public void Lock()
    {
        if (locked) return;

        float diff = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, 0f));

        locked = true;

        if (diff <= snapAngleRange)
        {
            transform.DOKill();

            transform.DORotate(Vector3.zero, snapDuration)
                .SetEase(Ease.OutBack);

            Debug.Log($"[3-3] Hole {index} SNAP / diff:{diff}");
        }
    }

    public bool IsAligned()
    {
        float angle = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, 0f));
        return angle <= snapAngleRange;
    }
}