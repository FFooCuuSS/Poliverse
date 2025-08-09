using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingIconrControllerbyHamme2_3 : MonoBehaviour
{
    public PendulamHammer2_3 hammer; // 흔드는 해머 스크립트 참조
    public float moveDistance = 2f;  // 최대 좌우 이동 거리
    public Vector3 moveAxis = Vector3.right; // 이동 방향 (좌우)

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition; // 초기 위치 저장 (로컬 좌표)
    }

    void Update()
    {
        if (hammer == null)
            return;

        // 해머 각도를 읽음 (Z축)
        float angle = hammer.transform.localEulerAngles.z;
        // Unity EulerAngles.z는 0~360 범위이므로 -180~180으로 변환
        if (angle > 180f) angle -= 360f;

        // 각도를 -swingAngle ~ +swingAngle 기준 -1~1 값으로 정규화
        float normalized = angle / hammer.swingAngle;

        // 좌우 이동 범위 안에서 위치 계산
        Vector3 offset = moveAxis.normalized * moveDistance * normalized;

        // 초기 위치에 오프셋 더하기
        transform.localPosition = startPosition + offset;
    }
}

