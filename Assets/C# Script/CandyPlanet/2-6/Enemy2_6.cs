using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2_6 : MonoBehaviour
{
    [SerializeField]private float speed = 1000f; // �ȼ�/�� �ӵ�

    private RectTransform rectTransform;
    private float screenLeftEdge;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        screenLeftEdge = -1000f; // ��ũ�� ���� �� (x = 0)
    }

    void Update()
    {
        // ���� �������� �̵�
        rectTransform.anchoredPosition += Vector2.left * speed * Time.deltaTime;

        // ȭ�� ���� ������ ������ ����
        if (rectTransform.anchoredPosition.x + rectTransform.rect.width < screenLeftEdge)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider2D other)
    {
        if (other.CompareTag("Cake"))
        {
            Debug.Log("GameOver");
        }
    }
}
