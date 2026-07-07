using System;
using DG.Tweening;
using UnityEngine;

public class Stone : MonoBehaviour
{
    [SerializeField] private float touchEpsilon = 0.05f;

    [Header("클릭 시 튕기는 연출")]
    [SerializeField] private float bounceMotionHeight = 0.45f;
    [SerializeField] private float bounceMotionDuration = 0.25f; // Inspector에서 확인용 (현재 하드코딩 사용 중)
    [SerializeField] private float upDuration = 0.10f;
    [SerializeField] private float downDuration = 0.12f;

    private Jelly target;
    private bool wasAboveGround = true;

    private float baseY;
    private float currentY; // DOTween이 조작할 가상의 Y값 변수

    private Tween bounceTween;
    private bool isBouncing = false;

    public event Action OnJellyTouch;

    private void Awake()
    {
        baseY = transform.position.y;
        currentY = baseY; // 초기화
    }

    public void SetJelly(Jelly jelly)
    {
        target = jelly;
        wasAboveGround = target.CurrentHeightAboveGround > touchEpsilon;
    }

    public float GetSurfaceY()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            return col.bounds.max.y;

        SpriteRenderer stoneSr = GetComponent<SpriteRenderer>();
        if (stoneSr != null)
            return stoneSr.bounds.max.y;

        return transform.position.y;
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 pos = transform.position;
        pos.x = target.transform.position.x;

        // 트위닝 중이 아닐 때는 기본 높이 유지
        if (!isBouncing)
        {
            currentY = baseY;
        }

        // DOTween이 계산한 currentY 혹은 baseY를 일괄 적용 (충돌 방지)
        pos.y = currentY;
        transform.position = pos;

        bool isAboveGround = target.CurrentHeightAboveGround > touchEpsilon;
        if (wasAboveGround && !isAboveGround)
        {
            Debug.Log("[Stone] Jelly 접촉 감지 -> OnJellyTouch 발생");
            OnJellyTouch?.Invoke();
        }

        wasAboveGround = isAboveGround;
    }

    public void PlayBounceMotion()
    {
        if (bounceTween != null && bounceTween.IsActive())
        {
            bounceTween.Kill();
            currentY = baseY; // 연속 클릭 시 튀는 현상을 막기 위해 강제 초기화
        }

        isBouncing = true;

        // Transform을 직접 움직이지 않고 currentY 변수 값만 Tweening 처리
        bounceTween = DOTween.Sequence()
            .Append(DOTween.To(() => currentY, x => currentY = x, baseY - 0.05f, 0.04f).SetEase(Ease.InQuad))
            .Append(DOTween.To(() => currentY, x => currentY = x, baseY + bounceMotionHeight, 0.08f).SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => currentY, x => currentY = x, baseY, 0.10f).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                // OnComplete에서 target을 참조하지 않으므로 NullReference 에러로부터 안전함
                currentY = baseY;
                isBouncing = false;
            });
    }
}