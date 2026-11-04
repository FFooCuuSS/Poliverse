using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public struct ThreatBox
{
    public Vector2 center;
    public Vector2 hu;
    public Vector2 hv;
    public float start;   // 이 박스가 위험한 시작 시각
    public float end;     // 위험한 끝 시각

    public bool Overlaps(Bounds b)
    {
        Vector2 d = (Vector2)b.center - center;
        Vector2 e = b.extents;

        if (Mathf.Abs(d.x) > Mathf.Abs(hu.x) + Mathf.Abs(hv.x) + e.x) return false;
        if (Mathf.Abs(d.y) > Mathf.Abs(hu.y) + Mathf.Abs(hv.y) + e.y) return false;
        if (hu.sqrMagnitude > 1e-6f && Separated(d, hu.normalized, e)) return false;
        if (hv.sqrMagnitude > 1e-6f && Separated(d, hv.normalized, e)) return false;
        return true;
    }

    private bool Separated(Vector2 d, Vector2 a, Vector2 e)
    {
        float rBox = Mathf.Abs(Vector2.Dot(hu, a)) + Mathf.Abs(Vector2.Dot(hv, a));
        float rAabb = e.x * Mathf.Abs(a.x) + e.y * Mathf.Abs(a.y);
        return Mathf.Abs(Vector2.Dot(d, a)) > rBox + rAabb;
    }
}

public class dealingWand : MonoBehaviour
{
    [Header("Hit Settings")]
    [SerializeField] private Collider2D hitCollider;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float activeTime = 0.2f;
    [SerializeField] private float lifeTime = 2f;

    [Header("Intro Tween")]
    [SerializeField] private float spawnBackOffset = 5f;
    [SerializeField] private float spawnLeftOffset = 5f;
    [SerializeField] private float moveDuration = 0.18f;
    [SerializeField] private float rotateDuration = 0.16f;
    [SerializeField] private float curveBend = 6f;

    // 레이저(히트 판정) ON/OFF 알림 — 보스 스프라이트 교체용
    public event Action<bool> OnLaserStateChanged;
    private bool laserOn;

    private Vector2 lastDir = Vector2.right;
    private Vector2 targetPos;
    private Quaternion targetRot;
    private Sequence introSeq;
    private GameObject tempEffect;

    public static readonly List<dealingWand> Active = new();
    public readonly List<ThreatBox> Threats = new();
    public float ThreatEndTime { get; private set; }

    private void BuildThreats(Vector2 position, Quaternion finalRot)
    {
        Threats.Clear();

        Collider2D col = hitCollider != null ? hitCollider : GetComponentInChildren<Collider2D>(true);
        var box = col as BoxCollider2D;
        if (box == null)
        {
            Debug.LogWarning($"[dealingWand] {name}: BoxCollider2D가 아니라 위험 영역 계산 불가");
            return;
        }

        // 현재 포즈와 무관한 형태로 저장 (회전만 제거)
        Quaternion inv = Quaternion.Inverse(transform.rotation);
        Vector3 worldCenter = box.transform.TransformPoint(box.offset);
        Vector2 localCenter = inv * (worldCenter - transform.position);
        Vector2 localHu = inv * box.transform.TransformVector(new Vector3(box.size.x * 0.5f, 0f, 0f));
        Vector2 localHv = inv * box.transform.TransformVector(new Vector3(0f, box.size.y * 0.5f, 0f));

        // 타임라인: Fire → (시퀀스) → FireRoutine → +moveDuration → 콜라이더 ON → activeTime 유지
        float seqLen = Mathf.Max(moveDuration, rotateDuration);
        float fireTime = Time.time;
        float enableAt = seqLen + moveDuration;
        ThreatEndTime = fireTime + enableAt + activeTime + 0.1f;

        bool moves = (targetPos - position).sqrMagnitude > 0.0001f
                  || Quaternion.Angle(finalRot, targetRot) > 0.1f;
        int n = moves ? Mathf.Clamp(Mathf.CeilToInt(activeTime / 0.1f), 4, 40) : 1;
        float dt = moves ? activeTime / n : activeTime;

        for (int i = 0; i <= n; i++)
        {
            float tau = moveDuration + activeTime * i / n;
            float x = Mathf.Clamp01(tau / Mathf.Max(lifeTime, 0.01f));
            float e = Mathf.Sin(x * Mathf.PI * 0.5f);

            Vector2 pos = Vector2.Lerp(position, targetPos, e);
            Quaternion rot = Quaternion.Slerp(finalRot, targetRot, e);

            float t = fireTime + seqLen + tau;

            Threats.Add(new ThreatBox
            {
                center = pos + (Vector2)(rot * localCenter),
                hu = (Vector2)(rot * localHu),
                hv = (Vector2)(rot * localHv),
                start = t - dt,
                end = t + dt,
            });
        }
    }


