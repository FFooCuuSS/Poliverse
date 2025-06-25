using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingIconController2_3 : MonoBehaviour
{
    public RectTransform barRect;      // �� ��ü ����
    public float speed = 100f;         // ������ �ӵ� (px/��)

    private RectTransform rectTransform;
    private bool movingRight = true;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        float delta = speed * Time.deltaTime;

        if (movingRight)
        {
            rectTransform.anchoredPosition += new Vector2(delta, 0);
            if (rectTransform.anchoredPosition.x >= barRect.rect.width / 2)
                movingRight = false;
        }
        else
        {
            rectTransform.anchoredPosition -= new Vector2(delta, 0);
            if (rectTransform.anchoredPosition.x <= -barRect.rect.width / 2)
                movingRight = true;
        }
    }
}
