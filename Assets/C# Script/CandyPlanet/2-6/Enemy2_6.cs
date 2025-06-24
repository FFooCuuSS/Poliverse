using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2_6 : MonoBehaviour
{
    [SerializeField]private float speed = 1000f; // 픽셀/초 속도

    private RectTransform rectTransform;
    private float screenLeftEdge;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        screenLeftEdge = -1000f; // 스크린 왼쪽 끝 (x = 0)
    }

    void Update()
    {
        // 왼쪽 방향으로 이동
        rectTransform.anchoredPosition += Vector2.left * speed * Time.deltaTime;

        // 화면 왼쪽 밖으로 나가면 삭제
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
