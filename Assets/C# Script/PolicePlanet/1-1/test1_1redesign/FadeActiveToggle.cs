using System.Collections;
using UnityEngine;

public class FadeActiveToggle : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeTime = 0.12f;        // 페이드 시간
    public float inactiveAlpha = 0.15f;   // 기본 낮은 알파

    [Header("Targets (비우면 자동 탐색)")]
    public SpriteRenderer[] spriteRenderers;

    private Coroutine fadeCo;

    private void Awake()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    // 외부에서 SetActive(true) 한 다음 호출하는 용도
    public void FadeIn()
    {
        StartFadeTo(1f);
    }

    // 외부에서 SetActive(false) 하기 전에 호출하는 용도
    public void FadeOut()
    {
        StartFadeTo(inactiveAlpha);
    }

    // 외부에서 바로 알파 세팅(리셋용)
    public void SetAlphaImmediate(float a)
    {
        StopFade();
        SetAlpha(a);
    }

    public void StopFade()
    {
        if (fadeCo != null)
        {
            StopCoroutine(fadeCo);
            fadeCo = null;
        }
    }

    public float GetFadeTime()
    {
        return fadeTime;
    }

    void StartFadeTo(float targetAlpha)
    {
        StopFade();
        fadeCo = StartCoroutine(FadeRoutine(targetAlpha));
    }

    IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = 1f;

        if (spriteRenderers != null && spriteRenderers.Length > 0 && spriteRenderers[0] != null)
            startAlpha = spriteRenderers[0].color.a;

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / fadeTime);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(targetAlpha);
        fadeCo = null;
    }

    void SetAlpha(float a)
    {
        if (spriteRenderers == null) return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null) continue;
            Color c = spriteRenderers[i].color;
            c.a = a;
            spriteRenderers[i].color = c;
        }
    }
}
