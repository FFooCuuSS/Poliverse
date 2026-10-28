using UnityEngine;
using DG.Tweening;

public class PlayerSwipe : MonoBehaviour
{
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float moveDuration = 0.2f;
    [Header("효과음")]
    [SerializeField] private AudioClip swipeSound;
    [Header("Lane Settings")]
    [SerializeField] private float bottomY = 0f;
    [SerializeField] private float laneDistance = 1f;
    [SerializeField] private int currentLane = 0;
    // 0 = 아래, 1 = 위
    // 시작 y가 0이면 currentLane = 0

    private Vector2 startPos;
    private bool isSwiping;
    private bool isMoving;

    // 리스폰 중 등 외부에서 입력을 막을 때
    [HideInInspector] public bool inputBlocked;

    private MiniGameBase miniGameBase;

    private float TopY => bottomY + laneDistance;

    private void Awake()
    {
        miniGameBase = GetComponentInParent<MiniGameBase>();

        // 시작 위치 보정
        Vector3 pos = transform.position;
        pos.y = GetLaneY(currentLane);
        transform.position = pos;
    }

    void Update()
    {
        Minigame_1_3 demoGame =
        FindObjectOfType<Minigame_1_3>();

        if (demoGame != null && demoGame.IsDemoMode) return;

        if (inputBlocked) return;
        if (isMoving) return;
        if (miniGameBase != null && miniGameBase.IsInputLocked) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        MouseInput();
#else
        TouchInput();
#endif
    }

    private void MouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isSwiping = true;
        }

        if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            HandleSwipe(Input.mousePosition);
            isSwiping = false;
        }
    }

    private void TouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            startPos = touch.position;
            isSwiping = true;
        }

        if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isSwiping)
        {
            HandleSwipe(touch.position);
            isSwiping = false;
        }
    }

    private void HandleSwipe(Vector2 endPos)
    {
        if (swipeSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
        {
            GameRoot.Instance.Audio.PlaySfx(swipeSound);
        }

        float deltaY = endPos.y - startPos.y;

        if (Mathf.Abs(deltaY) < swipeThreshold)
            return;

        int nextLane = currentLane;

        if (deltaY > 0)
        {
            nextLane = 1;
        }
        else
        {
            nextLane = 0;
        }

        if (nextLane == currentLane)
            return;

        currentLane = nextLane;

        Vector3 targetPos = transform.position;
        targetPos.y = GetLaneY(currentLane);

        isMoving = true;
        transform.DOKill();

        transform.DOMove(targetPos, moveDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => isMoving = false);
    }

    // 리스폰 등으로 위치가 강제로 바뀌었을 때 레인 상태를 맞춘다.
    // (기존엔 위 레인에서 실패 → 아래로 리스폰돼도 currentLane=1로 남아서
    //  위로 스와이프해도 '같은 레인'으로 판단돼 움직이지 않았다)
    public void SyncLaneToY(float y)
    {
        float mid = (bottomY + TopY) * 0.5f;
        currentLane = y >= mid ? 1 : 0;

        isMoving = false;
        isSwiping = false;
    }

    private float GetLaneY(int lane)
    {
        return lane == 0 ? bottomY : TopY;
    }
}