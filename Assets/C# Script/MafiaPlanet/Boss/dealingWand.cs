using System.Collections;
using UnityEngine;
using DG.Tweening;

public class dealingWand : MonoBehaviour
{
    [Header("Hit Settings")]
    [SerializeField] private Collider2D hitCollider;       // 자식 콜라이더
    [SerializeField] private GameObject hitEffectPrefab;   // 이펙트 프리팹
    [SerializeField] private float activeTime = 0.2f;      // 판정 유지 시간
    [SerializeField] private float lifeTime = 2f;          // 자동 파괴 시간(스폰 시점 기준)

    [Header("Intro Tween")]
    [SerializeField] private float spawnBackOffset = 5f;  // 방향 반대쪽 오프셋
    [SerializeField] private float spawnLeftOffset = 5f;  // 왼쪽(반시계 직교) 오프셋
    [SerializeField] private float moveDuration = 0.18f;   // 이동 시간
    [SerializeField] private float rotateDuration = 0.16f; // 회전 시간
    [SerializeField] private float curveBend = 6f;         // 곡선 굴곡 세기(중간 제어점 거리)

    private Vector2 lastDir = Vector2.right;               // 받은 방향(정규화 저장)
    private Sequence introSeq;
    private GameObject tempEffect;

    private void Start()
    {
        Debug.Log("뱁봉");

        if (hitCollider == null)
            hitCollider = GetComponentInChildren<Collider2D>();

        if (hitCollider != null)
            hitCollider.enabled = false;

        tempEffect = transform.GetChild(0).gameObject; tempEffect.SetActive(false);
    }

    public void Fire(Vector3 position, Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.0001f)
            lastDir = direction.normalized;
        Vector2 dir = lastDir;
        Vector2 left = new Vector2(-dir.y, dir.x);

        Vector3 spawnPos = position + (Vector3)(-dir * spawnBackOffset + -left * spawnLeftOffset);

        float finalAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float startAngle = finalAngle + 90f;
        Quaternion startRot = Quaternion.AngleAxis(startAngle, Vector3.forward);
        Quaternion finalRot = Quaternion.AngleAxis(finalAngle, Vector3.forward);

        transform.SetPositionAndRotation(spawnPos, startRot);

        Vector3 control = Vector3.Lerp(spawnPos, position, 0.5f)
                          + (Vector3)(left * curveBend)
                          + (Vector3)(-dir * (curveBend * 0.35f));

        introSeq?.Kill(false);
        introSeq = DOTween.Sequence();

        introSeq.Join(transform.DOPath(new Vector3[] { control, position },
                                       moveDuration,
                                       PathType.CatmullRom)
                              .SetEase(Ease.OutQuad));
        introSeq.Join(transform.DORotateQuaternion(finalRot, rotateDuration)
                              .SetEase(Ease.OutQuad));

        introSeq.AppendCallback(() => StartCoroutine(FireRoutine()));

        introSeq.AppendInterval(lifeTime);

        Vector3 retreatPos = position + (Vector3)(left.normalized * -7f);
        introSeq.Append(transform.DOMove(retreatPos, 0.5f)
                                  .SetEase(Ease.InSine));

        introSeq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }


    private IEnumerator FireRoutine()
    {
        yield return new WaitForSeconds(0.5f);

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