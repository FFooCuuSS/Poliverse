using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class enemy_1_1_test : MonoBehaviour
{
    private SpriteRenderer sr;
    private Sprite originalSprite;

    [Header("Hit Visual")]
    public Sprite hitSprite;

    private FadeActiveToggle fade;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>(true);

        if (sr != null)
            originalSprite = sr.sprite;

        fade = GetComponent<FadeActiveToggle>();
    }

    public void SetHitSprite()
    {
        if (sr == null) return;
        if (hitSprite == null) return;

        sr.sprite = hitSprite;
    }

    public void ResetSprite()
    {
        if (sr == null) return;

        sr.sprite = originalSprite;
    }

    public void Clear()
    {
        SetHitSprite();

        if (fade != null)
        {
            fade.FadeOut();
            return;
        }

        SetAlphaFallback(0.15f);
    }

    public void ResetEnemy()
    {
        ResetSprite();

        if (fade != null)
        {
            fade.SetAlphaImmediate(fade.inactiveAlpha);
            return;
        }

        SetAlphaFallback(0.15f);
    }

    public FadeActiveToggle TryGetFade()
    {
        return fade;
    }

    private void SetAlphaFallback(float alpha)
    {
        if (sr == null) return;

        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}