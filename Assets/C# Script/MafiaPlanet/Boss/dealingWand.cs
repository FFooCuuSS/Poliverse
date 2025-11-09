using System.Collections;
using UnityEngine;
using DG.Tweening;

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

    private Vector2 lastDir = Vector2.right;
    private Vector2 targetPos;
    private Quaternion targetRot;
    private Sequence introSeq;
    private GameObject tempEffect;

    private void Start()
    {
        if (hitCollider == null)
            hitCollider = GetComponentInChildren<Collider2D>();

        if (hitCollider != null)
            hitCollider.enabled = false;

        tempEffect = transform.GetChild(0).gameObject; tempEffect.SetActive(false);
    }

    public void Fire(Vector2 position, Vector2 direction, float lightRemaining, float wandRemaining, Vector2 offset = default, float angleOffsetDeg = 0f)
    {
        targetPos = position + offset;
        targetRot = transform.rotation;
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
        Quaternion finalRot = Quaternion.AngleAxis(finalAngle, Vector3.forward);

        // 타겟방향은 여기있음
        targetRot = Quaternion.AngleAxis(finalAngle + angleOffsetDeg, Vector3.forward);

        transform.SetPositionAndRotation(spawnPos, startRot);

        Vector2 control = Vector2.Lerp(spawnPos, position, 0.5f)
                         + (left * curveBend)
                         + (-dir * (curveBend * 0.35f));

        introSeq?.Kill(false);
        introSeq = DOTween.Sequence();

        introSeq.Join(transform.DOPath(
            new Vector3[] { control, position },
            moveDuration,
            PathType.CatmullRom)
            .SetEase(Ease.OutQuad));

        introSeq.Join(transform.DORotateQuaternion(finalRot, rotateDuration)
            .SetEase(Ease.OutQuad));

        introSeq.AppendCallback(() => StartCoroutine(FireRoutine()));
        introSeq.AppendInterval(lifeTime);

        Vector2 retreatPos = position + (left.normalized * -7f);

        introSeq.Append(transform.DOMove(retreatPos, 0.5f)
            .SetEase(Ease.InSine));

        introSeq.OnComplete(() => Destroy(gameObject));
    }



    private IEnumerator FireRoutine()
    {
        transform.DOMove(targetPos, lifeTime)
            .SetEase(Ease.OutSine);
        transform.DORotateQuaternion(targetRot, lifeTime)
            .SetEase(Ease.OutSine);

        yield return new WaitForSeconds(moveDuration);

        if (hitCollider != null)
        {
            hitCollider.enabled = true;
            tempEffect.SetActive(true);
        }

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, transform.rotation);

        yield return new WaitForSeconds(activeTime);

        if (hitCollider != null)
        {
            hitCollider.enabled = false;
            tempEffect.SetActive(false);
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Vector3 pos = transform.position;
        Vector2 dir = lastDir.sqrMagnitude < 0.0001f ? Vector2.right : lastDir.normalized;
        Vector2 left = new Vector2(-dir.y, dir.x);

        Vector3 targetPos = pos + (Vector3)(dir * spawnBackOffset + left * spawnLeftOffset); // 단순 참고
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetPos, 0.2f);
    }
#endif
}