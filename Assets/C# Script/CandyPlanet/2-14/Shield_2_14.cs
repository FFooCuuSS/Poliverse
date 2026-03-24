using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield_2_14 : MonoBehaviour
{
    public Transform player;
    public float radius = 2f;

    void Update()
    {
        // 1. 마우스 위치 (스크린 → 월드)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // 2. 방향 벡터
        Vector3 dir = (mousePos - player.position).normalized;

        // 3. 원 위 위치 계산
        transform.position = player.position + dir * radius;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
