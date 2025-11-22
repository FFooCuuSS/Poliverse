using System.Collections;
using UnityEngine;
using DG.Tweening;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EndingWand : MonoBehaviour
{
    [SerializeField] private bool notifyEnabled = false;

    [Header("Beam Shape / Visual")]
    [SerializeField] private float beamMaxLength = 20f;
    [SerializeField] private float beamHalfWidth = 0.075f;
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

    [Header("Appear / Smoothing")]
    [SerializeField] private float growSpeed = 40f;       
    [SerializeField] private float shrinkSpeed = 60f;     
    [SerializeField] private float appearFadeTime = 0.08f;
    private float currentLength = 0f;
    private Color _baseColor;

    // internal
    private Vector2 lastDir = Vector2.right;
    private Vector2 targetPos;
    private Quaternion targetRot;
    private Sequence introSeq;

    private MeshFilter mf;
    private MeshRenderer mr;
    private Mesh mesh;
    private GameObject hitFxInstance;

    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        mesh = mf.sharedMesh ?? (mf.sharedMesh = new Mesh { name = "EndingWandBeam" });
        mesh.MarkDynamic();

        if (mr.sharedMaterial == null)
            mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

        if (mr.sharedMaterial.HasProperty("_Color")) _baseColor = mr.sharedMaterial.color;
        else if (mr.sharedMaterial.HasProperty("_BaseColor")) _baseColor = mr.sharedMaterial.GetColor("_BaseColor");
        else _baseColor = beamColor;

        mr.sortingLayerName = sortingLayerName;
        mr.sortingOrder = sortingOrder;
        mr.sharedMaterial.renderQueue = 3000;

        currentLength = 0f;
        BuildBeamMesh(5f);
        SetAlpha(1f); // 시작은 투명
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
        float startAngle = finalAngle + 90f;
        Quaternion startRot = Quaternion.AngleAxis(startAngle, Vector3.forward);
        Quaternion finalRot = Quaternion.AngleAxis(finalAngle + angleOffsetDeg, Vector3.forward);

        targetRot = finalRot;
        transform.SetPositionAndRotation(spawnPos, startRot);

        Vector2 control = Vector2.Lerp(spawnPos, position, 0.5f)
                         + (left * curveBend)
                         + (-dir * (curveBend * 0.35f));

        introSeq?.Kill(false);
        introSeq = DOTween.Sequence();
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
        transform.DOMove(targetPos, lifeTime).SetEase(Ease.OutSine);
        transform.DORotateQuaternion(targetRot, lifeTime).SetEase(Ease.OutSine);

        yield return new WaitForSeconds(moveDuration);

        // 알파 페이드인
        DOTween.Kill(mr.sharedMaterial);
        DOTween.To(() => 0f, a => SetAlpha(a), 1f, appearFadeTime);

        float elapsed = 0f;

        while (elapsed < activeTime)
        {
            Vector2 origin = transform.position;
            Vector2 fwd = Vector2.down;

            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, fwd, beamMaxLength, ~0);
            float cutLength = beamMaxLength;
            Vector3 endWorld = origin + fwd * cutLength;

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
                endWorld = firstValidCut.Value.point;

                if (hitEffectPrefab != null)
                {
                    if (hitFxInstance == null)
                        hitFxInstance = Instantiate(hitEffectPrefab, endWorld, transform.rotation);
                    else
                        hitFxInstance.transform.SetPositionAndRotation(endWorld, transform.rotation);
                }

                if (destroyOnHit)
                {
                    currentLength = cutLength;
                    BuildBeamMesh(currentLength);
                    NotifyPlayersWithin(hits, cutLength);
                    if (hitFxInstance != null) Destroy(hitFxInstance);
                    Destroy(gameObject);
                    yield break;
                }
            }
            else
            {
                if (hitFxInstance != null) { Destroy(hitFxInstance); hitFxInstance = null; }
            }

            NotifyPlayersWithin(hits, cutLength);

            float spd = (cutLength > currentLength) ? growSpeed : shrinkSpeed;
            currentLength = Mathf.MoveTowards(currentLength, cutLength, spd * Time.deltaTime);

            BuildBeamMesh(currentLength);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 자연스런 수축 종료 연출
        while (currentLength > 0.001f)
        {
            currentLength = Mathf.MoveTowards(currentLength, 0f, shrinkSpeed * Time.deltaTime);
            BuildBeamMesh(currentLength);
            yield return null;
        }
        SetAlpha(0f);

        if (hitFxInstance != null) Destroy(hitFxInstance);
    }

    public void EnableNotify()
    {
        notifyEnabled = true;
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

    private void SetAlpha(float a)
    {
        a = Mathf.Clamp01(a);
        if (mr.sharedMaterial.HasProperty("_Color"))
            mr.sharedMaterial.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, a * _baseColor.a);
        else if (mr.sharedMaterial.HasProperty("_BaseColor"))
            mr.sharedMaterial.SetColor("_BaseColor", new Color(_baseColor.r, _baseColor.g, _baseColor.b, a * _baseColor.a));
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

            // 인터페이스 우선
            if (go.TryGetComponent(out IEndingWandHittable ih))
            {
                ih.OnLaserHit(h.point, this);
                continue;
            }

            go.SendMessage(playerHitMethod, h.point, SendMessageOptions.DontRequireReceiver);

            var pd = go.GetComponent<PlayerDrag>();
            if (pd != null) pd.EndingBlast();
        }
    }
}

public interface IEndingWandHittable
{
    void OnLaserHit(Vector2 hitPoint, EndingWand source);
}

