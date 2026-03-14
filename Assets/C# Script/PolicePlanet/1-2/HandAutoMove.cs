using UnityEngine;
using DG.Tweening;

public class HandAutoMove : MonoBehaviour
{
    [Header("Motion")]
    public float totalMoveDistance = 7f;
    public int steps = 4;
    public float totalTravelTime = 1.65f;

    [Header("Step Feel")]
    public float stepMoveDuration = 0.14f; // 실제로 내려가는 시간

    public bool hasArrived { get; private set; }

    private Vector3 startPos;
    private Tween currentTween;
    private Tween despawnTween;

    private int currentStep = 0;
    private float stepDistance;

    private void Awake()
    {
        startPos = transform.position;
        Recalculate();
    }

    private void Recalculate()
    {
        int safeSteps = Mathf.Max(1, steps);
        stepDistance = totalMoveDistance / safeSteps;
    }

    public float GetStepInterval()
    {
        int safeSteps = Mathf.Max(1, steps);
        return totalTravelTime / safeSteps;
    }

    public float GetActualStepMoveDuration()
    {
        float interval = GetStepInterval();
        return Mathf.Min(stepMoveDuration, interval);
    }

    public void ResetToStart(bool active = true)
    {
        KillDespawnTween();
        KillCurrentTween(false);
        Recalculate();

        hasArrived = false;
        currentStep = 0;

        gameObject.SetActive(active);
        transform.position = startPos;
    }

    public void ForceStep(int stepIndex)
    {
        Recalculate();

        int safeSteps = Mathf.Max(1, steps);
        stepIndex = Mathf.Clamp(stepIndex, 0, safeSteps);

        KillDespawnTween();
        KillCurrentTween(true);

        currentStep = stepIndex;
        transform.position = startPos + Vector3.down * stepDistance * currentStep;

        if (currentStep >= safeSteps)
        {
            hasArrived = true;
            return;
        }

        Vector3 nextPos = startPos + Vector3.down * stepDistance * (currentStep + 1);
        float moveDur = GetActualStepMoveDuration();

        currentTween = transform.DOMove(nextPos, moveDur)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                currentStep++;
                if (currentStep >= safeSteps)
                    hasArrived = true;

                currentTween = null;
            });
    }

    public void ForceFinish()
    {
        Recalculate();
        KillDespawnTween();
        KillCurrentTween(false);

        currentStep = Mathf.Max(1, steps);
        transform.position = startPos + Vector3.down * totalMoveDistance;
        hasArrived = true;
    }

    public void Despawn(float delay = 0.05f)
    {
        KillDespawnTween();
        KillCurrentTween(false);

        hasArrived = false;
        currentStep = 0;

        if (delay <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        despawnTween = DOVirtual.DelayedCall(delay, () =>
        {
            gameObject.SetActive(false);
            despawnTween = null;
        });
    }

    private void KillCurrentTween(bool complete)
    {
        if (currentTween != null)
        {
            if (currentTween.IsActive())
            {
                if (complete) currentTween.Complete();
                else currentTween.Kill();
            }
            currentTween = null;
        }
    }

    private void KillDespawnTween()
    {
        if (despawnTween != null)
        {
            if (despawnTween.IsActive())
                despawnTween.Kill();

            despawnTween = null;
        }
    }

    private void OnDisable()
    {
        KillDespawnTween();
        KillCurrentTween(false);
    }
}