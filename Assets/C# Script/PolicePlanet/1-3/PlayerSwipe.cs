using UnityEngine;
using System.Collections;

public class PlayerSwipe : MonoBehaviour
{
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float moveStep = 1f;
    [SerializeField] private float moveDuration = 0.2f; 

    private Vector2 startPos;
    private bool isSwiping;
    private bool isMoving = false;

    private MiniGameBase miniGameBase;

    private void Awake()
    {
        miniGameBase = GetComponentInParent<MiniGameBase>();
    }

    void Update()
    {
        if (isMoving) return;
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

        float direction = deltaY > 0 ? 1f : -1f;
        Vector3 targetPos = transform.position + Vector3.up * moveStep * direction;

        StartCoroutine(SmoothMove(transform.position, targetPos));
    }

    private IEnumerator SmoothMove(Vector3 from, Vector3 to)
    {
        isMoving = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }

        transform.position = to; // 위치 보정
        isMoving = false;
    }
}
