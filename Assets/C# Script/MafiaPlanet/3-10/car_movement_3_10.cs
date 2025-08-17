using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class car_movement_3_10 : MiniGameBase
{
    private Vector3[] lanes = new Vector3[3] { new Vector3(-4, -3.2f, 0), new Vector3(-4, 0, 0), new Vector3(-4, 3.2f, 0) };
    private int currentLane = 1;
    private Vector3 targetPosition;

    private Vector2 swipeStartPos;
    private bool isSwiping = false;

    public float moveSpeed = 10f; // 이동 속도 조절용

    private bool isFail;

    void Start()
    {
        targetPosition = lanes[currentLane];
        isFail = false;
    }

    void Update()
    {
        // 스와이프 입력 처리
        if (Input.GetMouseButtonDown(0))
        {
            swipeStartPos = Input.mousePosition;
            isSwiping = true;
        }

        if (Input.GetMouseButton(0) && isSwiping)
        {
            Vector2 swipeCurrentPos = Input.mousePosition;
            float swipeDeltaY = swipeCurrentPos.y - swipeStartPos.y;

            if (Mathf.Abs(swipeDeltaY) > 50f)
            {
                if (swipeDeltaY > 0)
                    MoveUp();
                else
                    MoveDown();

                isSwiping = false;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isSwiping = false;
        }

        // 부드럽게 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            isFail = true;
            base.Fail();
            Debug.Log("Fail");
        }
    }

    void MoveUp()
    {
        if (currentLane < 2)
        {
            currentLane++;
            targetPosition = lanes[currentLane];
        }
    }

    void MoveDown()
    {
        if (currentLane > 0)
        {
            currentLane--;
            targetPosition = lanes[currentLane];
        }
    }
}
