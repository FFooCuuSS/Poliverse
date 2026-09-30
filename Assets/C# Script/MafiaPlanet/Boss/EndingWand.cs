using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EndingWand : MonoBehaviour
{
    // true면 플레이어 피격 판정 OFF (거울 성공 시)
    [SerializeField] private bool notifyEnabled = false;

    [Header("Beam Shape / Visual")]
    [SerializeField] private float beamMaxLength = 20f;
    [SerializeField] private float beamHalfWidth = 0.075f;
    [Tooltip("빔 색/알파. 머티리얼 에셋은 건드리지 않음 (셰이더는 Sprites/Default 권장)")]
    [SerializeField] private Color beamColor = new(1f, 1f, 1f, 0.8f);
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 300;

    [Header("Lifetime")]
    [SerializeField] private float activeTime = 0.25f;
    [SerializeField] private float lifeTime = 2f;

    [Header("Intro Tween (same flow)")]
    [SerializeField] private float spawnBackOffset = 5f;
    [SerializeField] private float spawnLeftOffset = 5f;
    [SerializeField] private float moveDuration = 0.18f;
    [SerializeField] private float rotateDuration = 0.16f;
    [SerializeField] private float curveBend = 6f;

    [Header("Cut Filter / Behavior")]
    [SerializeField] private LayerMask clipLayers;
    [SerializeField] private string[] clipTags;
    [SerializeField] private bool destroyOnHit = false;

    [Header("Player Notify")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string playerHitMethod = "OnLaserHit";

    [Header("VFX (optional)")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Grow / Shrink")]
    [SerializeField] private float growSpeed = 40f;
    [SerializeField] private float shrinkSpeed = 60f;
    private float currentLength = 0f;

    // 레이저 ON/OFF 알림 — 보스 스프라이트 교체용
    public event Action<bool> OnLaserStateChanged;
    private bool laserOn;

    private Vector2 lastDir = Vector2.right;
    private Vector2 targetPos;
    private Quaternion targetRot;
    private Sequence introSeq;

    private MeshFilter mf;
    private MeshRenderer mr;
    private Mesh mesh;
    private GameObject hitFxInstance;

    private void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        mesh = mf.sharedMesh ?? (mf.sharedMesh = new Mesh { name = "EndingWandBeam" });
        mesh.MarkDynamic();

        if (mr.sharedMaterial == null)
            mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

        mr.sortingLayerName = sortingLayerName;
        mr.sortingOrder = sortingOrder;

        ApplyBeamColor();

        // 발사 전엔 길이 0 → 안 보임
        currentLength = 0f;
        BuildBeamMesh(0f);
    }

    private void OnDestroy()
    {
        SetLaser(false);
        if (hitFxInstance != null) Destroy(hitFxInstance);
    }

    public void Fire(Vector2 position, Vector2 direction, float lightRemaining, float wandRemaining,
                     Vector2 offset = default, float angleOffsetDeg = 0f)
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
        Quaternion finalRot = Quaternion.AngleAxis(finalAngle + angleOffsetDeg, Vector3.forward);

        targetRot = finalRot;
        transform.SetPositionAndRotation(spawnPos, startRot);

        Vector2 control = Vector2.Lerp(spawnPos, position, 0.5f)
                         + (left * curveBend)
                         + (-dir * (curveBend * 0.35f));

        introSeq?.Kill(false);
        introSeq = DOTween.Sequence().SetLink(gameObject);
        introSeq.Join(transform.DOPath(new Vector3[] { control, position }, moveDuration, PathType.CatmullRom)
                              .SetEase(Ease.OutQuad));
        introSeq.Join(transform.DORotateQuaternion(finalRot, rotateDuration).SetEase(Ease.OutQuad));
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

        SetLaser(true);

        float elapsed = 0f;
        while (elapsed < activeTime)
        {
            Vector2 origin = transform.position;
            Vector2 fwd = Vector2.down;

            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, fwd, beamMaxLength, ~0);
            float cutLength = beamMaxLength;

            RaycastHit2D? firstValidCut = null;
            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if ((clipLayers.value & (1 << h.collider.gameObject.layer)) == 0) continue;
                if (!TagPass(h.collider.gameObject)) continue;
                firstValidCut = h;
                break;
            }

            if (firstValidCut.HasValue)
            {
                cutLength = firstValidCut.Value.distance;
                Vector3 endWorld = firstValidCut.Value.point;

                if (hitEffectPrefab != null)
                {
                    if (hitFxInstance == null)
                        hitFxInstance = Instantiate(hitEffectPrefab, endWorld, transform.rotation, transform.parent);
                    else
                        hitFxInstance.transform.SetPositionAndRotation(endWorld, transform.rotation);
                }

                if (destroyOnHit)
                {
                    currentLength = cutLength;
                    BuildBeamMesh(currentLength);
                    NotifyPlayersWithin(hits, cutLength);
                    Destroy(gameObject); // OnDestroy에서 SetLaser(false) + FX 정리
                    yield break;
                }
            }
            else if (hitFxInstance != null)
            {
                Destroy(hitFxInstance);
                hitFxInstance = null;
            }

            NotifyPlayersWithin(hits, cutLength);

            float spd = (cutLength > currentLength) ? growSpeed : shrinkSpeed;
            currentLength = Mathf.MoveTowards(currentLength, cutLength, spd * Time.deltaTime);
            BuildBeamMesh(currentLength);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 수축 종료
        while (currentLength > 0.001f)
        {
            currentLength = Mathf.MoveTowards(currentLength, 0f, shrinkSpeed * Time.deltaTime);
            BuildBeamMesh(currentLength);
            yield return null;
        }
        BuildBeamMesh(0f);
        SetLaser(false);

        if (hitFxInstance != null) { Destroy(hitFxInstance); hitFxInstance = null; }
    }

    public void EnableNotify()
    {
        notifyEnabled = true;
    }

    private void ApplyBeamColor()
    {
        // MaterialPropertyBlock → 이 렌더러에만 적용, 에셋 저장 상태와 무관
        var mpb = new MaterialPropertyBlock();
        mr.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", beamColor);
        mr.SetPropertyBlock(mpb);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (mr != null) ApplyBeamColor();
    }
