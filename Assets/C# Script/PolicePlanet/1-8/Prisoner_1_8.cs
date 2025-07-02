using System.Collections;
using UnityEngine;

public class Prisoner_1_8 : MonoBehaviour
{
    public float moveSpeed = 0.1f;
    public GameObject prison;

    private Vector2 moveDirection;

    public float minX = -6.5f;
    public float maxX = 6.5f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    public float spawnMinDistance = 3f; // 감옥으로부터 최소 거리

    [Header("스프라이트")]
    public SpriteRenderer spriteRenderer;
    public Sprite leftSprite;
    public Sprite rightSprite;

    void Update()
    {
        UpdateMoveDirection();

        Vector2 newPosition = (Vector2)transform.position + moveDirection * moveSpeed * Time.deltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        transform.position = new Vector3(newPosition.x, newPosition.y, 0f);

        if (moveDirection.x < 0)
        {
            spriteRenderer.sprite = leftSprite;
        }
        else
        {
            spriteRenderer.sprite = rightSprite;
        }
    }

    void UpdateMoveDirection()
    {
        Vector2 prisonerPos = transform.position;
        Vector2 prisonPos = prison.transform.position;

        // 감옥에서의 방향 계산
        Vector2 awayFromPrison = (prisonerPos - prisonPos).normalized;

        //  y축 제거: x축 방향만 유지
        awayFromPrison.y = 0f;

        if (awayFromPrison == Vector2.zero)
        {
            //  랜덤 x축 방향 보정
            awayFromPrison = new Vector2(Random.value < 0.5f ? -1f : 1f, 0f);
        }

        moveDirection = awayFromPrison.normalized;
    }
}
