using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingIconrControllerbyHamme2_3 : MonoBehaviour
{
    public PendulamHammer2_3 hammer; // ���� �ظ� ��ũ��Ʈ ����
    public float moveDistance = 2f;  // �ִ� �¿� �̵� �Ÿ�
    public Vector3 moveAxis = Vector3.right; // �̵� ���� (�¿�)

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition; // �ʱ� ��ġ ���� (���� ��ǥ)
    }

    void Update()
    {
        if (hammer == null)
            return;

        // �ظ� ������ ���� (Z��)
        float angle = hammer.transform.localEulerAngles.z;
        // Unity EulerAngles.z�� 0~360 �����̹Ƿ� -180~180���� ��ȯ
        if (angle > 180f) angle -= 360f;

        // ������ -swingAngle ~ +swingAngle ���� -1~1 ������ ����ȭ
        float normalized = angle / hammer.swingAngle;

        // �¿� �̵� ���� �ȿ��� ��ġ ���
        Vector3 offset = moveAxis.normalized * moveDistance * normalized;

        // �ʱ� ��ġ�� ������ ���ϱ�
        transform.localPosition = startPosition + offset;
    }
}

