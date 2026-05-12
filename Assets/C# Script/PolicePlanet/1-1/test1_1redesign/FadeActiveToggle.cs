using System.Collections;
using UnityEngine;

public class FadeActiveToggle : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeTime = 0.12f;
    public float inactiveAlpha = 0.15f;

    [Header("Targets (비우면 자동 탐색)")]
    public SpriteRenderer[] spriteRenderers;

    private Coroutine fadeCo;

    private void Awake()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    public void FadeIn()
    {
        StartFadeTo(1f);
    }

    public void FadeOut()
    {
        StartFadeTo(inactiveAlpha);
    }

    public void SetAlphaImmediate(float alpha)
    {
        StopFade();
        SetAlpha(alpha);
    }

    public float GetFadeTime()
    {
        return fadeTime;
    }

    private void StartFadeTo(float targetAlpha)
    {
        StopFade();
        fadeCo = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private void StopFade()
    {
        if (fadeCo == null) return;

        StopCoroutine(fadeCo);
        fadeCo = null;
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = GetCurrentAlpha();
        float t = 0f;
        float dur = Mathf.Max(0.01f, fadeTime);

        while (t < dur)
        {
            t += Time.deltaTime;

            float a = Mathf.Lerp(startAlpha, targetAlpha, t / dur);
            SetAlpha(a);

            yield return null;
        }

        SetAlpha(targetAlpha);
        fadeCo = null;
    }

    private float GetCurrentAlpha()
    {
        if (spriteRenderers == null) return 1f;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                return spriteRenderers[i].color.a;
        }

        return 1f;
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderers == null) return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer sr = spriteRenderers[i];
            if (sr == null) continue;

            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}