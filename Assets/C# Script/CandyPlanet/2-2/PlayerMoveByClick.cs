using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveByClick : MonoBehaviour
{
    [SerializeField] private float moveX = 1.5f;
    [SerializeField] private float speed = 5f;

    private Vector3 targetPos;

    public bool isMoving = false;
    public bool canMove = false;


    void OnEnable()
    {
        Icicle.OnMoveBlocked += BlockMove;
        Icicle.OnMoveAllowed += AllowMove;
    }

    void OnDisable()
    {
        Icicle.OnMoveBlocked -= BlockMove;
        Icicle.OnMoveAllowed -= AllowMove;
    }
    private void BlockMove()
    {
        canMove = false;
    }
    private void AllowMove()
    {
        canMove = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMoving && canMove)
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
