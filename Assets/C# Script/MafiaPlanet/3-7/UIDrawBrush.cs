using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDrawBrush : MonoBehaviour
{
    public RectTransform drawArea; // RawImage의 RectTransform
    public GameObject brushPrefab;
    public Canvas canvas;
    public RectTransform targetZone; // 칠해야 할 구역
    public GameObject stage_3_7;
    

    public float spacing = 10f;
    private Minigame_3_7 minigame_3_7;
    private Vector2 lastPos;
    private bool isDrawing = false;
    private int totalBrushes = 0;
    private int brushesInsideTarget = 0;
    private HashSet<Vector2Int> coveredCells = new HashSet<Vector2Int>();
    private int totalCells;
    public float cellSize = 20f;
    private float fillPercent;

    private int cols;
    private int rows;
    private bool isEnd = false;

    void Start()
    {
        float width = targetZone.rect.width;
        float height = targetZone.rect.height;
        minigame_3_7 = stage_3_7.GetComponent<Minigame_3_7>();
        cols = Mathf.FloorToInt(width / cellSize);
        rows = Mathf.FloorToInt(height / cellSize);

        totalCells = cols * rows;
    }

    void Update()
    {
        Vector2 screenPos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0) && RectTransformUtility.RectangleContainsScreenPoint(drawArea, screenPos, canvas.worldCamera))
        {
            isDrawing = true;
            TryDraw(forceSingleDot: true);  // 첫 점 강제 1회 찍기
        }
        else if (Input.GetMouseButton(0) && isDrawing && RectTransformUtility.RectangleContainsScreenPoint(drawArea, screenPos, canvas.worldCamera))
        {
            TryDraw(forceSingleDot: false);  // 드래그 중은 보간 사용
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }

        if (totalCells > 0 && !isEnd)
        {
            fillPercent = (coveredCells.Count / (float)totalCells) * 100f;
            Debug.Log($"타겟 영역 칠한 비율: {fillPercent:F1}%");
        }

        if (fillPercent >= 80f)
        {
            minigame_3_7.Succeed();
            isEnd = true;
        }
    }

    void TryDraw(bool forceSingleDot)
    {
        Vector2 screenPos = Input.mousePosition;
        Vector2 localPos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(drawArea, screenPos, canvas.worldCamera, out localPos))
        {
            float distance = Vector2.Distance(localPos, lastPos);

            if (forceSingleDot || distance > spacing)
            {
                int segments = forceSingleDot ? 1 : Mathf.CeilToInt(distance / spacing);

                for (int i = 1; i <= segments; i++)
                {
                    Vector2 interp = forceSingleDot ? localPos : Vector2.Lerp(lastPos, localPos, i / (float)segments);
                    GameObject brush = Instantiate(brushPrefab, drawArea);
                    RectTransform brushRect = brush.GetComponent<RectTransform>();
                    brushRect.anchoredPosition = interp;

                    // 타겟 커버 체크
                    Vector2 brushScreenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, brushRect.position);
                    Vector2 localToTarget;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(targetZone, brushScreenPos, canvas.worldCamera, out localToTarget))
                    {
                        float width = targetZone.rect.width;
                        float height = targetZone.rect.height;
                        int cellX = Mathf.FloorToInt((localToTarget.x + width * targetZone.pivot.x) / cellSize);
                        int cellY = Mathf.FloorToInt((localToTarget.y + height * targetZone.pivot.y) / cellSize);

                        // 셀 범위 검사 추가
                        if (cellX >= 0 && cellY >= 0 && cellX < cols && cellY < rows)
                        {
                            Vector2Int cell = new Vector2Int(cellX, cellY);
                            coveredCells.Add(cell);
                        }
                    }
                }

                lastPos = localPos;
            }
        }
    }



    public void ClearAllBrushes()
    {
        GameObject[] brushes = GameObject.FindGameObjectsWithTag("Brush");

        foreach (GameObject brush in brushes)
        {
            Destroy(brush);
        }

        // 카운터도 초기화
        totalBrushes = 0;
        brushesInsideTarget = 0;
    }
}
