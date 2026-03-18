using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameStartEnd : MonoBehaviour
{
    [Header("Countdown")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private int startCount = 5;
    [SerializeField] private float countInterval = 1f;

    [Header("Final Sprite Objects")]
    [SerializeField] private GameObject finalObject;
    [SerializeField] private GameObject delayedUI1;
    [SerializeField] private GameObject delayedUI2;
    [SerializeField] private float delayedUIInterval = 0.2f;

    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private int finalScore = 87;
    [SerializeField] private float scoreDuration = 3f;

    [Header("Buttons + Same Time Object")]
    [SerializeField] private GameObject button1;
    [SerializeField] private GameObject button2;
    [SerializeField] private GameObject sameTimeObject;

    [Header("Fade Panel")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private string menuSceneName = "LobbyScene";

    private bool isMovingScene = false;
    private bool finalSequenceStarted = false;

    private void Start()
    {
        InitSpriteObject(finalObject);
        InitSpriteObject(delayedUI1);
        InitSpriteObject(delayedUI2);

        InitCanvasObject(button1);
        InitCanvasObject(button2);
        InitCanvasObject(sameTimeObject);

        if (countdownText != null)
        {
            countdownText.text = "";
            countdownText.gameObject.SetActive(true);
        }

        if (scoreText != null)
        {
            scoreText.text = "";
            Color c = scoreText.color;
            c.a = 1f;
            scoreText.color = c;
        }

        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c;
            fadePanel.gameObject.SetActive(true);
        }

        StartCoroutine(StartCountdownRoutine());
    }

    private void InitSpriteObject(GameObject obj)
    {
        if (obj == null) return;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        obj.SetActive(false);
    }

    private void InitCanvasObject(GameObject obj)
    {
        if (obj == null) return;

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = obj.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        obj.SetActive(false);
    }

    private IEnumerator StartCountdownRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        if (countdownText == null)
            yield break;

        for (int i = startCount; i >= 1; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(countInterval);
        }

        countdownText.text = "";
        countdownText.gameObject.SetActive(false);
    }

    public void ShowFinalPanel()
    {
        if (finalSequenceStarted) return;
        finalSequenceStarted = true;
        StartCoroutine(FinalSequence());
    }

    private IEnumerator FinalSequence()
    {
        yield return new WaitForSeconds(0.5f);

        yield return FadeInSpriteObject(finalObject, 1f);

        yield return new WaitForSeconds(0.5f);


        yield return StartCoroutine(ScoreRoutine());

        if (scoreText != null)
            yield return scoreText.DOFade(0f, 0.3f).SetEase(Ease.Linear).WaitForCompletion();

        yield return FadeInSpriteObject(delayedUI1, 0.3f);

        yield return new WaitForSeconds(delayedUIInterval);

        yield return FadeInSpriteObject(delayedUI2, 0.3f);

        yield return new WaitForSeconds(0.2f);

        MoveLeft(finalObject);
        MoveLeft(delayedUI1);
        MoveLeft(delayedUI2);

        yield return new WaitForSeconds(0.5f);

        FadeInCanvasObject(button1, 0.4f);
        FadeInCanvasObject(button2, 0.4f);
        FadeInCanvasObject(sameTimeObject, 0.4f);

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        MoveDown(finalObject);
        MoveDown(delayedUI1);
        MoveDown(delayedUI2);
        MoveDown(button1);
        MoveDown(button2);
        MoveDown(sameTimeObject);
    }

    private IEnumerator ScoreRoutine()
    {
        if (scoreText == null)
            yield break;

        scoreText.text = "0";
        float time = 0f;

        while (time < scoreDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / scoreDuration);
            float curved = 1f - Mathf.Pow(1f - t, 2.5f);
            int value = Mathf.RoundToInt(Mathf.Lerp(0, finalScore, curved));

            scoreText.text = value.ToString();
            yield return null;
        }

        scoreText.text = finalScore.ToString();
    }

    private IEnumerator FadeInSpriteObject(GameObject obj, float duration)
    {
        if (obj == null) yield break;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        obj.SetActive(true);

        yield return sr.DOFade(1f, duration)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
    }

    private void FadeInCanvasObject(GameObject obj, float duration)
    {
        if (obj == null) return;

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = obj.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        obj.SetActive(true);

        cg.DOFade(1f, duration).SetEase(Ease.Linear);
    }

    private void MoveLeft(GameObject obj)
    {
        if (obj == null) return;

        obj.transform.DOMoveX(obj.transform.position.x - 3f, 0.5f)
            .SetEase(Ease.OutCubic);
    }

    private void MoveDown(GameObject obj)
    {
        if (obj == null) return;

        obj.transform.DOMoveY(obj.transform.position.y - 3f, 0.5f)
            .SetEase(Ease.OutCubic);
    }

    public void RetryScene()
    {
        if (isMovingScene) return;
        StartCoroutine(FadeAndLoadScene(SceneManager.GetActiveScene().name));
    }

    public void GoToMenuScene()
    {
        if (isMovingScene) return;
        StartCoroutine(FadeAndLoadScene(menuSceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        isMovingScene = true;

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);

            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c;

            yield return fadePanel
                .DOFade(1f, fadeDuration)
                .SetEase(Ease.Linear)
                .WaitForCompletion();
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        SceneManager.LoadScene(sceneName);
    }
}