    private void Start()
    {
        if (hitCollider == null)
            hitCollider = GetComponentInChildren<Collider2D>();

        if (hitCollider != null)
            hitCollider.enabled = false;

        tempEffect = transform.GetChild(0).gameObject;
        tempEffect.SetActive(false);
    }

    private void OnDestroy()
    {
        Active.Remove(this);
        // 레이저 켜진 채로 파괴돼도 카운트가 꼬이지 않게
        SetLaser(false);
    }

    public void Fire(Vector2 position, Vector2 direction, float lightRemaining, float wandRemaining, Vector2 offset = default, float angleOffsetDeg = 0f)
    {
        targetPos = position + offset;
        lifeTime = wandRemaining;
        activeTime = lightRemaining;

        if (direction.sqrMagnitude > 0.0001f)
            lastDir = direction.normalized;

        Vector2 dir = lastDir;
        Vector2 left = new(-dir.y, dir.x);

        Vector2 spawnPos = position + (-dir * spawnBackOffset + -left * spawnLeftOffset);

        float finalAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion startRot = Quaternion.AngleAxis(finalAngle + 90f, Vector3.forward);
        Quaternion finalRot = Quaternion.AngleAxis(finalAngle, Vector3.forward);

        targetRot = Quaternion.AngleAxis(finalAngle + angleOffsetDeg, Vector3.forward);

        transform.SetPositionAndRotation(spawnPos, startRot);

        BuildThreats(position, finalRot);
        if (!Active.Contains(this)) Active.Add(this);

        Vector2 control = Vector2.Lerp(spawnPos, position, 0.5f)
                         + (left * curveBend)
                         + (-dir * (curveBend * 0.35f));

        introSeq?.Kill(false);
        introSeq = DOTween.Sequence().SetLink(gameObject);

        introSeq.Join(transform.DOPath(new Vector3[] { control, position }, moveDuration, PathType.CatmullRom)
            .SetEase(Ease.OutQuad));
        introSeq.Join(transform.DORotateQuaternion(finalRot, rotateDuration)
            .SetEase(Ease.OutQuad));

        introSeq.AppendCallback(() => StartCoroutine(FireRoutine()));
        introSeq.AppendInterval(lifeTime);

        Vector2 retreatPos = position + (left.normalized * -7f);
        introSeq.Append(transform.DOMove(retreatPos, 0.5f).SetEase(Ease.InSine));

        introSeq.OnComplete(() => Destroy(gameObject));
    }

    private IEnumerator FireRoutine()
    {
        transform.DOMove(targetPos, lifeTime).SetEase(Ease.OutSine).SetLink(gameObject);
        transform.DORotateQuaternion(targetRot, lifeTime).SetEase(Ease.OutSine).SetLink(gameObject);

        yield return new WaitForSeconds(moveDuration);

        if (hitCollider != null)
        {
            hitCollider.enabled = true;
            tempEffect.SetActive(true);
        }
        SetLaser(true);

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, transform.rotation);

        yield return new WaitForSeconds(activeTime);

        if (hitCollider != null)
        {
            hitCollider.enabled = false;
            tempEffect.SetActive(false);
        }
        SetLaser(false);
    }

    private void SetLaser(bool on)
    {
        if (laserOn == on) return;
        laserOn = on;
        OnLaserStateChanged?.Invoke(on);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Vector3 pos = transform.position;
        Vector2 dir = lastDir.sqrMagnitude < 0.0001f ? Vector2.right : lastDir.normalized;
        Vector2 left = new Vector2(-dir.y, dir.x);

        Vector3 gizmoPos = pos + (Vector3)(dir * spawnBackOffset + left * spawnLeftOffset);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(gizmoPos, 0.2f);
    }
#endif
}