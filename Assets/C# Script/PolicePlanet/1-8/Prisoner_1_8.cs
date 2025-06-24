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


    void Update()
    {
        UpdateMoveDirection();

        Vector2 newPosition = (Vector2)transform.position + moveDirection * moveSpeed * Time.deltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        transform.position = new Vector3(newPosition.x, newPosition.y, 0f);
    }

    void UpdateMoveDirection()
    {
        Vector2 prisonerPos = transform.position;
        Vector2 prisonPos = prison.transform.position;

        Vector2 awayFromPrison = (prisonerPos - prisonPos).normalized;

        if (awayFromPrison == Vector2.zero)
        {
            awayFromPrison = Random.insideUnitCircle.normalized;
        }

        moveDirection = awayFromPrison;
    }
}
