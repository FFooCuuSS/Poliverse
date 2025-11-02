using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SuccessFailPanel : MonoBehaviour
{
    [SerializeField] private Sprite[] panelSprite; // [0] = success, [1] = failure
    [SerializeField] private float autoHideDelay = 1f; // ÀÚµ¿ ¼û±è ´ë±â ½Ã°£

    private Image sourceImage;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        sourceImage = GetComponent<Image>();
    }

    public void SuccessPanel()
    {
        sourceImage.sprite = panelSprite[0];
        StartFade(1f, 0.2f, true); 
    }

    public void FailurePanel()
    {
        sourceImage.sprite = panelSprite[1];
        StartFade(1f, 0.2f, true);
    }

    public void HideResultPanel()
    {
        StartFade(0f, 0.2f, false);
    }

    private void StartFade(float targetAlpha, float duration, bool autoHide)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAlpha(targetAlpha, duration, autoHide));
    }

    private IEnumerator FadeAlpha(float targetAlpha, float duration, bool autoHide)
    {
        float startAlpha = sourceImage.color.a;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            sourceImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        sourceImage.color = new Color(1f, 1f, 1f, targetAlpha);

        // hide
        if (autoHide && targetAlpha > 0f)
        {
            yield return new WaitForSeconds(autoHideDelay);
            yield return StartCoroutine(FadeAlpha(0f, 0.5f, false));
        }
    }
}
