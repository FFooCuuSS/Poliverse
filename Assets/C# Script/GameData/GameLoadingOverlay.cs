using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class GameLoadingOverlay : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField]
    private CanvasGroup canvasGroup;

    [Tooltip("FadePanel을 제외한 모든 로딩 내용을 묶은 부모")]
    [SerializeField]
    private GameObject loadingContentRoot;

    [Tooltip("검은 로딩 캔버스 전체가 나타나는 시간")]
    [SerializeField, Min(0f)]
    private float canvasFadeInDuration = 0.4f;

    [Header("Black Fade Panel")]
    [Tooltip("항상 가장 위에 표시되는 전체 화면 검은 패널")]
    [SerializeField]
    private Image fadePanel;

    [Tooltip("검은 패널이 내려가며 로딩 내용이 나타나는 시간")]
    [SerializeField, Min(0f)]
    private float showFadeDuration = 0.4f;

    [Tooltip("로딩 완료 후 검은 패널이 다시 올라오는 시간")]
    [SerializeField, Min(0f)]
    private float coverFadeDuration = 0.35f;

    [Tooltip("새 씬 위에서 검은 로딩 캔버스가 사라지는 시간")]
    [SerializeField, Min(0f)]
    private float revealFadeDuration = 0.4f;

    [Header("Loading Text")]
    [SerializeField]
    private TMP_Text loadingText;

    [SerializeField]
    private string loadingBaseText = "로딩중";

    [SerializeField, Min(0.05f)]
    private float dotInterval = 0.5f;

    [Header("Random Player Text")]
    [SerializeField]
    private TMP_Text playerText;

    [SerializeField, TextArea(2, 4)]
    private string[] playerMessages;

    [Header("Progress (Optional)")]
    [SerializeField]
    private Slider progressSlider;

    private Coroutine dotCoroutine;
    private Tween transitionTween;

    private int lastMessageIndex = -1;

    private bool entranceComplete;
    private double shownRealtime;
    private double contentShownRealtime;

    public bool IsVisible =>
        gameObject.activeSelf;

    public bool IsEntranceComplete =>
        entranceComplete;

    public double ShownRealtime =>
        shownRealtime;

    public double VisibleDuration =>
        Time.realtimeSinceStartupAsDouble -
        shownRealtime;

    public double ContentVisibleDuration
    {
        get
        {
            if (!entranceComplete)
                return 0d;

            return Time.realtimeSinceStartupAsDouble -
                   contentShownRealtime;
        }
    }

    private void Awake()
    {
        FindReferences();
    }

    private void OnEnable()
    {
        FindReferences();
        PrepareInitialState();

        shownRealtime =
            Time.realtimeSinceStartupAsDouble;

        entranceComplete = false;
        contentShownRealtime = 0d;

        SetProgress(0f);
        SelectRandomMessage();

        PlayEntranceAnimation();
    }

    private void OnDisable()
    {
        StopDotAnimation();
        KillTransitionTween();

        SetLoadingContentActive(false);

        entranceComplete = false;
    }

    /// <summary>
    /// SceneFlowManager가 씬 전환을 시작할 때 호출한다.
    /// 활성화 전에 검은 패널과 콘텐츠 상태부터 준비해서
    /// 한 프레임 깜빡이는 현상을 방지한다.
    /// </summary>
    public void Show()
    {
        if (gameObject.activeSelf)
            return;

        FindReferences();
        PrepareInitialState();

        gameObject.SetActive(true);
    }

    public void HideImmediate()
    {
        StopDotAnimation();
        KillTransitionTween();

        SetLoadingContentActive(false);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // 다음 활성화 때 투명한 패널이 잠깐 보이지 않도록
        // 검은색 알파 1 상태로 미리 복구한다.
        SetFadeAlpha(1f);

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 로딩 캔버스는 검은 상태로 나타난다.
    /// 화면이 완전히 가려진 뒤 로딩 콘텐츠를 활성화하고,
    /// FadePanel을 내려서 내용을 공개한다.
    /// </summary>
    private void PlayEntranceAnimation()
    {
        KillTransitionTween();

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        SetLoadingContentActive(false);
        SetFadeAlpha(1f);

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.raycastTarget = true;
        }

        Sequence sequence = DOTween.Sequence();

        // 로딩 내용은 꺼진 상태이며,
        // 검은 FadePanel만 서서히 나타난다.
        if (canvasGroup != null)
        {
            sequence.Append(
                canvasGroup
                    .DOFade(
                        1f,
                        Mathf.Max(
                            0f,
                            canvasFadeInDuration
                        )
                    )
                    .SetEase(Ease.Linear)
            );
        }

        // 화면이 완전히 검게 가려진 뒤에만
        // 로딩 콘텐츠를 활성화한다.
        sequence.AppendCallback(
            ShowLoadingContent
        );

        // Image의 Color Alpha를 DOTween으로 직접 변경한다.
        if (fadePanel != null)
        {
            sequence.Append(
                fadePanel
                    .DOFade(
                        0f,
                        Mathf.Max(
                            0f,
                            showFadeDuration
                        )
                    )
                    .SetEase(Ease.Linear)
            );
        }

        transitionTween = sequence
            .SetUpdate(true)
            .OnComplete(
                OnEntranceAnimationComplete
            );
    }

    private void ShowLoadingContent()
    {
        SetLoadingContentActive(true);
        RestartDotAnimation();
    }

    private void OnEntranceAnimationComplete()
    {
        entranceComplete = true;

        contentShownRealtime =
            Time.realtimeSinceStartupAsDouble;

        transitionTween = null;
    }

    public IEnumerator WaitForEntrance()
    {
        while (gameObject.activeInHierarchy &&
               !entranceComplete)
        {
            yield return null;
        }
    }

    /// <summary>
    /// 로딩 완료 후 FadePanel을 다시 검게 올린다.
    /// 완전히 가려진 다음 로딩 콘텐츠를 비활성화한다.
    /// </summary>
    public IEnumerator FadeToBlack()
    {
        KillTransitionTween();

        if (fadePanel == null)
        {
            SetLoadingContentActive(false);
            StopDotAnimation();
            yield break;
        }

        fadePanel.gameObject.SetActive(true);
        fadePanel.raycastTarget = true;

        // 현재 알파값에서 1까지 DOTween으로 변경한다.
        transitionTween = fadePanel
            .DOFade(
                1f,
                Mathf.Max(
                    0f,
                    coverFadeDuration
                )
            )
            .SetEase(Ease.Linear)
            .SetUpdate(true);

        yield return transitionTween.WaitForCompletion();

        transitionTween = null;

        // 화면이 검게 덮인 뒤에만 로딩 내용을 끈다.
        StopDotAnimation();
        SetLoadingContentActive(false);
    }

    /// <summary>
    /// 새 씬이 활성화된 뒤 실행한다.
    /// 로딩 내용은 꺼져 있고 검은 FadePanel만 남아 있으므로,
    /// CanvasGroup을 페이드아웃하면 새 씬이 나타난다.
    /// </summary>
    public IEnumerator RevealSceneAndHide()
    {
        KillTransitionTween();

        StopDotAnimation();
        SetLoadingContentActive(false);
        SetFadeAlpha(1f);

        if (fadePanel != null)
            fadePanel.gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;

            transitionTween = canvasGroup
                .DOFade(
                    0f,
                    Mathf.Max(
                        0f,
                        revealFadeDuration
                    )
                )
                .SetEase(Ease.Linear)
                .SetUpdate(true);

            yield return transitionTween.WaitForCompletion();

            transitionTween = null;
        }

        HideImmediate();
    }

    public void SetProgress(float progress)
    {
        if (progressSlider == null)
            return;

        progressSlider.value =
            Mathf.Clamp01(progress);
    }

    private void RestartDotAnimation()
    {
        StopDotAnimation();

        if (loadingText == null)
            return;

        dotCoroutine =
            StartCoroutine(DotAnimationRoutine());
    }

    private void StopDotAnimation()
    {
        if (dotCoroutine == null)
            return;

        StopCoroutine(dotCoroutine);
        dotCoroutine = null;
    }

    private IEnumerator DotAnimationRoutine()
    {
        int dotCount = 1;

        while (true)
        {
            loadingText.text =
                loadingBaseText +
                new string('.', dotCount);

            dotCount++;

            if (dotCount > 3)
                dotCount = 1;

            yield return new WaitForSecondsRealtime(
                Mathf.Max(0.05f, dotInterval)
            );
        }
    }

    private void SelectRandomMessage()
    {
        if (playerText == null ||
            playerMessages == null ||
            playerMessages.Length == 0)
        {
            return;
        }

        int selectedIndex;

        if (playerMessages.Length == 1)
        {
            selectedIndex = 0;
        }
        else
        {
            do
            {
                selectedIndex = Random.Range(
                    0,
                    playerMessages.Length
                );
            }
            while (selectedIndex ==
                   lastMessageIndex);
        }

        lastMessageIndex = selectedIndex;
        playerText.text = playerMessages[selectedIndex];
    }

    private void SetLoadingContentActive(
        bool isActive)
    {
        if (loadingContentRoot == null)
            return;

        if (loadingContentRoot.activeSelf != isActive)
            loadingContentRoot.SetActive(isActive);
    }

    private void PrepareInitialState()
    {
        StopDotAnimation();
        KillTransitionTween();

        SetLoadingContentActive(false);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
        }

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.raycastTarget = true;

            SetFadeAlpha(1f);
        }
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadePanel == null)
            return;

        Color color = fadePanel.color;
        color.a = Mathf.Clamp01(alpha);
        fadePanel.color = color;
    }

    private void FindReferences()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void KillTransitionTween()
    {
        if (transitionTween != null)
        {
            transitionTween.Kill();
            transitionTween = null;
        }

        // 이전 전환이 중간에 취소돼도 잔여 Tween이 남지 않게 한다.
        if (canvasGroup != null)
            canvasGroup.DOKill();

        if (fadePanel != null)
            fadePanel.DOKill();
    }
}