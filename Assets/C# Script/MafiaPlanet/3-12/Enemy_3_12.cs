using UnityEngine;

public class Enemy_3_12 : MonoBehaviour
{
    [SerializeField] private GameObject stage_3_12;
    [SerializeField] private Collider2D playerCollider;

    [Header("AI 행동 세팅")]
    [SerializeField] private int enemyType;      // 0: 순찰, 1: 고정
    [SerializeField] private Vector2[] patrolPoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float actionInterval = 2f;
    [SerializeField] private float[] lookAngles;

    [Header("시야 컴포넌트")]
    [SerializeField] private EnemyVision_3_12 vision; // 자식에 붙은 컴포넌트

    private Minigame_3_12 minigame_3_12;

    private int currentPointIndex = 0;
    private int currentLookIndex = 0;
    private float timer = 2f;
    private bool isMoving = true;
    private bool alreadyTriggered = false;

    void Start()
    {
        minigame_3_12 = stage_3_12.GetComponent<Minigame_3_12>();

        if (enemyType == 0 && patrolPoints.Length == 1)
        {
            Vector2 startPos = transform.position;
            Vector2 patrol = patrolPoints[0];
            patrolPoints = new Vector2[] { startPos, patrol };
        }

        if (vision == null) vision = GetComponentInChildren<EnemyVision_3_12>(true);
    }

    void Update()
    {
        
        if (!alreadyTriggered && vision != null && playerCollider != null && vision.CanSeePlayer(playerCollider))
        {
            var bdd = playerCollider.GetComponentInParent<BlockingDirectionalDrag_3_12>();
            if (bdd != null) bdd.Revealed();

            minigame_3_12.Fail();
            isMoving = false;
            alreadyTriggered = true;
        }
        
        if (!isMoving) return;

        timer += Time.deltaTime;

        if (timer >= actionInterval)
        {
            timer = 0f;

            // 회전
            currentLookIndex = 1 - currentLookIndex;
            float angle = lookAngles[currentLookIndex];
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (enemyType == 0)
                currentPointIndex = 1 - currentPointIndex;
        }

        if (enemyType == 0)
        {
            Vector3 target = patrolPoints[currentPointIndex];
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        }
    }
}
