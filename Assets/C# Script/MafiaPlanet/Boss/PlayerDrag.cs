using System.Collections.Generic;
using UnityEngine;

public class PlayerDrag : DragAndDrop
{
    [SerializeField] private float offsetX = 0f;
    [SerializeField] private float offsetY = 0f;
    [SerializeField] private GameObject stage_3_15;

    [Tooltip("피격 후 무적 시간")]
    [SerializeField] private float hitCooldown = 1f;

    [Header("Demo Dodge")]
    [SerializeField] private float dodgeSpeed = 20f;
    [SerializeField] private float dodgeMargin = 0.6f;
    [SerializeField] private float searchStep = 0.4f;

    private readonly List<ThreatBox> threats = new();
    private bool hasDodgeTarget;
    private Vector2 dodgeTarget;

    private Minigame_3_15 minigame_3_15;
    private float lastHitTime = -999f;

    private bool isDemoMode;
    private Collider2D playerCollider;
    private dealingWand[] dealingWands;
    private float nextScanTime;


    private void Start()
    {
        if (stage_3_15 != null)
            minigame_3_15 = stage_3_15.GetComponent<Minigame_3_15>();

        playerCollider = GetComponent<Collider2D>();

        if (playerCollider == null)
            playerCollider = GetComponentInChildren<Collider2D>();
    }

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        isDragging = false;
        nextScanTime = 0f;
    }

    protected override void OnMouseDown()
    {
        if (isDemoMode) return;

        base.OnMouseDown();
    }

    protected override void OnMouseUp()
    {
        if (isDemoMode)
        {
            isDragging = false;
            return;
        }

        base.OnMouseUp();
    }

    protected override void Update()
    {
        if (isDemoMode)
        {
            isDragging = false;
            DemoDodge();
            return;
        }

        base.Update();
    }

    private void DemoDodge()
    {
        if (playerCollider == null) return;

        // 아직 유효한 모든 레이저의 위험 영역 수집 (발사 전 경고 구간 포함)
        threats.Clear();
        foreach (var w in dealingWand.Active)
        {
            if (w != null && Time.time <= w.ThreatEndTime)
                threats.AddRange(w.Threats);
        }

        if (threats.Count == 0) { hasDodgeTarget = false; return; }

        Bounds pb = playerCollider.bounds;
        pb.Expand(dodgeMargin * 2f);                      // 사방 dodgeMargin만큼 여유
        Vector3 centerOffset = pb.center - transform.position;
        Vector3 size = pb.size;

        // 지금 위치가 안전하면 가만히 있음
        if (CountOverlap(pb) == 0) { hasDodgeTarget = false; return; }

        // 목표가 없거나 목표가 더 이상 안전하지 않을 때만 재탐색
        if (!hasDodgeTarget || CountOverlapAt(dodgeTarget, centerOffset, size) > 0)
        {
            float minX = -maxX + offsetX, maxX_ = maxX + offsetX;
            float minY = -maxY + offsetY, maxY_ = maxY + offsetY;

            float bestScore = float.MaxValue;
            Vector2 best = transform.position;

            for (float x = minX; x <= maxX_; x += searchStep)
            {
                for (float y = minY; y <= maxY_; y += searchStep)
                {
                    var p = new Vector2(x, y);
                    int ov = CountOverlapAt(p, centerOffset, size);
                    float score = ov * 1000f + Vector2.Distance(transform.position, p);
                    if (score < bestScore) { bestScore = score; best = p; }
                }
            }

            dodgeTarget = best;
            hasDodgeTarget = true;
        }

        Vector3 target = new Vector3(dodgeTarget.x, dodgeTarget.y, transform.position.z);
        transform.position = Vector3.MoveTowards(
            transform.position,
            GetConstrainedPosition(transform.position, target),
            dodgeSpeed * Time.deltaTime);
    }

    private int CountOverlapAt(Vector2 pos, Vector3 centerOffset, Vector3 size)
    {
        var b = new Bounds(new Vector3(pos.x, pos.y, 0f) + centerOffset, size);
        return CountOverlap(b);
    }

    private int CountOverlap(Bounds b)
    {
        int c = 0;
        for (int i = 0; i < threats.Count; i++)
            if (threats[i].Overlaps(b)) c++;
        return c;
    }

    protected override Vector3 GetConstrainedPosition(
        Vector3 current, Vector3 target)
    {
        float clampedX = Mathf.Clamp(
            target.x, -maxX + offsetX, maxX + offsetX);

        float clampedY = Mathf.Clamp(
            target.y, -maxY + offsetY, maxY + offsetY);

        return new Vector3(clampedX, clampedY, target.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FailureTag"))
            TakeHit($"페이즈2 지팡이 빛 피격 ({collision.name})");
    }

    // EndingWand에서 호출
    public void EndingBlast()
    {
        TakeHit("페이즈2 EndingWand 레이저 피격");
    }

    public void TakeHit(string reason)
    {
        if (Time.time - lastHitTime < hitCooldown) return;

        lastHitTime = Time.time;

        if (minigame_3_15 != null)
            minigame_3_15.ReportMiss(reason);
    }
}