using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulamHammer2_3 : MonoBehaviour
{
    [Header("Pendulum Settings")]
    public float swingAngle = 45f; // �ִ� ���� (�� ����)
    public float swingSpeed = 1f;  // ��鸲 �ӵ�

    private float time;

    void Update()
    {
        time += Time.deltaTime * swingSpeed;

        // �����ĸ� �̿��� ���� ���
        float angle = swingAngle * Mathf.Sin(time);

        // Z�� ȸ�� ���� (2D��� Z�� ȸ��, 3D��� �ʿ信 ���� ����)
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}