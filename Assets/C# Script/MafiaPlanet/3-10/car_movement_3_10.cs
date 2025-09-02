using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class car_movement_3_10 : MonoBehaviour
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
    private bool isFail;

    public GameObject Stage_3_10;
    Minigame_3_10 minigame_3_10;

    public instantiate_3_10_box spawner;   // 스포너 참조 (씬에서 할당)

    Rigidbody2D rb;
    Collider2D col;

    void Start()
    {
        targetPosition = lanes[currentLane];
        isFail = false;
        minigame_3_10 = Stage_3_10.GetComponent<Minigame_3_10>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isFail)
        {
            FreezeSelf();   // 자동차 정지
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
        if (isFail) return;

        if (collision.CompareTag("Enemy"))
        {
            isFail = true;
            FreezeSelf();             // 자동차 멈춤
            DestroyAllEnemies();      // 소환된 적 전부 파괴
            if (spawner != null)      // 스포너 정지
                spawner.StopSpawning();

            minigame_3_10.MinigameFailed();
            Debug.Log("Fail");
        }
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
