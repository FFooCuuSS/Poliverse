using System;
using UnityEngine;
using DG.Tweening;

public class Enemy_3_12 : MonoBehaviour
{
    public enum Facing
    {
        Up,
        Right,
        Down,
        Left
    }

    [Serializable]
    public struct BeatAction
    {
        [Tooltip("이 박자에 이동할 셀 수. 고정 적은 (0, 0)")]
        public Vector2Int moveDelta;

        [Tooltip("이동 후 바라볼 방향")]
        public Facing facing;
    }

    [Header("References")]
    [SerializeField] private EnemyVision_3_12 vision;

    [Tooltip("SpriteRenderer 등을 담은 자식 오브젝트")]
    [SerializeField] private Transform moveVisual;

    [Header("Initial State")]
    [SerializeField] private Facing initialFacing = Facing.Down;

    [Header("Beat Pattern")]
    [SerializeField] private BeatAction[] beatPattern;
    [SerializeField] private bool loopPattern = true;

    [Header("Animation")]
    [SerializeField] private float moveTweenDuration = 0.18f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;

    private GridBoard_3_12 board;

    private Vector3 initialWorldPosition;
    private Vector3 baseVisualLocalPosition;
    private Quaternion baseVisualLocalRotation;

    private Vector3Int currentCell;
    private int patternIndex;

    public Vector3Int CurrentCell => currentCell;

    private void Awake()
    {
        initialWorldPosition = transform.position;

        if (vision == null)
            vision = GetComponentInChildren<EnemyVision_3_12>(true);

        if (moveVisual != null)
        {
            baseVisualLocalPosition = moveVisual.localPosition;
            baseVisualLocalRotation = moveVisual.localRotation;
        }
    }

    public void Initialize(GridBoard_3_12 targetBoard)
    {
        board = targetBoard;
        patternIndex = 0;

        currentCell = board.WorldToCell(initialWorldPosition);
        board.SnapToCell(transform, currentCell);

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            GetFacingAngle(initialFacing)
        );

        if (moveVisual != null)
        {
            moveVisual.DOKill();
            moveVisual.localPosition = baseVisualLocalPosition;
            moveVisual.localRotation = baseVisualLocalRotation;
        }
    }

    /// <summary>
    /// Move 박자마다 한 번 호출한다.
    /// </summary>
    public void ResolveBeatAction()
    {
        if (board == null || beatPattern == null || beatPattern.Length == 0)
            return;

        if (patternIndex >= beatPattern.Length)
        {
            if (!loopPattern)
                return;

            patternIndex = 0;
        }

        BeatAction action = beatPattern[patternIndex];
        patternIndex++;

        Vector3Int targetCell = currentCell;
        targetCell.x += action.moveDelta.x;
        targetCell.y += action.moveDelta.y;

        bool hasMovement = action.moveDelta != Vector2Int.zero;

        if (hasMovement && !board.CanEnter(targetCell))
            targetCell = currentCell;

        ApplyPose(targetCell, action.facing);
    }

    private void ApplyPose(Vector3Int targetCell, Facing facing)
    {
        Vector3 oldVisualWorldPosition =
            moveVisual != null ? moveVisual.position : transform.position;

        Quaternion oldVisualWorldRotation =
            moveVisual != null ? moveVisual.rotation : transform.rotation;

        currentCell = targetCell;

        board.SnapToCell(transform, currentCell);
        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            GetFacingAngle(facing)
        );

        if (moveVisual == null)
            return;

        moveVisual.DOKill();

        // 판정용 루트와 시야는 즉시 새 위치/방향으로 바뀐다.
        // 그래픽만 이전 자세에서 새 자세로 Tween된다.
        moveVisual.position = oldVisualWorldPosition;
        moveVisual.rotation = oldVisualWorldRotation;

        moveVisual
            .DOLocalMove(baseVisualLocalPosition, moveTweenDuration)
            .SetEase(moveEase);

        moveVisual
            .DOLocalRotateQuaternion(baseVisualLocalRotation, moveTweenDuration)
            .SetEase(Ease.OutQuad);
    }

    public bool CanSeePlayer(Collider2D playerCollider)
    {
        return vision != null &&
               playerCollider != null &&
               vision.CanSeePlayer(playerCollider);
    }

    private static float GetFacingAngle(Facing facing)
    {
        switch (facing)
        {
            case Facing.Up:
                return 0f;

            case Facing.Right:
                return -90f;

            case Facing.Down:
                return 180f;

            case Facing.Left:
                return 90f;

            default:
                return 0f;
        }
    }
}