using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public Transform targetA;  // ���� ��
    public Transform targetB;  // ������ ��
    public RectTransform rectTransform; // ���� �̹��� (�簢�� UI)

    void Update()
    {
        if (targetA == null || targetB == null || rectTransform == null)
            return;

        Vector3 posA = targetA.position;
        Vector3 posB = targetB.position;

        // �� ���� �߰� ��ġ
        Vector3 center = (posA + posB) / 2f;
        rectTransform.position = center;

        // �� �� ���� ���� ����
        Vector3 direction = posB - posA;
        float distance = direction.magnitude;

        // ȸ��
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        // ���� ���̸� distance�� ����, ���� ���̴� ����
        rectTransform.sizeDelta = new Vector2(distance, rectTransform.sizeDelta.y);
    }
}
