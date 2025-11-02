using UnityEngine;

public class PlayerDrag : DragAndDrop
{
    [SerializeField] private float offsetX = 0f;
    [SerializeField] private float offsetY = 0f;

    protected override Vector3 GetConstrainedPosition(Vector3 current, Vector3 target)
    {
        // 부모에서 지정한 maxX, maxY 활용
        float clampedX = Mathf.Clamp(target.x, -maxX + offsetX, maxX + offsetX);
        float clampedY = Mathf.Clamp(target.y, -maxY + offsetY, maxY + offsetY);

        return new Vector3(clampedX, clampedY, target.z);
    }
}
