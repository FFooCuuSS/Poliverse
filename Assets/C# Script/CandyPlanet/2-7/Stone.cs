using System;
using DG.Tweening;
using UnityEngine;

// 플레이어의 플랫폼. 항상 현재 Jelly의 x값을 따라가고,
// Collider/Trigger 없이 Jelly의 계산된 높이값을 매 프레임 직접 폴링해서
// "바닥에 닿았는지"를 코드로만 판정한다.
public class Stone : MonoBehaviour
{
    [SerializeField] private float touchEpsilon = 0.05f; // 이 값 이하로 내려오면 "닿음"으로 판정

    [Header("클릭 시 튕기는 연출")]
    [SerializeField] private float bounceMotionHeight = 0.3f;   // 위로 튕기는 높이
    [SerializeField] private float bounceMotionDuration = 0.10f; // 왔다갔다 전체 시간

    private Jelly target;
    private bool wasAboveGround = true;

    private float baseY;
    private float bounceOffsetY = 0f; // x추적 로직과 별개로 y에만 더해지는 연출용 오프셋
    private Tween bounceTween;

    public event Action OnJellyTouch;

    private void Awake()
    {
        baseY = transform.position.y;
    }

    public void SetJelly(Jelly jelly)
    {
        target = jelly;
        wasAboveGround = target.CurrentHeightAboveGround > touchEpsilon;
    }

    // Stone의 실제 "표면" y값. Collider2D가 있으면 그 상단, 없으면 스프라이트 상단을 사용.
    // (Stone도 피벗이 Center일 수 있어서 transform.position.y만 쓰면 표면보다 낮게 잡힘)
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

        // x는 항상 젤리를 추적, y는 기준 위치 + 연출용 오프셋을 합산
        // (연출(bounceOffsetY)이 이 x추적 로직과 충돌하지 않도록 오프셋 값만 트윈함)
        Vector3 pos = transform.position;
        pos.x = target.transform.position.x;
        pos.y = baseY + bounceOffsetY;
        transform.position = pos;

        bool isAboveGround = target.CurrentHeightAboveGround > touchEpsilon;
       // Debug.Log($"height={target.CurrentHeightAboveGround}, above={isAboveGround}");
        if (wasAboveGround && !isAboveGround)
        {
            Debug.Log("[Stone] Jelly 접촉 감지 -> OnJellyTouch 발생");
            OnJellyTouch?.Invoke();
        }

        wasAboveGround = isAboveGround;
    }

    // 플레이어가 클릭했을 때 탁구채로 공을 치듯 위로 빠르게 튕겼다가 깔끔하게 제자리로
    public void PlayBounceMotion()
    {
        bounceTween?.Kill();
        bounceOffsetY = 0f;

        bounceTween = DOTween.Sequence()
            .Append(DOTween.To(() => bounceOffsetY, x => bounceOffsetY = x, bounceMotionHeight, 0.03f).SetEase(Ease.OutExpo)
    ).Append(DOTween.To(() => bounceOffsetY, x => bounceOffsetY = x, 0f, 0.07f)
        .SetEase(Ease.InQuad)
    );
    }
}