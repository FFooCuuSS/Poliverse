using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulamHammer2_3 : MonoBehaviour
{
    [Header("Pendulum Settings")]
    public float swingAngle = 45f; // 최대 각도 (도 단위)
    public float swingSpeed = 1f;  // 흔들림 속도

    private float time;

    void Update()
    {
        time += Time.deltaTime * swingSpeed;

        // 사인파를 이용한 각도 계산
        float angle = swingAngle * Mathf.Sin(time);

        // Z축 회전 적용 (2D라면 Z축 회전, 3D라면 필요에 따라 수정)
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}