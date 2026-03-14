using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushMover3_8 : MonoBehaviour
{
    public float moveSpeed = 6.25f;     // 계산된 이동 속도
    public float destroyX = -10f;       // 이 위치 도달 시 삭제
    public float stopDuration = 0.5f;   // 좌클릭 정지 시간

    private bool isStopped = false;

    void Update()
    {
        if (!isStopped)
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        }

        if (transform.position.x <= destroyX)
        {
            Destroy(gameObject);
        }
    }

    public void StopForMoment()
    {
        StartCoroutine(StopRoutine());
    }

    IEnumerator StopRoutine()
    {
        isStopped = true;

        yield return new WaitForSeconds(stopDuration);

        isStopped = false;
    }
}
