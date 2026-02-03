using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PlayerHold : MonoBehaviour
{
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float moveStep = 1f;
    [SerializeField] private float moveDuration = 0.3f;

    [SerializeField] private float followSpeed = 0.1f;
    [SerializeField] private float holdTargetY = -3.5f;

    private bool isHolding;
    private Tween holdTween;

    private float startY;
    private bool isMoving;

    private MiniGameBase miniGameBase;

    private void Awake()
    {
        miniGameBase = GetComponentInParent<MiniGameBase>();
        startY = transform.position.y;
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
            StartHold();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndHold();
        }
    }

    private void TouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
            StartHold();

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            EndHold();
    }

    private void StartHold()
    {
        if (isMoving) return;

        isHolding = true;
        transform.DOKill();

        holdTween = transform.DOMoveY(holdTargetY, moveDuration)
            .SetEase(Ease.OutCubic);
    }

    private void EndHold()
    {
        isHolding = false;
        transform.DOKill();
        transform.DOMoveY(startY, moveDuration)
            .SetEase(Ease.OutCubic);
    }
}

