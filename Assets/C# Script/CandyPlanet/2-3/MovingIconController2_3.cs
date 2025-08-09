using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingIconController2_3 : MonoBehaviour
{
    public Transform barTransform;   // �� ��ü ������ Transform
    public float barWidth = 5f;       // ���� ���� �� (���� ����)
    public float speed = 2f;          // ������ �ӵ� (����/��)

    private Transform tf;
    private bool movingRight = true;

    void Start()
    {
        tf = GetComponent<Transform>();
    }

    void Update()
    {
        float delta = speed * Time.deltaTime;

        if (movingRight)
        {
            tf.position += new Vector3(delta, 0, 0);
            if (tf.position.x >= barTransform.position.x + barWidth / 2)
                movingRight = false;
        }
        else
        {
            tf.position -= new Vector3(delta, 0, 0);
            if (tf.position.x <= barTransform.position.x - barWidth / 2)
                movingRight = true;
        }
    }
}
