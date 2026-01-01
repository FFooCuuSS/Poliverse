using UnityEngine;

public class PlayerSwipe : MonoBehaviour
{
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float moveStep = 1f;

    private Vector2 startPos;
    private bool isSwiping;

    private MiniGameBase miniGameBase;

    private void Awake()
    {
        miniGameBase = GetComponentInParent<MiniGameBase>();
    }

    void Update()
    {
        if (miniGameBase != null && miniGameBase.IsInputLocked)
            return;

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

        // 입력 전달 (리듬 판정용)
        miniGameBase?.OnPlayerInput("Swipe");
        Debug.Log("스와이프");

        // 실제 이동 (게임 로직)
        if (deltaY > 0)
            MoveUp();
        else
            MoveDown();
    }

    private void MoveUp()
    {
        transform.position += new Vector3(0f, moveStep, 0f);
    }

    private void MoveDown()
    {
        transform.position += new Vector3(0f, -moveStep, 0f);
    }
}
