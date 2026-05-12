using UnityEngine;
using DG.Tweening;

public class PlayerSwipe : MonoBehaviour
{
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float moveDuration = 0.2f;

    [Header("Lane Settings")]
    [SerializeField] private float bottomY = 0f;
    [SerializeField] private float laneDistance = 1f;
    [SerializeField] private int currentLane = 0;
    // 0 = 아래, 1 = 위
    // 시작 y가 0이면 currentLane = 0

    private Vector2 startPos;
    private bool isSwiping;
    private bool isMoving;

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

    private float GetLaneY(int lane)
    {
        return lane == 0 ? bottomY : TopY;
    }
}