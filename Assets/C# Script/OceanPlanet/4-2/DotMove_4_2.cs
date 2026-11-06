using DG.Tweening;
using UnityEngine;

public class DotMove_4_2 : MonoBehaviour
{
    public float lingerTime = 1f;
    public float moveDistance = 2f;

    public float fadeInDuration = 0.25f;
    public float stepFadeDuration = 0.15f;
    [Range(0f, 1f)] public float stepDipAlpha = 0.2f;
    public float exitFadeDuration = 0.25f;

    public int Step { get; private set; }

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (sr == null) return;
        SetAlpha(0f);
        sr.DOFade(1f, fadeInDuration).SetLink(gameObject);
    }

    public void Move()
    {
        Step++;
        transform.position += Vector3.left * moveDistance;

        if (sr == null) return;

        sr.DOKill();
        SetAlpha(stepDipAlpha);
        sr.DOFade(1f, stepFadeDuration).SetLink(gameObject);
    }

    // delay초 기다렸다가 그 자리에서 페이드아웃 후 삭제
    public void FadeOutAndDestroy(float delay)
    {
        if (sr == null)
        {
            Destroy(gameObject, delay);
            return;
        }

        sr.DOKill();
        SetAlpha(1f);   // 도착 상태 그대로 선명하게 유지

        sr.DOFade(0f, exitFadeDuration)
          .SetDelay(delay)
          .SetLink(gameObject)
          .OnComplete(() => Destroy(gameObject));
    }
    private void SetAlpha(float a)
    {
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }
}