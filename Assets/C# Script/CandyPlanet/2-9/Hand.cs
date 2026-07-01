using UnityEngine;

public class Hand : MonoBehaviour
{
    public float downY = 2f;
    public float upY = 5f;
    public float speed = 10f;

    [Tooltip("클릭 시점을 리듬 판정 입력으로 전달할 미니게임")]
    [SerializeField] private MiniGameBase minigame;

    private bool isMoving = false;
    private bool isDown = false;

    private Cloud grabbedCloud;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMoving)
        {
            isMoving = true;
            isDown = true;

            // 클릭 시점 = 판정 입력 시점. RhythmManager가 CSV의 Input 타이밍과 비교해 Perfect/Good/Miss를 매긴다.
            if (minigame != null)
            {
                minigame.OnPlayerInput();
            }
        }

        if (isMoving)
        {
            Move();
        }
    }

    void Move()
    {
        float targetY = isDown ? downY : upY;

        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(transform.position.x, targetY, 0),
            speed * Time.deltaTime
        );

        if (Mathf.Abs(transform.position.y - targetY) < 0.01f)
        {
            if (isDown)
            {
                isDown = false; // 다시 올라감
            }
            else
            {
                isMoving = false;

                // 올라온 후 처리
                if (grabbedCloud != null)
                {
                    grabbedCloud.ReleaseAndBreak();
                    grabbedCloud = null;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!isDown) return;

        Cloud cloud = col.GetComponent<Cloud>();

        if (cloud != null && grabbedCloud == null)
        {
            grabbedCloud = cloud;
            cloud.Grab(transform);
        }
    }
}