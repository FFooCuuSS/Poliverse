using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class enemy_1_1_test : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalColor;

    private FadeActiveToggle fade;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;

        // 페이드 컴포넌트 캐싱(없어도 동작)
        fade = GetComponent<FadeActiveToggle>();
    }

    public void Highlight(bool on)
    {
        if (sr == null) return;

        // 필요하면 여기서 색 변경 로직을 다시 살리면 됨
        // Color c = on ? Color.yellow : originalColor;
        // c.a = sr.color.a; // 알파는 페이드가 관리하니까 유지
        // sr.color = c;
    }

    // Clear는 "표현 정리 + 페이드 아웃"만 담당
    // SetActive(false)는 minigame에서 원하는 타이밍에 처리
    public void Clear()
    {
        Highlight(false);

        if (fade != null)
        {
            // FadeActiveToggle이 SetActive를 만지지 않는 버전일 때 사용
            fade.FadeOut();
        }
        else
        {
            // 페이드가 없다면 그냥 알파만 낮춰둠(즉시 꺼지는 건 바깥에서)
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0.15f;
                sr.color = c;
            }
        }
    }

    // 라운드 시작/리셋 시 상태 초기화
    public void ResetEnemy()
    {
        Highlight(false);

        // 리셋 시 "낮은 알파"로 시작시키고 싶으면 여기서 맞춰둠
        // (SetActive(false) 상태여도 값은 저장되니까 다음 SetActive(true) 후에 그대로 적용됨)
        if (fade != null)
        {
            fade.SetAlphaImmediate(fade.inactiveAlpha);
        }
        else
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0.15f;
                sr.color = c;
            }
        }
    }

    // 바깥 코드에서 필요하면 fade에 접근할 수 있게 제공
    public FadeActiveToggle TryGetFade()
    {
        return fade;
    }
}
