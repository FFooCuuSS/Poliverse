using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulamHammer2_3 : MonoBehaviour
{
    [Header("Pendulum Settings")]
    public float swingAngle = 45f; // 최대 각도 (도 단위)
    public float swingSpeed = 1f;  // 흔들림 속도
    public float startAngle = 0f;
    private float time;

    public Minigame_2_3 minigame;

    void Start()
    {
        time = startAngle;
    }

    void Update()
    {
        time += Time.deltaTime * swingSpeed;

        // 각도 계산
        float angle = swingAngle * Mathf.Sin(time);

        // 회전 적용
        transform.localRotation = Quaternion.Euler(0, 0, angle);

        // Input 타이밍에는 중앙 통과 체크만
        if (minigame != null && minigame.IsInputTiming)
        {
            // 중앙과의 거리 (정확히 중앙 통과 여부 확인)
            float distanceToCenter = Mathf.Abs(transform.localEulerAngles.z);
            // 필요 시 -180~180 변환해서 정확히 체크
        }
    }
}