#endif

    private void SetLaser(bool on)
    {
        if (laserOn == on) return;
        laserOn = on;
        OnLaserStateChanged?.Invoke(on);
    }

    private void BuildBeamMesh(float length)
    {
        Vector3[] v =
        {
            new(-beamHalfWidth, 0f, 0f),
            new( beamHalfWidth, 0f, 0f),
            new(-beamHalfWidth, -length, 0f),
            new( beamHalfWidth, -length, 0f),
        };
        int[] t = { 0, 1, 2, 1, 3, 2 };

        mesh.Clear();
        mesh.vertices = v;
        mesh.triangles = t;
        mesh.RecalculateBounds();
    }

    private bool TagPass(GameObject go)
    {
        if (clipTags == null || clipTags.Length == 0) return true;
        for (int i = 0; i < clipTags.Length; i++)
            if (!string.IsNullOrEmpty(clipTags[i]) && go.CompareTag(clipTags[i])) return true;
        return false;
    }

    private void NotifyPlayersWithin(RaycastHit2D[] hits, float maxDistance)
    {
        if (notifyEnabled) return;

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.distance > maxDistance) break;

            var go = h.collider.gameObject;
            if (!go.CompareTag(playerTag)) continue;

            if (go.TryGetComponent(out IEndingWandHittable ih))
            {
                ih.OnLaserHit(h.point, this);
                continue;
            }

            go.SendMessage(playerHitMethod, h.point, SendMessageOptions.DontRequireReceiver);

            // Miss 보고 (PlayerDrag 쪽 쿨다운으로 매 프레임 중복 방지)
            var pd = go.GetComponent<PlayerDrag>();
            if (pd != null) pd.EndingBlast();
        }
    }
}

public interface IEndingWandHittable
{
    void OnLaserHit(Vector2 hitPoint, EndingWand source);
}