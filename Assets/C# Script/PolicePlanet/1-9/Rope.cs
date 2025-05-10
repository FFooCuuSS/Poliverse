using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public Transform targetA;  // 왼쪽 끝
    public Transform targetB;  // 오른쪽 끝
    public RectTransform rectTransform; // 연결 이미지 (사각형 UI)

    void Update()
    {
        if (targetA == null || targetB == null || rectTransform == null)
            return;

        Vector3 posA = targetA.position;
        Vector3 posB = targetB.position;

        // 두 점의 중간 위치
        Vector3 center = (posA + posB) / 2f;
        rectTransform.position = center;

        // 두 점 간의 방향 벡터
        Vector3 direction = posB - posA;
        float distance = direction.magnitude;

        // 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        // 가로 길이만 distance로 설정, 세로 길이는 유지
        rectTransform.sizeDelta = new Vector2(distance, rectTransform.sizeDelta.y);
    }
}
