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
        if (grabCollider != null)
            grabCollider.enabled = false; // 시작 시 항상 비활성화 상태 보장
    }

    void Update()
    {
        // ★ 핵심: 데모 모드일 때는 마우스 클릭에 의한 플레이어 직접 입력 차단
        if (minigame is Minigame_2_9 minigame29 && minigame29.IsDemoMode)
            return;

        if (Input.GetMouseButtonDown(0) && !isGrabbing)
        {
            PerformGrabAction();

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
                ReleaseGrabAction();
            }
        }
    }

    // 실제 잡기 연출 실행
    private void PerformGrabAction()
    {
        isGrabbing = true;
        timer = 0f;

        if (spriteRenderer != null && grabSprite != null)
            spriteRenderer.sprite = grabSprite;

        if (grabCollider != null)
            grabCollider.enabled = true;

        float now = Time.time;
        float interval = lastClickTime < 0 ? 0f : now - lastClickTime;
        Debug.Log($"[Hand] 클릭 시각: {now:F3}s / 이전 클릭과 간격: {interval:F3}s");
        lastClickTime = now;
    }

    private void ReleaseGrabAction()
    {
        isGrabbing = false;

        if (spriteRenderer != null && openSprite != null)
            spriteRenderer.sprite = openSprite;

        if (grabCollider != null)
            grabCollider.enabled = false;

        if (grabbedCloud != null)
        {
            grabbedCloud.ReleaseAndBreak();
            grabbedCloud = null;
        }
    }

    // 데모 모드 전용 강제 액션 함수 추가
    public void TriggerDemoGrab()
    {
        if (isGrabbing) return;
        PerformGrabAction();
        // 데모 연출 종료를 위해 코루틴 활용 혹은 타이머 복귀
        StartCoroutine(DemoGrabRoutine());
    }

    private System.Collections.IEnumerator DemoGrabRoutine()
    {
        yield return new WaitForSeconds(grabDuration);
        ReleaseGrabAction();
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