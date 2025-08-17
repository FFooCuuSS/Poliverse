using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class car_movement_3_10 : MonoBehaviour
{
    private Vector3[] lanes = new Vector3[3] { new Vector3(-4, -3.2f, 0), new Vector3(-4, 0, 0), new Vector3(-4, 3.2f, 0) };
    private int currentLane = 1;
    private Vector3 targetPosition;

    private Vector2 swipeStartPos;
    private bool isSwiping = false;

    public float moveSpeed = 10f; // �̵� �ӵ� ������

    private bool isFail;

    public GameObject Stage_3_10;
    Minigame_3_10 minigame_3_10;

    void Start()
    {
        targetPosition = lanes[currentLane];
        isFail = false;
        minigame_3_10 = Stage_3_10.GetComponent<Minigame_3_10>();
    }

    void Update()
    {
        // �������� �Է� ó��
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

        // �ε巴�� �̵�
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            isFail = true;
            minigame_3_10.MinigameFailed();
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
