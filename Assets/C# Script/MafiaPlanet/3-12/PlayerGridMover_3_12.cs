using UnityEngine;
using DG.Tweening;

public class PlayerGridMover_3_12 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider2D playerCollider;

    [Tooltip("이동 연출을 담당할 자식 오브젝트. Collider가 있는 루트는 넣지 않는다.")]
    [SerializeField] private Transform moveVisual;

    [SerializeField] private Animator animator;

    [Header("Swipe")]
    [SerializeField] private float minimumSwipePixels = 50f;

    [Header("Move Animation")]
    [SerializeField] private float moveTweenDuration = 0.18f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;

    private GridBoard_3_12 board;

    private Vector3 initialWorldPosition;
    private Vector3 baseVisualLocalPosition;

    private Vector2 pointerStart;
    private bool pointerDown;
    private bool inputEnabled;

    private bool hasQueuedMove;
    private Vector2Int queuedDirection;

    public Collider2D PlayerCollider => playerCollider;
    public Vector3Int CurrentCell { get; private set; }
    public bool HasQueuedMove => hasQueuedMove;
    public Vector2Int QueuedDirection => queuedDirection;

    private void Awake()
    {
        initialWorldPosition = transform.position;

        if (playerCollider == null)
            playerCollider = GetComponentInChildren<Collider2D>();

        if (moveVisual != null)
            baseVisualLocalPosition = moveVisual.localPosition;
    }

    public void Initialize(GridBoard_3_12 targetBoard)
    {
        board = targetBoard;

        CurrentCell = board.WorldToCell(initialWorldPosition);
        board.SnapToCell(transform, CurrentCell);

        hasQueuedMove = false;
        queuedDirection = Vector2Int.zero;
        inputEnabled = true;

        if (moveVisual != null)
        {
            moveVisual.DOKill();
            moveVisual.localPosition = baseVisualLocalPosition;
        }
    }

    public void SetInputEnabled(bool value)
    {
        inputEnabled = value;

        if (!value)
        {
            hasQueuedMove = false;
            queuedDirection = Vector2Int.zero;
            pointerDown = false;
        }
    }

    private void Update()
    {
        if (!inputEnabled || board == null)
            return;

        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        // 모바일 터치
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    BeginPointer(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndPointer(touch.position);
                    break;
            }

            return;
        }

        // 에디터 및 PC 테스트용 마우스 입력
        if (Input.GetMouseButtonDown(0))
            BeginPointer(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
            EndPointer(Input.mousePosition);
    }

    private void BeginPointer(Vector2 screenPosition)
    {
        pointerStart = screenPosition;
        pointerDown = true;
    }

    private void EndPointer(Vector2 screenPosition)
    {
        if (!pointerDown) return;

        pointerDown = false;

        Vector2 delta = screenPosition - pointerStart;

        if (delta.magnitude < minimumSwipePixels)
            return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            QueueMove(delta.x > 0f ? Vector2Int.right : Vector2Int.left);
        }
        else
        {
            QueueMove(delta.y > 0f ? Vector2Int.up : Vector2Int.down);
        }
    }

    public void QueueMove(Vector2Int direction)
    {
        if (!inputEnabled) return;

        // 같은 박자 전에 다시 스와이프하면 마지막 입력으로 교체된다.
        queuedDirection = direction;
        hasQueuedMove = true;
    }

    /// <summary>
    /// Move 박자에서 Minigame_3_12가 호출한다.
    /// </summary>
    public bool ResolveBeatMove()
    {
        if (!hasQueuedMove || board == null)
            return false;

        Vector2Int direction = queuedDirection;

        hasQueuedMove = false;
        queuedDirection = Vector2Int.zero;

        Vector3Int targetCell = CurrentCell;
        targetCell.x += direction.x;
        targetCell.y += direction.y;

        if (!board.CanEnter(targetCell))
            return false;

        MoveToCell(targetCell);
        return true;
    }

    private void MoveToCell(Vector3Int targetCell)
    {
        CurrentCell = targetCell;

        Vector3 previousVisualWorldPosition =
            moveVisual != null ? moveVisual.position : transform.position;

        board.SnapToCell(transform, targetCell);

        if (moveVisual == null)
            return;

        moveVisual.DOKill();

        // 실제 Collider 루트는 새 칸으로 즉시 이동시키고,
        // 그래픽만 이전 칸에서 새 칸으로 날아오는 것처럼 보이게 한다.
        moveVisual.position = previousVisualWorldPosition;

        moveVisual
            .DOLocalMove(baseVisualLocalPosition, moveTweenDuration)
            .SetEase(moveEase);
    }

    public void Revealed()
    {
        if (animator != null)
            animator.SetTrigger("Revealed");
    }
}