using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartEnd : MonoBehaviour
{
    [Header("Countdown")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private int startCount = 5;
    [SerializeField] private float countInterval = 1f;

    [Header("Hide On Result (배경 스탠딩 등)")]
    [Tooltip("결과 연출이 시작될 때 페이드아웃 후 꺼질 오브젝트들")]
    [SerializeField] private GameObject[] hideOnResultObjects;
    [SerializeField] private float hideFadeDuration = 0.3f;

    [Header("Result Standing (PNG 시퀀스 애니메이션)")]
    [SerializeField] private GameObject resultStanding;
    [Tooltip("결과 스탠딩과 같이 나타났다가 같이 사라질 오브젝트들")]
    [SerializeField] private GameObject[] standingCompanions;
    [SerializeField] private float standingDuration = 2f;
    [SerializeField] private float standingFadeDuration = 0.5f;

    [Header("Result Background (선택)")]
    [Tooltip("등급 뒤에 깔릴 배경 스프라이트. 필요 없으면 비워둔다.")]
    [SerializeField] private GameObject finalObject;
    [SerializeField] private float finalObjectFadeDuration = 0.3f;

    [Header("Evaluation Image (S/A/B/C/D)")]
    [SerializeField] private Image evaluationImage;
    [SerializeField] private Sprite evaluationSpriteS;
    [SerializeField] private Sprite evaluationSpriteA;
    [SerializeField] private Sprite evaluationSpriteB;
    [SerializeField] private Sprite evaluationSpriteC;
    [SerializeField] private Sprite evaluationSpriteD;

    [Header("Score Character")]
    [SerializeField] private Image scoreCharacterImage;
    [SerializeField] private Sprite scoreCharacterS;
    [Tooltip("A, B 등급일 때")]
    [SerializeField] private Sprite scoreCharacterAB;
    [Tooltip("C, D 등급일 때")]
    [SerializeField] private Sprite scoreCharacterCD;

    [Header("Evaluation Pop Animation")]
    [Tooltip("최종 위치 기준, 등장 시작 위치 오프셋")]
    [SerializeField] private Vector2 evaluationPopOffset = new Vector2(250f, 200f);
    [SerializeField] private float evaluationPopDuration = 0.35f;
    [SerializeField] private float characterPopDuration = 0.3f;
    [SerializeField] private float idleScale = 1.05f;
    [SerializeField] private float idleDuration = 0.6f;

    [Header("Buttons + Same Time Object")]
    [SerializeField] private float buttonDelay = 0.5f;
    [SerializeField] private GameObject button1;
    [SerializeField] private GameObject button2;
    [SerializeField] private GameObject sameTimeObject;

    [Header("Scene")]
    [SerializeField] private string lobbySceneName = "LobbyScene";

    [Header("Debug")]
    [SerializeField] private bool debugMode;
    [SerializeField] private GameObject debugBackgroundPanel;
    [SerializeField, Range(0, 100)] private int debugFinalScore = 87;
    [SerializeField] private RunEvaluation debugEvaluation = RunEvaluation.A;
    [SerializeField] private int debugPerfect = 30;
    [SerializeField] private int debugGood = 10;
    [SerializeField] private int debugMiss = 5;

    private bool isMovingScene;
    private bool finalSequenceStarted;

    private PlanetRunResult currentResult;

    private Vector2 evaluationFinalPos;
    private Vector2 characterFinalPos;

    private void Start()
    {
        InitSpriteObject(finalObject);

        InitCanvasObject(button1);
        InitCanvasObject(button2);
        InitCanvasObject(sameTimeObject);

        if (resultStanding != null)
            resultStanding.SetActive(false);

        SetAllActive(standingCompanions, false);

        if (countdownText != null)
        {
            countdownText.text = "";
            countdownText.gameObject.SetActive(true);
        }

        // 에디터에서 배치한 위치를 최종 위치로 기억해두고 숨긴다.
        if (evaluationImage != null)
        {
            evaluationFinalPos = evaluationImage.rectTransform.anchoredPosition;
            evaluationImage.gameObject.SetActive(false);
        }

        if (scoreCharacterImage != null)
        {
            characterFinalPos = scoreCharacterImage.rectTransform.anchoredPosition;
            scoreCharacterImage.gameObject.SetActive(false);
        }

        if (debugBackgroundPanel != null)
            debugBackgroundPanel.SetActive(false);

        StartCoroutine(StartCountdownRoutine());
    }

    private void InitSpriteObject(GameObject target)
    {
        if (target == null)
            return;

        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }

        target.SetActive(false);
    }

    private void InitCanvasObject(GameObject target)
    {
        if (target == null)
            return;

        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = target.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        target.SetActive(false);
    }

    private IEnumerator StartCountdownRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        if (countdownText == null)
            yield break;

        for (int count = startCount; count >= 1; count--)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSeconds(countInterval);
        }

        countdownText.text = "";
        countdownText.gameObject.SetActive(false);

        if (debugMode && debugBackgroundPanel != null)
            debugBackgroundPanel.SetActive(true);
    }

    public void ShowFinalPanel(PlanetRunResult result)
    {
        if (finalSequenceStarted)
            return;

        if (result == null)
        {
            Debug.LogError("[GameStartEnd] 최종 결과가 null입니다.");
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
    /// 실제 게임에서는 ShowFinalPanel(result)를 사용한다.
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

        PlanetRunResult debugResult = new PlanetRunResult
        {
            planetId = 0,
            totalNode = debugPerfect + debugGood + debugMiss,
            perfect = debugPerfect,
            good = debugGood,
            miss = debugMiss,
            score = debugFinalScore,
            evaluation = debugEvaluation,
            isCleared = true
        };

        ShowFinalPanel(debugResult);
    }

    private IEnumerator FinalSequence()
    {
        // 1. 배경 스탠딩 끄기 + 결과 스탠딩 애니메이션 (동시에 진행)
        SetAllActive(hideOnResultObjects, false);
        yield return ShowResultStanding();

        // 2. (선택) 등급 뒤 배경
        yield return FadeInSpriteObject(finalObject, finalObjectFadeDuration);

        // 3. 등급 이미지 + 스코어 캐릭터 팝업
        yield return ShowEvaluation();

        // 4. 버튼 등장
        yield return new WaitForSeconds(buttonDelay);

        FadeInCanvasObject(button1, 0.4f);
        FadeInCanvasObject(button2, 0.4f);
        FadeInCanvasObject(sameTimeObject, 0.4f);
    }

    // ===== 배경 오브젝트 숨기기 =====

    private IEnumerator HideOnResultRoutine()
    {
        if (hideOnResultObjects == null || hideOnResultObjects.Length == 0)
            yield break;

        foreach (GameObject target in hideOnResultObjects)
        {
            if (target != null && target.activeInHierarchy)
                FadeOutGroup(target, hideFadeDuration);
        }

        yield return new WaitForSeconds(hideFadeDuration);

        SetAllActive(hideOnResultObjects, false);
    }

    // ===== 결과 스탠딩 =====

    private IEnumerator ShowResultStanding()
    {
        if (resultStanding == null)
            yield break;

        // 켜지는 순간 Animator 기본 상태가 처음부터 재생된다.
        ShowGroup(resultStanding);

        if (standingCompanions != null)
        {
            foreach (GameObject obj in standingCompanions)
                ShowGroup(obj);
        }

        yield return new WaitForSeconds(standingDuration);

        FadeOutGroup(resultStanding, standingFadeDuration);

        if (standingCompanions != null)
        {
            foreach (GameObject obj in standingCompanions)
                FadeOutGroup(obj, standingFadeDuration);
        }

        yield return new WaitForSeconds(standingFadeDuration);

        resultStanding.SetActive(false);
        SetAllActive(standingCompanions, false);
    }

    // ===== 등급 + 캐릭터 =====

    private Sprite GetEvaluationSprite(RunEvaluation evaluation)
    {
        switch (evaluation)
        {
            case RunEvaluation.S: return evaluationSpriteS;
            case RunEvaluation.A: return evaluationSpriteA;
            case RunEvaluation.B: return evaluationSpriteB;
            case RunEvaluation.C: return evaluationSpriteC;
            case RunEvaluation.D: return evaluationSpriteD;
            default: return null;
        }
    }

    private Sprite GetScoreCharacterSprite(RunEvaluation evaluation)
    {
        switch (evaluation)
        {
            case RunEvaluation.S:
                return scoreCharacterS;

            case RunEvaluation.A:
            case RunEvaluation.B:
                return scoreCharacterAB;

            case RunEvaluation.C:
            case RunEvaluation.D:
                return scoreCharacterCD;

            default:
                return null;
        }
    }

    private IEnumerator ShowEvaluation()
    {
        if (currentResult == null)
            yield break;

        RunEvaluation evaluation = currentResult.evaluation;

        Tween evaluationTween = null;

        // 등급 이미지: 작게 나타나 커지며 최종 위치로
        Sprite evaluationSprite = GetEvaluationSprite(evaluation);

        if (evaluationImage != null && evaluationSprite != null)
        {
            RectTransform rect = evaluationImage.rectTransform;

            evaluationImage.sprite = evaluationSprite;
            evaluationImage.preserveAspect = true;
            SetImageAlpha(evaluationImage, 1f);

            rect.anchoredPosition = evaluationFinalPos + evaluationPopOffset;
            rect.localScale = Vector3.zero;

            evaluationImage.gameObject.SetActive(true);

            Sequence seq = DOTween.Sequence().SetLink(evaluationImage.gameObject);

            seq.Join(rect.DOAnchorPos(evaluationFinalPos, evaluationPopDuration)
                .SetEase(Ease.OutCubic));
            seq.Join(rect.DOScale(1f, evaluationPopDuration)
                .SetEase(Ease.OutBack));

            seq.OnComplete(() => StartIdle(rect));

            evaluationTween = seq;
        }
        else if (evaluationImage != null)
        {
            Debug.LogWarning($"[GameStartEnd] {evaluation} 등급 스프라이트가 비어 있습니다.");
        }

        // 스코어 캐릭터: 같은 타이밍에 제자리에서 팝
        Sprite characterSprite = GetScoreCharacterSprite(evaluation);

        if (scoreCharacterImage != null && characterSprite != null)
        {
            RectTransform rect = scoreCharacterImage.rectTransform;

            scoreCharacterImage.sprite = characterSprite;
            scoreCharacterImage.preserveAspect = true;
            SetImageAlpha(scoreCharacterImage, 1f);

            rect.anchoredPosition = characterFinalPos;
            rect.localScale = Vector3.zero;

            scoreCharacterImage.gameObject.SetActive(true);

            rect.DOScale(1f, characterPopDuration)
                .SetEase(Ease.OutBack)
                .SetLink(scoreCharacterImage.gameObject)
                .OnComplete(() => StartIdle(rect));
        }
        else if (scoreCharacterImage != null)
        {
            Debug.LogWarning($"[GameStartEnd] {evaluation} 등급 캐릭터 스프라이트가 비어 있습니다.");
        }

        if (evaluationTween != null)
            yield return evaluationTween.WaitForCompletion();
        else
            yield return new WaitForSeconds(characterPopDuration);
    }

    private void StartIdle(RectTransform rect)
    {
        if (rect == null)
            return;

        rect.DOScale(idleScale, idleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(rect.gameObject);
    }

    // ===== 공용 연출 =====

    private void ShowGroup(GameObject target)
    {
        if (target == null)
            return;

        // 월드 스프라이트: 자식까지 알파 1
        foreach (SpriteRenderer sr in target.GetComponentsInChildren<SpriteRenderer>(true))
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        // Canvas UI: CanvasGroup 알파 1
        if (target.GetComponent<RectTransform>() != null)
        {
            CanvasGroup cg = target.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = target.AddComponent<CanvasGroup>();

            cg.alpha = 1f;
        }

        target.SetActive(true);
    }

    private void FadeOutGroup(GameObject target, float duration)
    {
        if (target == null)
            return;

        foreach (SpriteRenderer sr in target.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.DOFade(0f, duration)
                .SetEase(Ease.Linear)
                .SetLink(sr.gameObject);
        }

        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.DOFade(0f, duration)
                .SetEase(Ease.Linear)
                .SetLink(cg.gameObject);
        }
    }

    private void SetAllActive(GameObject[] targets, bool active)
    {
        if (targets == null)
            return;

        foreach (GameObject obj in targets)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    private IEnumerator FadeInSpriteObject(GameObject target, float duration)
    {
        if (target == null)
            yield break;

        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[GameStartEnd] {target.name}에 SpriteRenderer가 없습니다.");
            yield break;
        }

        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;

        target.SetActive(true);

        yield return spriteRenderer
            .DOFade(1f, duration)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
    }

    private void FadeInCanvasObject(GameObject target, float duration)
    {
        if (target == null)
            return;

        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = target.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        target.SetActive(true);

        canvasGroup
            .DOFade(1f, duration)
            .SetEase(Ease.Linear);
    }

    // ===== 씬 이동 =====

    public void RetryScene()
    {
        LoadSceneWithLoading(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 결과 화면의 로비 버튼에 연결한다.
    /// </summary>
    public void GoToLobbyScene()
    {
        LoadSceneWithLoading(lobbySceneName);
    }

    /// <summary>
    /// 기존 버튼 연결 호환용.
    /// </summary>
    public void GoToMenuScene()
    {
        GoToLobbyScene();
    }

    private void LoadSceneWithLoading(string sceneName)
    {
        if (isMovingScene)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[GameStartEnd] 이동할 씬 이름이 비어 있습니다.");
            return;
        }

        if (GameRoot.Instance == null || GameRoot.Instance.SceneFlow == null)
        {
            Debug.LogError(
                "[GameStartEnd] GameRoot 또는 SceneFlowManager가 없습니다. " +
                "BootStrapScene부터 실행했는지 확인하세요."
            );
            return;
        }

        isMovingScene = true;

        GameRoot.Instance.SceneFlow.LoadScene(sceneName);
    }

    // ===== 유틸 =====

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}