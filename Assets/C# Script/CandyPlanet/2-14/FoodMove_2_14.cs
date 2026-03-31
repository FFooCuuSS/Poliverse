using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodMove_2_14 : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos = Vector3.zero;  // (0,0,0) 고정
    private float moveDuration;
    private float timer;
    private bool stopped = false;

    public void Init(float duration)
    {
        startPos = transform.position;
        moveDuration = duration;
        timer = 0f;
        stopped = false;
    }

    void Update()
    {
        if (stopped) return;

        timer += Time.deltaTime;
        float t = timer / moveDuration;
        transform.position = Vector3.Lerp(startPos, targetPos, t);

        // 목표 도착하면 멈추기
        if (t >= 1f)
            stopped = true;
    }

    public void StopMovement()
    {
        stopped = true;
    }
}
