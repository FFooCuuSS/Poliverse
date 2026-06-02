using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timetester3_2 : MonoBehaviour
{
    // 누적 시간
    float time = 0f;

    // 다음 출력 시간
    float nextLogTime = 0.5f;

    void Update()
    {
        // 시간 누적
        time += Time.deltaTime;

        // 0.5초마다 출력
        if (time >= nextLogTime)
        {
            Debug.Log("Current Time : " + time);

            // 다음 출력 시간 증가
            nextLogTime += 0.5f;
        }

        // 마우스 좌클릭 시 현재 시간 출력
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Left Click Time : " + time);
        }
    }
}