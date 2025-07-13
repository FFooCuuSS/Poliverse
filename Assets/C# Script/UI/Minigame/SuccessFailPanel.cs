using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuccessFailPanel : MonoBehaviour
{
    [SerializeField] private Sprite[] panelSprite; // [0] = success, [1] = failure

    private Image sourceImage;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        sourceImage = GetComponent<Image>();
    }

    public void SuccessPanel()
    {
        sourceImage.sprite = panelSprite[0];
        StartFade(0.8f); // ���� 1�� ���̵� ��
    }

    public void FailurePanel()
    {
        sourceImage.sprite = panelSprite[1];
        StartFade(0.8f);
    }

    public void hideResultPanel()
    {
        StartFade(0f); // ���� 0���� ���̵� �ƿ�
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAlpha(targetAlpha, 0.2f));
    }

    private IEnumerator FadeAlpha(float targetAlpha, float duration)
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
    }
}
