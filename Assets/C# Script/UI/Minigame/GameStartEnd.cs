using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartEnd : MonoBehaviour
{
    [Header("Countdown")]
    [SerializeField]
    private TMP_Text countdownText;

    [SerializeField]
    private float startDelay = 2f;

    [SerializeField]
    private int startCount = 5;

    [SerializeField]
    private float countInterval = 1f;

    [Header("Final Sprite Objects")]
    [SerializeField]
    private GameObject finalObject;

    [Tooltip("평가 텍스트 이후에 나타날 연출 오브젝트")]
    [SerializeField]
    private GameObject delayedUI2;

    [SerializeField]
    private float delayedUIInterval = 0.2f;

    [Header("Score")]
    [SerializeField]
    private TMP_Text scoreText;

    [Tooltip("기존 Delayed UI 1 자리에 사용할 평가 텍스트")]
    [SerializeField]
    private TMP_Text evaluationText;

    [SerializeField]
    private float scoreDuration = 3f;

    [Header("Result Movement")]
    [Tooltip("월드 오브젝트가 왼쪽으로 이동할 거리")]
    [SerializeField]
    private float worldMoveLeftAmount = 3f;

    [Tooltip("Canvas UI가 왼쪽으로 이동할 픽셀 거리")]
    [SerializeField]
    private float uiMoveLeftAmount = 300f;

    [SerializeField]
    private float resultMoveDuration = 0.5f;

    [Header("Buttons + Same Time Object")]
    [SerializeField]
    private GameObject button1;

    [SerializeField]
    private GameObject button2;

    [SerializeField]
    private GameObject sameTimeObject;

    [Header("Scene")]
    [SerializeField]
    private string lobbySceneName = "LobbyScene";

    [Header("Debug")]
    [SerializeField]
    private bool debugMode;

    [SerializeField]
    private GameObject debugBackgroundPanel;

    [SerializeField, Range(0, 100)]
    private int debugFinalScore = 87;

    [SerializeField]
    private RunEvaluation debugEvaluation =
        RunEvaluation.A;

    private bool isMovingScene;
    private bool finalSequenceStarted;

    private PlanetRunResult currentResult;

    private void Start()
    {
        InitSpriteObject(finalObject);
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
            SetTextAlpha(scoreText, 1f);
        }

        if (evaluationText != null)
        {
            evaluationText.text = "";
            SetTextAlpha(evaluationText, 0f);
            evaluationText.gameObject.SetActive(false);
        }

        if (debugBackgroundPanel != null)
            debugBackgroundPanel.SetActive(false);

        StartCoroutine(
            StartCountdownRoutine()
        );
    }

    private void InitSpriteObject(
        GameObject target)
    {
        if (target == null)
            return;

        SpriteRenderer spriteRenderer =
            target.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            Color color =
                spriteRenderer.color;

            color.a = 0f;

            spriteRenderer.color =
                color;
        }

        target.SetActive(false);
    }

    private void InitCanvasObject(
        GameObject target)
    {
        if (target == null)
            return;

        CanvasGroup canvasGroup =
            target.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                target.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        target.SetActive(false);
    }

    private IEnumerator StartCountdownRoutine()
    {
        yield return new WaitForSeconds(
            startDelay
        );

        if (countdownText == null)
            yield break;

        for (int count = startCount;
             count >= 1;
             count--)
        {
            countdownText.text =
                count.ToString();

            yield return new WaitForSeconds(
                countInterval
            );
        }

        countdownText.text = "";
        countdownText.gameObject.SetActive(false);

        if (debugMode &&
            debugBackgroundPanel != null)
        {
            debugBackgroundPanel.SetActive(true);
        }
    }

    public void ShowFinalPanel(
        PlanetRunResult result)
    {
        if (finalSequenceStarted)
            return;

        if (result == null)
        {
            Debug.LogError(
                "[GameStartEnd] 최종 결과가 null입니다."
            );

            return;
        }

        currentResult = result;
        finalSequenceStarted = true;

        if (debugBackgroundPanel != null)
            debugBackgroundPanel.SetActive(false);

        StartCoroutine(FinalSequence());
    }

    /// <summary>
    /// 결과 연출만 별도로 확인할 때 사용한다.
    /// 실제 게임에서는 PlanetRunResult를 전달하는
    /// ShowFinalPanel(result)를 사용한다.
    /// </summary>
    public void ShowFinalPanel()
    {
        if (!debugMode)
        {
            Debug.LogError(
                "[GameStartEnd] 실제 게임에서 매개변수 없는 " +
                "ShowFinalPanel()이 호출되었습니다."
            );

            return;
        }

        PlanetRunResult debugResult =
            new PlanetRunResult
            {
                planetId = 0,
                totalNode = 1,
                score = debugFinalScore,
                evaluation = debugEvaluation,
                isCleared = true
            };

        ShowFinalPanel(debugResult);
    }

    private IEnumerator FinalSequence()
    {
        yield return new WaitForSeconds(0.5f);

        yield return FadeInSpriteObject(
            finalObject,
            1f
        );

        yield return new WaitForSeconds(0.5f);

        yield return ScoreRoutine();

        if (scoreText != null)
        {
            yield return scoreText
                .DOFade(0f, 0.3f)
                .SetEase(Ease.Linear)
                .WaitForCompletion();
        }

        // 기존 DelayedUI1 대신 평가 텍스트 등장
        yield return ShowEvaluation();

        yield return new WaitForSeconds(
            delayedUIInterval
        );

        yield return FadeInSpriteObject(
            delayedUI2,
            0.3f
        );

        yield return new WaitForSeconds(0.2f);

        // 기존 DelayedUI1의 이동 연출을
        // EvaluationText가 대신 사용한다.
        MoveLeft(finalObject);

        if (evaluationText != null)
        {
            MoveLeft(
                evaluationText.gameObject
            );
        }

        MoveLeft(delayedUI2);

        yield return new WaitForSeconds(
            resultMoveDuration
        );

        FadeInCanvasObject(
            button1,
            0.4f
        );

        FadeInCanvasObject(
            button2,
            0.4f
        );

        FadeInCanvasObject(
            sameTimeObject,
            0.4f
        );
    }

    private IEnumerator ScoreRoutine()
    {
        if (scoreText == null ||
            currentResult == null)
        {
            yield break;
        }

        scoreText.gameObject.SetActive(true);
        SetTextAlpha(scoreText, 1f);

        scoreText.text = "0";

        float time = 0f;

        while (time < scoreDuration)
        {
            time += Time.deltaTime;

            float normalized =
                Mathf.Clamp01(
                    time / scoreDuration
                );

            float curved =
                1f -
                Mathf.Pow(
                    1f - normalized,
                    2.5f
                );

            int value =
                Mathf.RoundToInt(
                    Mathf.Lerp(
                        0,
                        currentResult.score,
                        curved
                    )
                );

            scoreText.text =
                value.ToString();

            yield return null;
        }

        scoreText.text =
            currentResult.score.ToString();
    }

    private IEnumerator ShowEvaluation()
    {
        if (evaluationText == null ||
            currentResult == null)
        {
            yield break;
        }

        evaluationText.text =
            currentResult.evaluation ==
            RunEvaluation.None
                ? "-"
                : currentResult
                    .evaluation
                    .ToString();

        SetTextAlpha(
            evaluationText,
            0f
        );

        evaluationText
            .gameObject
            .SetActive(true);

        yield return evaluationText
            .DOFade(1f, 0.3f)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
    }

    private IEnumerator FadeInSpriteObject(
        GameObject target,
        float duration)
    {
        if (target == null)
            yield break;

        SpriteRenderer spriteRenderer =
            target.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning(
                $"[GameStartEnd] " +
                $"{target.name}에 SpriteRenderer가 없습니다."
            );

            yield break;
        }

        Color color =
            spriteRenderer.color;

        color.a = 0f;

        spriteRenderer.color =
            color;

        target.SetActive(true);

        yield return spriteRenderer
            .DOFade(1f, duration)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
    }

    private void FadeInCanvasObject(
        GameObject target,
        float duration)
    {
        if (target == null)
            return;

        CanvasGroup canvasGroup =
            target.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                target.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        target.SetActive(true);

        canvasGroup
            .DOFade(1f, duration)
            .SetEase(Ease.Linear);
    }

    private void MoveLeft(
        GameObject target)
    {
        if (target == null)
            return;

        RectTransform rectTransform =
            target.GetComponent<RectTransform>();

        bool isCanvasUI =
            rectTransform != null &&
            target.GetComponentInParent<Canvas>() != null;

        if (isCanvasUI)
        {
            float targetX =
                rectTransform.anchoredPosition.x -
                uiMoveLeftAmount;

            rectTransform
                .DOAnchorPosX(
                    targetX,
                    resultMoveDuration
                )
                .SetEase(Ease.OutCubic);

            return;
        }

        float worldTargetX =
            target.transform.position.x -
            worldMoveLeftAmount;

        target.transform
            .DOMoveX(
                worldTargetX,
                resultMoveDuration
            )
            .SetEase(Ease.OutCubic);
    }

    public void RetryScene()
    {
        LoadSceneWithLoading(
            SceneManager
                .GetActiveScene()
                .name
        );
    }

    /// <summary>
    /// 결과 화면의 로비 버튼에 연결한다.
    /// 전역 로딩창을 띄운 뒤 LobbyScene으로 이동한다.
    /// </summary>
    public void GoToLobbyScene()
    {
        LoadSceneWithLoading(
            lobbySceneName
        );
    }

    /// <summary>
    /// 기존 버튼 연결 호환용.
    /// 기존 GoToMenuScene 연결을 유지해도
    /// 로비로 이동한다.
    /// </summary>
    public void GoToMenuScene()
    {
        GoToLobbyScene();
    }

    private void LoadSceneWithLoading(
        string sceneName)
    {
        if (isMovingScene)
            return;

        if (string.IsNullOrWhiteSpace(
                sceneName))
        {
            Debug.LogError(
                "[GameStartEnd] " +
                "이동할 씬 이름이 비어 있습니다."
            );

            return;
        }

        if (GameRoot.Instance == null ||
            GameRoot.Instance.SceneFlow == null)
        {
            Debug.LogError(
                "[GameStartEnd] GameRoot 또는 " +
                "SceneFlowManager가 없습니다. " +
                "BootStrapScene부터 실행했는지 확인하세요."
            );

            return;
        }

        isMovingScene = true;

        GameRoot.Instance.SceneFlow.LoadScene(
            sceneName
        );
    }

    private void SetTextAlpha(
        TMP_Text text,
        float alpha)
    {
        if (text == null)
            return;

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
}