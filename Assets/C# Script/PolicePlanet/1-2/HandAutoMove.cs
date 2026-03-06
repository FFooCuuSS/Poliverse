using UnityEngine;
using DG.Tweening;

public class HandAutoMove : MonoBehaviour
{
    [Header("Motion")]
    public float totalMoveDistance = 7f;
    public int steps = 4;
    public float pauseBetweenSteps = 0.2f;
    public float totalTravelTime = 1.65f;

    [Header("Polish")]
    public float punchDuration = 0.08f;
    public float punchStrengthY = 0.08f;
    public int punchVibrato = 6;
    public float punchElasticity = 0.6f;

    public bool hasArrived { get; private set; }

    private Vector3 startPos;
    private Sequence seq;

    private void Awake()
    {
        startPos = transform.position;
    }

    public void ResetToStart(bool active = true)
    {
        KillTween();
        hasArrived = false;
        gameObject.SetActive(active);
        transform.position = startPos;
    }

    public void StartMove()
    {
        KillTween();
        hasArrived = false;

        int safeSteps = Mathf.Max(1, steps);

        float stepDistance = totalMoveDistance / safeSteps;

        // pause 총합은 총 이동시간 안에 포함된다
        float totalPause = pauseBetweenSteps * Mathf.Max(0, safeSteps - 1);

        // 실제 step 이동에 쓸 수 있는 총 시간
        float moveTimeTotal = Mathf.Max(0.01f, totalTravelTime - totalPause);

        // step 하나당 이동 시간
        float moveDurationPerStep = moveTimeTotal / safeSteps;

        Vector3 basePos = transform.position;
        Vector3 finalPos = basePos + Vector3.down * totalMoveDistance;

        seq = DOTween.Sequence();

        for (int i = 0; i < safeSteps; i++)
        {
            Vector3 to = basePos + Vector3.down * stepDistance * (i + 1);

            // 핵심: 이동 시간이 totalTravelTime을 정확히 구성해야 함
            Tween moveTween = transform.DOMove(to, moveDurationPerStep).SetEase(Ease.OutCubic);
            seq.Append(moveTween);

            // punch는 시간을 추가로 먹지 않게 Join으로 겹친다
            if (punchDuration > 0f)
            {
                Tween punchTween = transform.DOPunchPosition(
                    new Vector3(0f, punchStrengthY, 0f),
                    punchDuration,
                    punchVibrato,
                    punchElasticity
                );

                seq.Join(punchTween);
            }

            if (i < safeSteps - 1 && pauseBetweenSteps > 0f)
                seq.AppendInterval(pauseBetweenSteps);
        }

        seq.OnComplete(() =>
        {
            // 마지막에 정확한 위치 보정
            transform.position = finalPos;
            hasArrived = true;
            seq = null;
        });
    }

    public void Despawn(float delay = 0.05f)
    {
        KillTween();
        hasArrived = false;

        if (delay <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        DOVirtual.DelayedCall(delay, () => gameObject.SetActive(false));
    }

    private void KillTween()
    {
        if (seq != null)
        {
            seq.Kill();
            seq = null;
        }

        DOTween.Kill(transform);
    }
}