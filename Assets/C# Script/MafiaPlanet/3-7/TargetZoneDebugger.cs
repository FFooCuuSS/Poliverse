//using UnityEngine;

//public class TargetZoneDebugger : MonoBehaviour
//{
//    public RectTransform targetZone;
//    public float cellSize = 20f;
//    public Color cellColor = new Color(0f, 1f, 0f, 0.3f); // 연한 초록

//    void OnDrawGizmos()
//    {
//        if (targetZone == null) return;

//        Vector3[] worldCorners = new Vector3[4];
//        targetZone.GetWorldCorners(worldCorners);

//        float width = targetZone.rect.width;
//        float height = targetZone.rect.height;
//        int cols = Mathf.FloorToInt(width / cellSize);
//        int rows = Mathf.FloorToInt(height / cellSize);

//        Vector3 origin = worldCorners[0]; // bottom-left
//        Vector3 right = (worldCorners[3] - worldCorners[0]) / cols; // x 방향
//        Vector3 up = (worldCorners[1] - worldCorners[0]) / rows; // y 방향

//        Gizmos.color = cellColor;

//        for (int y = 0; y < rows; y++)
//        {
//            for (int x = 0; x < cols; x++)
//            {
//                Vector3 cellStart = origin + right * x + up * y;
//                Gizmos.DrawWireCube(cellStart + (right + up) / 2f, right + up);
//            }
//        }
//    }
//}
