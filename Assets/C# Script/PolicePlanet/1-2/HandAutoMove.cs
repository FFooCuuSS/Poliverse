using System.Collections;
using UnityEngine;

public class HandAutoMove : MonoBehaviour
{
    public float totalMoveDistance = 7f;   // 전체 내려오는 거리
    public float moveDuration = 2.34f;     // 각 이동 시간
    public float pauseDuration = 0.5f;    // 각 이동 사이 텀

    public bool hasArrived { get; private set; }

    private Vector3 startPos;

    private void Awake()
    {
        startPos = transform.position;
    }

    public void StartMove()
    {
        hasArrived = false;
        StopAllCoroutines();
        StartCoroutine(MoveInSteps());
    }

    private IEnumerator MoveInSteps()
    {
        float stepDistance = totalMoveDistance / 3f;

        for (int i = 0; i < 3; i++)
        {
            Vector3 from = transform.position;
            Vector3 to = from + Vector3.down * stepDistance;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / moveDuration;
                transform.position = Vector3.Lerp(from, to, t);
                yield return null;
            }

            // 마지막 이동이 아니면 잠깐 멈춤
            if (i < 2)
                yield return new WaitForSeconds(pauseDuration);
        }

        hasArrived = true;
    }
}
