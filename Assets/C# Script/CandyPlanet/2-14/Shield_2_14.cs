using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield_2_14 : MonoBehaviour
{
    public Transform player;
    public float radius = 2f;

    private Minigame_2_14 miniGame;

    void Update()
    {
        // 마우스 위치 (스크린 → 월드)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // 방향 벡터
        Vector3 dir = (mousePos - player.position).normalized;

        // 원 위 위치 계산
        transform.position = player.position + dir * radius;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Start()
    {
        miniGame = GetComponentInParent<Minigame_2_14>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Food")) return;

        if (miniGame == null) return;

        if (miniGame.IsInputOpen)
        {
            Debug.Log("막기 성공! 음식 삭제");

            // 음식 이동 멈추기
            var foodMove = col.GetComponent<FoodMove_2_14>();
            if (foodMove != null)
            {
                foodMove.StopMovement();
            }

            // 음식 즉시 삭제
            Destroy(col.gameObject);

            miniGame.successCount++;
        }
        else
        {
            Debug.Log("타이밍 아님");
        }

    }
}
