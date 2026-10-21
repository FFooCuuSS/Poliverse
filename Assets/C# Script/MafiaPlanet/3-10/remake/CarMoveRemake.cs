using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMoveRemake : MonoBehaviour
{

    [Header("효과음")]
    [SerializeField] private AudioClip collideSound;
    [Header("효과음")]
    [SerializeField] private AudioClip swipeSound;

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


    Rigidbody2D rb;
    Collider2D col;

    private bool isDemoMode;
    private GameObject warningSign1;
    private GameObject warningSign2;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        isSwiping = false;
    }

    public void SetWarningSigns(GameObject sign1, GameObject sign2)
    {
        warningSign1 = sign1;
        warningSign2 = sign2;
    }


    void Start()
    {
        targetPosition = lanes[currentLane];
        isFinished = false;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isFinished)
        {
            FreezeSelf();
            GameFinished();
            return;
        }

        if (isDemoMode)
        {
            HandleDemoMovement();
        }
        else
        {
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
                    if (swipeSound != null)
                    {
                        if (GameRoot.Instance != null &&
                            GameRoot.Instance.Audio != null)
                        {
                            GameRoot.Instance.Audio.PlaySfx(swipeSound);
                        }
                    }

                    if (swipeDeltaY > 0)
                        MoveUp();
                    else
                        MoveDown();

                    isSwiping = false;
                }
            }

            if (Input.GetMouseButtonUp(0))
                isSwiping = false;
        }

        // ★ 데모와 일반 모드 모두 자동차가 이동하도록 처리
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //

        if (collision.CompareTag("Enemy"))
        {
            if (collideSound != null)
            {
                if (GameRoot.Instance != null && GameRoot.Instance.Audio != null)
                {
                    GameRoot.Instance.Audio.PlaySfx(collideSound);
                }
            }

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

    private void HandleDemoMovement()
    {
        if (warningSign1 == null || warningSign2 == null)
            return;

        // 경고 표지판 두 개가 모두 켜졌을 때만 판단
        if (!warningSign1.activeSelf || !warningSign2.activeSelf)
            return;

        // 경고 표지판이 있는 도로의 Y 좌표
        float dangerY1 = warningSign1.transform.position.y;
        float dangerY2 = warningSign2.transform.position.y;

        // 세 도로 중 위험 표시가 없는 도로 찾기
        for (int i = 0; i < lanes.Length; i++)
        {
            float laneY = lanes[i].y;

            bool isDangerLane =
                Mathf.Abs(laneY - dangerY1) < 0.5f ||
                Mathf.Abs(laneY - dangerY2) < 0.5f;

            if (!isDangerLane)
            {
                currentLane = i;
                targetPosition = lanes[i];

                Debug.Log("[3-10 데모] 안전 도로 이동: " + i);
                return;
            }
        }
    }
}
