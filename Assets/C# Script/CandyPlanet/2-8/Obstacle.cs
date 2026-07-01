using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private float destroyY = -10f; // 화면 아래로 벗어나면 정리

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true; // 물리 충돌 대신 트리거로만 반응
    }

    private void Update()
    {
        // 순수 연출용 낙하. 실제 판정은 Minigame_2_8이 리듬 박자("Input")에 맞춰 처리함
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Floor")) // 발판 콜라이더
        {
            // 게임 로직(각도 변화)은 여기서 처리하지 않음. 연출 정리만.
            Destroy(gameObject);
        }
    }
}