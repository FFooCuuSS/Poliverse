using System.Collections.Generic;
using UnityEngine;

public class SignatureAutoBrush : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SignatureHoldInput_3_7 holdInput;
    [SerializeField] private RectTransform brushContainer;
    [SerializeField] private GameObject brushPrefab;
    [SerializeField] private RectTransform pathRoot;
    [SerializeField] private RectTransform penVisual;

    [Header("Move Settings")]
    [SerializeField] private float drawDuration = 2f;
    [SerializeField] private float moveSpeed = 400f;
    [SerializeField] private float brushSpacing = 3f;

    [Header("Curve Settings")]
    [SerializeField] private bool useCurveOnFirstStroke = true;
    [SerializeField] private int curveSamplesPerSegment = 16;

    [Header("Reconnect Settings")]
    [SerializeField] private float maxConnectDistance = 40f;

    [Header("Safety")]
    [SerializeField] private int maxBrushCount = 200;

    [Header("Debug")]
    [SerializeField] private bool spawnFirstBrushOnReset = false;

    [Header("Pooling")]
    [SerializeField] private bool usePooling = true;
    [SerializeField] private bool prewarmOnReset = true;
    [SerializeField] private int extraPrewarmCount = 16;

    private readonly List<List<Vector2>> strokePaths = new List<List<Vector2>>();
    private readonly List<GameObject> brushPool = new List<GameObject>();

    private int usedBrushCount;

    private bool started;
    private bool stopped = true;

    private int currentStrokeIndex;
    private int currentPointIndex;

    private Vector2 currentPosition;
    private Vector2 lastBrushPos;

    private bool hasLastBrushPos;

    private float totalPathLength;

    private void Awake()
    {
        BuildStrokePaths();
    }

    private void Update()
    {
        if (stopped) return;
        if (!started) return;
        if (strokePaths.Count == 0) return;

        AdvancePen(Time.deltaTime);
    }

    public void StartDrawingPath()
    {
        // 이미 그리는 중이면 Input 신호가 여러 번 와도 재시작하지 않음
        if (started && !stopped)
            return;

        if (strokePaths.Count == 0)
            BuildStrokePaths();

        if (strokePaths.Count == 0)
        {
            Debug.LogWarning("[3-7] No stroke path found.");
            return;
        }

        totalPathLength = CalculateTotalPathLength();

        if (totalPathLength <= 0f)
        {
            Debug.LogWarning("[3-7] Total path length is zero.");
            return;
        }

        if (drawDuration > 0f)
            moveSpeed = totalPathLength / drawDuration;

        started = true;
        stopped = false;

        Debug.Log($"[3-7] Start Drawing. Length={totalPathLength:F1}, Speed={moveSpeed:F1}, Duration={drawDuration:F1}");
    }

    public void StopDrawing()
    {
        stopped = true;
    }

    public void ResetBrush()
    {
        BuildStrokePaths();

        started = false;
        stopped = true;

        currentStrokeIndex = 0;
        currentPointIndex = 0;

        hasLastBrushPos = false;

        HideAllBrushes();

        totalPathLength = CalculateTotalPathLength();

        if (usePooling && prewarmOnReset)
            PrewarmBrushPool();

        if (strokePaths.Count > 0 && strokePaths[0].Count > 0)
        {
            currentPosition = strokePaths[0][0];
            UpdatePenVisual(currentPosition);

            // 기본값 false.
            // 이게 true면 시작/Show 시점에 점이 찍혀서 부하와 오해가 생김.
            if (spawnFirstBrushOnReset)
                SpawnBrushForce(currentPosition);
        }
        else
        {
            Debug.LogWarning("[3-7] ResetBrush failed. Stroke path is empty.");
        }
    }

    private void AdvancePen(float deltaTime)
    {
        float remain = moveSpeed * deltaTime;

        while (remain > 0f)
        {
            if (currentStrokeIndex >= strokePaths.Count)
            {
                stopped = true;
                return;
            }

            List<Vector2> currentStroke = strokePaths[currentStrokeIndex];

            if (currentStroke == null || currentStroke.Count == 0)
            {
                MoveToNextStroke();
                continue;
            }

            if (currentPointIndex >= currentStroke.Count - 1)
            {
                MoveToNextStroke();
                continue;
            }

            Vector2 target = currentStroke[currentPointIndex + 1];
            float dist = Vector2.Distance(currentPosition, target);

            if (dist <= 0.001f)
            {
                currentPosition = target;
                currentPointIndex++;
                UpdatePenVisual(currentPosition);
                continue;
            }

            float step = Mathf.Min(remain, dist);

            Vector2 oldPos = currentPosition;
            Vector2 nextPos = Vector2.MoveTowards(currentPosition, target, step);

            currentPosition = nextPos;
            remain -= step;

            UpdatePenVisual(currentPosition);

            // 핵심:
            // 펜 이동은 자동.
            // 흔적 생성은 오직 hold 중일 때만.
            if (holdInput != null && holdInput.IsHolding)
                SpawnBrushesAlongSegment(oldPos, currentPosition);

            if (Vector2.Distance(currentPosition, target) <= 0.001f)
                currentPointIndex++;
        }
    }

    private void MoveToNextStroke()
    {
        currentStrokeIndex++;

        if (currentStrokeIndex >= strokePaths.Count)
        {
            stopped = true;
            return;
        }

        currentPointIndex = 0;

        // 획이 바뀌면 이전 획과 이어지면 안 됨
        hasLastBrushPos = false;

        List<Vector2> nextStroke = strokePaths[currentStrokeIndex];

        if (nextStroke != null && nextStroke.Count > 0)
        {
            currentPosition = nextStroke[0];
            UpdatePenVisual(currentPosition);
        }
    }

    private void SpawnBrushesAlongSegment(Vector2 from, Vector2 to)
    {
        if (brushPrefab == null || brushContainer == null) return;
        if (maxBrushCount > 0 && usedBrushCount >= maxBrushCount) return;

        if (!hasLastBrushPos)
        {
            SpawnBrushForce(from);
        }
        else
        {
            float gap = Vector2.Distance(lastBrushPos, to);

            // 너무 멀면 이전 점과 현재 점을 억지로 잇지 않음
            if (gap > maxConnectDistance)
            {
                hasLastBrushPos = false;
                SpawnBrushForce(to);
                return;
            }
        }

        float distanceToTarget = Vector2.Distance(lastBrushPos, to);

        while (distanceToTarget >= brushSpacing)
        {
            if (maxBrushCount > 0 && usedBrushCount >= maxBrushCount)
                return;

            Vector2 dir = (to - lastBrushPos).normalized;
            Vector2 spawnPos = lastBrushPos + dir * brushSpacing;

            SpawnBrushForce(spawnPos);

            distanceToTarget = Vector2.Distance(lastBrushPos, to);
        }
    }

    private void SpawnBrushForce(Vector2 anchoredPos)
    {
        if (brushPrefab == null || brushContainer == null)
        {
            Debug.LogWarning("[3-7] BrushPrefab or BrushContainer is missing.");
            return;
        }

        if (maxBrushCount > 0 && usedBrushCount >= maxBrushCount)
            return;

        GameObject brush = GetBrushObject();
        if (brush == null) return;

        RectTransform brushRect = brush.GetComponent<RectTransform>();
        if (brushRect != null)
            brushRect.anchoredPosition = anchoredPos;

        brush.SetActive(true);

        lastBrushPos = anchoredPos;
        hasLastBrushPos = true;
    }

    private GameObject GetBrushObject()
    {
        if (!usePooling)
        {
            usedBrushCount++;
            return Instantiate(brushPrefab, brushContainer);
        }

        if (usedBrushCount < brushPool.Count)
        {
            GameObject pooled = brushPool[usedBrushCount];
            usedBrushCount++;
            return pooled;
        }

        GameObject created = Instantiate(brushPrefab, brushContainer);
        created.SetActive(false);
        brushPool.Add(created);

        usedBrushCount++;
        return created;
    }

    private void HideAllBrushes()
    {
        usedBrushCount = 0;

        if (usePooling)
        {
            for (int i = 0; i < brushPool.Count; i++)
            {
                if (brushPool[i] != null)
                    brushPool[i].SetActive(false);
            }
        }
        else
        {
            if (brushContainer == null) return;

            for (int i = brushContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = brushContainer.GetChild(i);
                if (child != null)
                    Destroy(child.gameObject);
            }
        }
    }

    private void PrewarmBrushPool()
    {
        if (brushPrefab == null || brushContainer == null) return;

        float spacing = Mathf.Max(0.5f, brushSpacing);
        int estimatedCount = Mathf.CeilToInt(totalPathLength / spacing) + extraPrewarmCount;

        if (maxBrushCount > 0)
            estimatedCount = Mathf.Min(estimatedCount, maxBrushCount);

        while (brushPool.Count < estimatedCount)
        {
            GameObject created = Instantiate(brushPrefab, brushContainer);
            created.SetActive(false);
            brushPool.Add(created);
        }

        Debug.Log($"[3-7] Brush pool ready: {brushPool.Count}");
    }

    private void UpdatePenVisual(Vector2 anchoredPos)
    {
        if (penVisual != null)
            penVisual.anchoredPosition = anchoredPos;
    }

    private float CalculateTotalPathLength()
    {
        float length = 0f;

        for (int s = 0; s < strokePaths.Count; s++)
        {
            List<Vector2> stroke = strokePaths[s];
            if (stroke == null || stroke.Count < 2) continue;

            for (int i = 0; i < stroke.Count - 1; i++)
                length += Vector2.Distance(stroke[i], stroke[i + 1]);
        }

        return length;
    }

    private void BuildStrokePaths()
    {
        strokePaths.Clear();

        if (pathRoot == null)
        {
            Debug.LogWarning("[3-7] PathRoot is missing.");
            return;
        }

        if (brushContainer == null)
        {
            Debug.LogWarning("[3-7] BrushContainer is missing.");
            return;
        }

        for (int i = 0; i < pathRoot.childCount; i++)
        {
            RectTransform stroke = pathRoot.GetChild(i) as RectTransform;
            if (stroke == null) continue;

            List<Vector2> rawPoints = new List<Vector2>();

            for (int j = 0; j < stroke.childCount; j++)
            {
                RectTransform point = stroke.GetChild(j) as RectTransform;
                if (point == null) continue;

                Vector2 localPoint = brushContainer.InverseTransformPoint(point.position);
                rawPoints.Add(localPoint);
            }

            if (rawPoints.Count < 2)
                continue;

            bool useCurve = i == 0 && useCurveOnFirstStroke && rawPoints.Count >= 4;

            if (useCurve)
                strokePaths.Add(BuildCatmullRomStroke(rawPoints));
            else
                strokePaths.Add(new List<Vector2>(rawPoints));
        }

        Debug.Log($"[3-7] Built Stroke Paths: {strokePaths.Count}, Length={CalculateTotalPathLength():F1}");
    }

    private List<Vector2> BuildCatmullRomStroke(List<Vector2> controlPoints)
    {
        List<Vector2> result = new List<Vector2>();

        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector2 p0 = i == 0 ? controlPoints[i] : controlPoints[i - 1];
            Vector2 p1 = controlPoints[i];
            Vector2 p2 = controlPoints[i + 1];
            Vector2 p3 = i + 2 < controlPoints.Count ? controlPoints[i + 2] : controlPoints[i + 1];

            int samples = Mathf.Max(2, curveSamplesPerSegment);

            for (int s = 0; s < samples; s++)
            {
                float t = s / (float)samples;
                Vector2 point = CatmullRom(p0, p1, p2, p3, t);

                if (result.Count == 0 || Vector2.Distance(result[result.Count - 1], point) > 0.01f)
                    result.Add(point);
            }
        }

        result.Add(controlPoints[controlPoints.Count - 1]);
        return result;
    }

    private Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }
}