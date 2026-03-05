using System.Collections;
using UnityEngine;

public class HandAutoMove : MonoBehaviour
{
    public float totalMoveDistance = 7f;   // 전체 내려오는 거리
    public float pauseDuration = 0.5f;     // 각 이동 사이 텀
    public int steps = 3;

    public bool hasArrived { get; private set; }

    private Vector3 startPos;
    private Coroutine job;

    private void Awake()
    {
        startPos = transform.position;
    }

    public void ResetToStart()
    {
        hasArrived = false;
        if (job != null) StopCoroutine(job);
        transform.position = startPos;
    }

    public void StartMove(float totalTime)
    {
        hasArrived = false;
        if (job != null) StopCoroutine(job);
        job = StartCoroutine(MoveInSteps(totalTime));
    }

    private IEnumerator MoveInSteps(float totalTime)
    {
        float stepDistance = totalMoveDistance / steps;

        // 총 시간에서 pause를 빼고, step당 이동 시간 산출
        float totalPause = pauseDuration * (steps - 1);
        float moveTimeTotal = Mathf.Max(0.01f, totalTime - totalPause);
        float moveDurationPerStep = Mathf.Max(0.01f, moveTimeTotal / steps);

        for (int i = 0; i < steps; i++)
        {
            Vector3 from = transform.position;
            Vector3 to = from + Vector3.down * stepDistance;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / moveDurationPerStep;
                transform.position = Vector3.Lerp(from, to, t);
                yield return null;
            }

            if (i < steps - 1)
                yield return new WaitForSeconds(pauseDuration);
        }

        hasArrived = true;
        job = null;
    }
}