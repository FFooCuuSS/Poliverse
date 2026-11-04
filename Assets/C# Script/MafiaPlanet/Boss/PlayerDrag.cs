using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    [SerializeField] private float dodgeLookAhead = 0.6f;
    [SerializeField] private float planHorizon = 2f;
    [SerializeField] private float searchStep = 0.4f;
    [SerializeField] private float dodgeCenterBias = 1.0f;    // 중앙에서 먼 칸일수록 불리하게 (클수록 가운데 선호)
    [SerializeField] private float dodgeTightPenalty = 2.0f;  // 마진이 빠듯한 칸에 주는 페널티
    private bool forcedMove;
    private float dodgeCeilingY = float.PositiveInfinity;
    // 패턴 3 등에서 플레이어가 이 y보다 위로 못 가게 제한
    public void SetDodgeCeiling(float y) => dodgeCeilingY = y;

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

        // 데모가 아니면 제한/강제 이동 상태 초기화
        if (!isDemo)
        {
            dodgeCeilingY = float.PositiveInfinity;
            forcedMove = false;
            hasDodgeTarget = false;
        }
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
        if (forcedMove) return;
        if (playerCollider == null) return;

        float now = Time.time;

        // 앞으로 planHorizon초 안에 위험해지는 영역 수집
        threats.Clear();
        foreach (var w in dealingWand.Active)
        {
            if (w == null || now > w.ThreatEndTime) continue;
            foreach (var t in w.Threats)
                if (t.end >= now && t.start <= now + planHorizon)
                    threats.Add(t);
        }

        Bounds pb = playerCollider.bounds;
        Vector3 co = pb.center - transform.position;
        Vector3 baseSize = pb.size;
        Vector2 cur = transform.position;

        // 위험 영역이 없으면 천장보다 위에 있을 때만 아래로 복귀
        if (threats.Count == 0)
        {
            hasDodgeTarget = false;
            if (transform.position.y > dodgeCeilingY)
            {
                Vector3 down = new Vector3(transform.position.x, dodgeCeilingY, transform.position.z);
                transform.position = Vector3.MoveTowards(transform.position, down, dodgeSpeed * Time.deltaTime);
            }
            return;
        }

        Vector3 sizeBig = baseSize + Vector3.one * dodgeMargin * 2f;
        Vector3 sizeSmall = baseSize + Vector3.one * 0.05f * 2f;

        // 지금 자리가 앞으로 dodgeLookAhead초 동안 안전하면 제자리
        if (IsSafeAt(cur, co, sizeBig, now, now + dodgeLookAhead))
        {
            hasDodgeTarget = false;
            return;
        }

        // 기존 목표가 아직 유효하면 유지
        bool keep = hasDodgeTarget && IsPathSafe(cur, dodgeTarget, co, sizeSmall);

        if (!keep)
        {
            float minX = -maxX + offsetX, maxX_ = maxX + offsetX;
            float minY = -maxY + offsetY;
            float maxY_ = Mathf.Min(maxY + offsetY, dodgeCeilingY);

            bool found = false;
            Vector2 best = cur;

            // 1차: 경로까지 안전한 지점 / 2차(비상): 도착 지점만 안전한 지점
            for (int pass = 0; pass < 2 && !found; pass++)
            {
                bool needPath = pass == 0;
                float bestScore = float.MaxValue;

                for (float x = minX; x <= maxX_; x += searchStep)
                {
                    for (float y = minY; y <= maxY_; y += searchStep)
                    {
                        var p = new Vector2(x, y);
                        float dist = Vector2.Distance(cur, p);

                        // 가까움 + 화면 중앙 선호
                        float score = dist
                                    + dodgeCenterBias * Mathf.Abs(x - offsetX)
                                    + dodgeCenterBias * 0.5f * Mathf.Abs(y - offsetY);
                        if (score >= bestScore) continue;

                        float tArr = now + dist / dodgeSpeed;
                        bool okSmall = needPath
                            ? IsPathSafe(cur, p, co, sizeSmall)
                            : IsSafeAt(p, co, sizeSmall, tArr, tArr + dodgeLookAhead);
                        if (!okSmall) continue;

                        // 큰 마진이 안 되면 탈락시키지 않고 페널티만
                        if (needPath && !IsPathSafe(cur, p, co, sizeBig))
                            score += dodgeTightPenalty;

                        if (score < bestScore) { bestScore = score; best = p; found = true; }
                    }
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

    public void DemoMoveTo(Vector2 pos, float duration)
    {
        forcedMove = true;
        hasDodgeTarget = false;

        Vector3 target = GetConstrainedPosition(
            transform.position,
            new Vector3(pos.x, pos.y, transform.position.z));

        transform
            .DOMove(target, duration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private bool IsSafeAt(Vector2 pos, Vector3 co, Vector3 size, float t0, float t1)
    {
        var b = new Bounds(new Vector3(pos.x, pos.y, 0f) + co, size);
        for (int i = 0; i < threats.Count; i++)
        {
            var t = threats[i];
            if (t.end < t0 || t.start > t1) continue;
            if (t.Overlaps(b)) return false;
        }
        return true;
    }

    private bool IsPathSafe(Vector2 from, Vector2 to, Vector3 co, Vector3 size)
    {
        float now = Time.time;
        float dist = Vector2.Distance(from, to);
        int steps = Mathf.Max(1, Mathf.CeilToInt(dist / 0.3f));

        for (int i = 1; i <= steps; i++)
        {
            float k = (float)i / steps;
            Vector2 p = Vector2.Lerp(from, to, k);
            float tArr = now + dist * k / dodgeSpeed;
            if (!IsSafeAt(p, co, size, tArr - 0.1f, tArr + 0.1f)) return false;
        }

        float tEnd = now + dist / dodgeSpeed;
        return IsSafeAt(to, co, size, tEnd, tEnd + dodgeLookAhead);
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