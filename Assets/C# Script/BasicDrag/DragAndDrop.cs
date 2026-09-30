using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    [Header("Refs")]
    public Minigame_1_2 minigame;

    [Header("Constraint")]
    [SerializeField] protected float maxX = 7f;
    [SerializeField] protected float maxY = 4f;

    public bool isDragging = false;
    public bool banDragging = false;
    protected Vector3 offset;

    protected virtual void Awake()
    {
        // 만약 인스펙터에서 할당하지 않았다면 부모/씬에서 Minigame_1_2 자동 탐색
        if (minigame == null)
        {
            minigame = Object.FindFirstObjectByType<Minigame_1_2>();
        }
    }

    protected virtual void OnMouseDown()
    {
        if (minigame != null && minigame.IsDemoMode)
            return;

        if (banDragging)
            return;

        isDragging = true;
        Vector3 mouseWorldPos = GetMouseWorldPos();
        offset = transform.position - mouseWorldPos;
    }

    protected virtual void OnMouseUp()
    {
        isDragging = false;
    }

    protected virtual void Update()
    {
        if (minigame != null && minigame.IsDemoMode)
        {
            isDragging = false;
            return;
        }

        if (!isDragging || banDragging) return;

        Vector3 mouseWorldPos = GetMouseWorldPos();
        Vector3 targetPos = mouseWorldPos + offset;
        transform.position = GetConstrainedPosition(transform.position, targetPos);
    }

    protected virtual Vector3 GetConstrainedPosition(Vector3 current, Vector3 target)
    {
        float clampedX = Mathf.Clamp(target.x, -maxX, maxX);
        float clampedY = Mathf.Clamp(target.y, -maxY, maxY);
        return new Vector3(clampedX, clampedY, target.z);
    }

    protected Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}
