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

        float totalPause = pauseBetweenSteps * Mathf.Max(0, safeSteps - 1);
        float moveTimeTotal = Mathf.Max(0.01f, totalTravelTime - totalPause);
        float moveDurationPerStep = moveTimeTotal / safeSteps;

        Vector3 basePos = startPos; // ¡Ú Áß¿ä
        Vector3 finalPos = basePos + Vector3.down * totalMoveDistance;

        transform.position = basePos;

        seq = DOTween.Sequence();

        for (int i = 0; i < safeSteps; i++)
        {
            Vector3 to = basePos + Vector3.down * stepDistance * (i + 1);

            seq.Append(
                transform.DOMove(to, moveDurationPerStep)
                .SetEase(Ease.OutCubic)
            );

            if (punchDuration > 0f)
            {
                seq.Join(
                    transform.DOPunchPosition(
                        Vector3.up * punchStrengthY,
                        punchDuration,
                        punchVibrato,
                        punchElasticity
                    )
                    .SetEase(Ease.OutQuad)
                );
            }

            if (i < safeSteps - 1)
                seq.AppendInterval(pauseBetweenSteps);
        }

        seq.OnComplete(() =>
        {
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