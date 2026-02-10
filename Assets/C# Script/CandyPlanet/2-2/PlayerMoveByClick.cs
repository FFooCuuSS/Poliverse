using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveByClick : MonoBehaviour
{
    [SerializeField] private float moveX = 1.5f;
    [SerializeField] private float speed = 5f;

    private Vector3 targetPos;
    private bool isMoving = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMoving)
        {
            targetPos = transform.position + Vector3.right * moveX;
            isMoving = true;
        }

        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                speed * Time.deltaTime
            );

            if (transform.position == targetPos)
                isMoving = false;
        }
    }

    private void Move()
    {
        transform.position += new Vector3(moveX, 0f, 0f);
    }
}
