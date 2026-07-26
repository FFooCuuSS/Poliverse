using UnityEngine;

public class GridBoard_3_12 : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Grid grid;

    [Header("Collision")]
    [Tooltip("Wall, Enemy처럼 이동을 막는 레이어를 선택")]
    [SerializeField] private LayerMask blockingMask;

    [Tooltip("한 칸을 검사할 때 사용할 충돌 검사 크기")]
    [SerializeField] private Vector2 cellCheckSize = new Vector2(0.7f, 0.7f);

    [Header("Goal")]
    [SerializeField] private Transform goalMarker;

    public Vector3Int GoalCell
    {
        get
        {
            if (goalMarker == null)
                return Vector3Int.zero;

            return WorldToCell(goalMarker.position);
        }
    }

    private void Awake()
    {
        if (grid == null)
            grid = GetComponentInChildren<Grid>();
    }

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return grid.WorldToCell(worldPosition);
    }

    public Vector3 CellToWorld(Vector3Int cell)
    {
        return grid.GetCellCenterWorld(cell);
    }

    public bool CanEnter(Vector3Int cell)
    {
        Vector2 worldPosition = CellToWorld(cell);

        Collider2D hit = Physics2D.OverlapBox(
            worldPosition,
            cellCheckSize,
            0f,
            blockingMask
        );

        return hit == null;
    }

    public void SnapToCell(Transform target, Vector3Int cell)
    {
        Vector3 worldPosition = CellToWorld(cell);
        worldPosition.z = target.position.z;
        target.position = worldPosition;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (grid == null || goalMarker == null) return;

        Gizmos.color = Color.green;
        Vector3 center = grid.GetCellCenterWorld(grid.WorldToCell(goalMarker.position));
        Gizmos.DrawWireCube(center, cellCheckSize);
    }
#endif
}