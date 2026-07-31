using UnityEngine;
using DG.Tweening;

/// <summary>
/// 3. 접시(그릇) 이동 로직
///
/// 와이어프레임의 좌우 화살표처럼, 터치 드래그로 접시를 레인들 사이에서 수평 이동시킴.
/// 연속 드래그가 아니라 "레인 단위 스냅"으로 구현 (3~4개 레인 중 하나로 딱딱 이동).
/// 필요하면 snapMovement를 false로 바꿔 자유 드래그 + 손 뗄 때 가장 가까운 레인 스냅으로도 전환 가능.
/// </summary>
public class PlateController : MonoBehaviour
{
    [SerializeField] private Transform[] laneAnchors; // ChocoRingSpawner와 동일한 레인 배열을 공유해야 함
    [SerializeField] private float snapMoveDuration = 0.15f;
    [SerializeField] private Ease snapEase = Ease.OutQuad;

    [Header("드래그 감도 (스냅 모드일 때 스와이프 한 번 = 레인 한 칸)")]
    [SerializeField] private float swipeThresholdPixels = 40f;

    private int _currentLaneIndex;
    private bool _inputEnabled;
    private Vector2 _dragStartScreenPos;
    private bool _isDragging;

    private void Awake()
    {
        _currentLaneIndex = laneAnchors.Length / 2; // 가운데 레인에서 시작
        SnapToLane(_currentLaneIndex, instant: true);
    }

    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
    }

    public int CurrentLane => _currentLaneIndex;

    private void Update()
    {
        if (!_inputEnabled) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _dragStartScreenPos = Input.mousePosition;
            _isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            EvaluateSwipe((Vector2)Input.mousePosition - _dragStartScreenPos);
            _isDragging = false;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                _dragStartScreenPos = touch.position;
                _isDragging = true;
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (_isDragging)
                {
                    EvaluateSwipe(touch.position - _dragStartScreenPos);
                    _isDragging = false;
                }
                break;
        }
    }

    private void EvaluateSwipe(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) < swipeThresholdPixels) return; // 너무 짧은 드래그는 무시

        int direction = delta.x > 0 ? 1 : -1;
        int nextLane = Mathf.Clamp(_currentLaneIndex + direction, 0, laneAnchors.Length - 1);

        if (nextLane != _currentLaneIndex)
        {
            _currentLaneIndex = nextLane;
            SnapToLane(_currentLaneIndex, instant: false);
        }
    }

    private void SnapToLane(int laneIndex, bool instant)
    {
        Vector3 target = new Vector3(laneAnchors[laneIndex].position.x, transform.position.y, transform.position.z);

        if (instant)
        {
            transform.position = target;
        }
        else
        {
            transform.DOKill();
            transform.DOMove(target, snapMoveDuration).SetEase(snapEase);
        }
    }
}