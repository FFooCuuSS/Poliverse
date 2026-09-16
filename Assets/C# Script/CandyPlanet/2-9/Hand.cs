using UnityEngine;

public class Hand : MonoBehaviour
{
    [Header("Sprite")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite grabSprite;

    [Header("Timing")]
    public float grabDuration = 0.2f; // grab 스프라이트로 유지되는 시간

    [Header("Grab 판정용 콜라이더 (평소 비활성화 상태로 둘 것)")]
    [SerializeField] private Collider2D grabCollider;

    [Tooltip("클릭 시점을 리듬 판정 입력으로 전달할 미니게임")]
    [SerializeField] private MiniGameBase minigame;

    private bool isGrabbing = false;
    private float timer = 0f;

    private Cloud grabbedCloud;

    private float lastClickTime = -1f;
    void Awake()
    {
        grabCollider.enabled = false; // 시작 시 항상 비활성화 상태 보장
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isGrabbing)
        {
            isGrabbing = true;
            timer = 0f;

            spriteRenderer.sprite = grabSprite;
            grabCollider.enabled = true; // 이미 겹쳐있는 구름과 OnTriggerEnter2D 발동

             // --- 클릭 시간 측정 디버그 ---
            float now = Time.time;
            float interval = lastClickTime < 0 ? 0f : now - lastClickTime;
            Debug.Log($"[Hand] 클릭 시각: {now:F3}s / 이전 클릭과 간격: {interval:F3}s");
            lastClickTime = now;
            // ---------------------------

            if (minigame != null)
            {
                minigame.OnPlayerInput();
            }
        }

        if (isGrabbing)
        {
            timer += Time.deltaTime;

            if (timer >= grabDuration)
            {
                isGrabbing = false;

                spriteRenderer.sprite = openSprite;
                grabCollider.enabled = false;

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
        Cloud cloud = col.GetComponent<Cloud>();

        if (cloud != null && grabbedCloud == null)
        {
            grabbedCloud = cloud;
            cloud.Grab(transform);
        }
    }
}