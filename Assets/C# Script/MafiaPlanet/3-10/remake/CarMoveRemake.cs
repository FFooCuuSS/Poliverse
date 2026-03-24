using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMoveRemake : MonoBehaviour
{
    private Vector3[] lanes = new Vector3[3] {
        new Vector3(-4, -3.2f, 0),
        new Vector3(-4,  0f,   0),
        new Vector3(-4,  3.2f, 0)
    };
    private int currentLane = 1;
    private Vector3 targetPosition;

    private Vector2 swipeStartPos;
    private bool isSwiping = false;

    public float moveSpeed = 10f;
    private bool isFinished;

    public GameObject Stage_3_10;
    Minigame_3_10_remake minigame_3_10;


    Rigidbody2D rb;
    Collider2D col;

    void Start()
    {
        targetPosition = lanes[currentLane];
        isFinished = false;
        minigame_3_10 = Stage_3_10.GetComponent<Minigame_3_10_remake>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isFinished)
        {
            FreezeSelf();   // 자동차 정지
            GameFinished();  // 게임 종료 처리
            return;
        }

        // === 스와이프 입력 처리 ===
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
                if (swipeDeltaY > 0) MoveUp();
                else MoveDown();
                isSwiping = false;
            }
        }

        if (Input.GetMouseButtonUp(0)) isSwiping = false;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //

        if (collision.CompareTag("Enemy"))
        {
            //부딫힌 상황 부여
            Debug.Log("collapsed");
        }
    }
    void GameFinished()
    {
        isFinished = true;
        DestroyAllEnemies();
       
    }

    void FreezeSelf()
    {
        moveSpeed = 0f;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }
        if (col != null) col.enabled = false;
    }

    void DestroyAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject e in enemies)
        {
            Destroy(e);
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
