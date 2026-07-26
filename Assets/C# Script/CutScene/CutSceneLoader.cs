using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CutSceneLoader : MonoBehaviour
{
    [Header("슬라이드 생성 위치")]
    [SerializeField]
    private RectTransform slideContainer;

    [Header("Resources 경로")]
    [SerializeField]
    private string resourcesFolderPath = "CutScenes";

    [Header("슬라이드 크기")]
    [SerializeField]
    private float referenceScreenWidth = 1920f;

    [SerializeField]
    private float slideWidthRatio = 0.69f;

    [Header("슬라이드 이동 설정")]
    [SerializeField]
    private float slideGap = 80f;

    [SerializeField]
    private float slideMoveDuration = 0.3f;

    [Header("현재/비현재 슬라이드 표시")]
    [SerializeField]
    private float activeScale = 1f;

    [SerializeField]
    private float inactiveScale = 0.9f;

    [SerializeField]
    private float activeAlpha = 1f;

    [SerializeField]
    private float inactiveAlpha = 0.45f;

    [Header("컷씬 종료 후 이동할 씬")]
    [SerializeField]
    private string nextSceneName;

    private readonly List<CutsceneSlideView> slides =
        new List<CutsceneSlideView>();

    private int currentSlideIndex;
    private bool isMoving;
    private bool isEnding;

    private float SlideWidth =>
        referenceScreenWidth * slideWidthRatio;

    private float SlideSpacing =>
        SlideWidth + slideGap;

    public RectTransform CurrentSlideRect
    {
        get
        {
            if (slides.Count == 0)
                return null;

            return slides[currentSlideIndex]
                .RectTransform;
        }
    }

    private void Start()
    {
        LoadSlidesFromResources();
    }

    private void LoadSlidesFromResources()
    {
        ClearExistingSlides();

        GameObject[] loadedPrefabs =
            Resources.LoadAll<GameObject>(
                resourcesFolderPath
            );

        if (loadedPrefabs == null ||
            loadedPrefabs.Length == 0)
        {
            Debug.LogError(
                $"Resources/{resourcesFolderPath} " +
                "안에 슬라이드 프리팹이 없음"
            );

            return;
        }

        loadedPrefabs = loadedPrefabs
            .OrderBy(prefab => prefab.name)
            .ToArray();

        slides.Clear();

        for (int i = 0;
             i < loadedPrefabs.Length;
             i++)
        {
            GameObject slideObj = Instantiate(
                loadedPrefabs[i],
                slideContainer
            );

            slideObj.name =
                loadedPrefabs[i].name;

            RectTransform rect =
                slideObj.GetComponent<RectTransform>();

            if (rect == null)
            {
                Debug.LogError(
                    $"{slideObj.name}에 " +
                    "RectTransform이 없음"
                );

                Destroy(slideObj);
                continue;
            }

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                new Vector2(
                    i * SlideSpacing,
                    0f
                );

            rect.localScale = Vector3.one;

            CanvasGroup group =
                slideObj.GetComponent<CanvasGroup>();

            if (group == null)
            {
                group =
                    slideObj.AddComponent<CanvasGroup>();
            }

            CutsceneSlideView slideView =
                slideObj.GetComponent<CutsceneSlideView>();

            if (slideView == null)
            {
                Debug.LogError(
                    $"{slideObj.name}에 " +
                    "CutsceneSlideView가 없음"
                );

                Destroy(slideObj);
                continue;
            }

            slideView.Initialize(this);
            slides.Add(slideView);
        }

        currentSlideIndex = 0;
        isMoving = false;
        isEnding = false;

        slideContainer.anchoredPosition =
            Vector2.zero;

        UpdateSlideVisualsImmediate();

        Debug.Log(
            $"컷씬 슬라이드 " +
            $"{slides.Count}개 로드 완료"
        );
    }

    private void ClearExistingSlides()
    {
        if (slideContainer == null)
        {
            Debug.LogError(
                "SlideContainer가 연결되지 않음"
            );

            return;
        }

        for (int i =
                 slideContainer.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                slideContainer
                    .GetChild(i)
                    .gameObject
            );
        }
    }

    public void OnTap()
    {
        if (isMoving || isEnding)
            return;

        if (slides.Count == 0)
            return;

        CutsceneSlideView currentSlide =
            slides[currentSlideIndex];

        bool playedStep =
            currentSlide.PlayNextStep(this);

        if (playedStep)
            return;

        GoNextSlide();
    }

    public void GoNextSlide()
    {
        if (isMoving || isEnding)
            return;

        if (slides.Count == 0)
            return;

        if (currentSlideIndex >=
            slides.Count - 1)
        {
            EndCutscene();
            return;
        }

        currentSlideIndex++;

        slides[currentSlideIndex]
            .ResetSteps(this);

        StartCoroutine(
            MoveSlideContainer()
        );
    }

    public void GoPrevSlide()
    {
        if (isMoving || isEnding)
            return;

        if (slides.Count == 0)
            return;

        if (currentSlideIndex <= 0)
            return;

        currentSlideIndex--;

        slides[currentSlideIndex]
            .ShowAllSteps(this);

        StartCoroutine(
            MoveSlideContainer()
        );
    }

    private IEnumerator MoveSlideContainer()
    {
        isMoving = true;

        Vector2 startPos =
            slideContainer.anchoredPosition;

        Vector2 targetPos =
            new Vector2(
                -currentSlideIndex *
                SlideSpacing,
                0f
            );

        StartCoroutine(
            UpdateSlideVisualsSmooth()
        );

        float timer = 0f;

        while (timer < slideMoveDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / slideMoveDuration
            );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            slideContainer.anchoredPosition =
                Vector2.Lerp(
                    startPos,
                    targetPos,
                    t
                );

            yield return null;
        }

        slideContainer.anchoredPosition =
            targetPos;

        UpdateSlideVisualsImmediate();

        isMoving = false;
    }

    private IEnumerator UpdateSlideVisualsSmooth()
    {
        float timer = 0f;

        Vector3[] startScales =
            new Vector3[slides.Count];

        float[] startAlphas =
            new float[slides.Count];

        for (int i = 0;
             i < slides.Count;
             i++)
        {
            RectTransform rect =
                slides[i].RectTransform;

            CanvasGroup group =
                rect.GetComponent<CanvasGroup>();

            startScales[i] =
                rect.localScale;

            startAlphas[i] =
                group != null
                    ? group.alpha
                    : 1f;
        }

        while (timer < slideMoveDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / slideMoveDuration
            );

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            for (int i = 0;
                 i < slides.Count;
                 i++)
            {
                bool isCurrent =
                    i == currentSlideIndex;

                RectTransform rect =
                    slides[i].RectTransform;

                CanvasGroup group =
                    rect.GetComponent<CanvasGroup>();

                Vector3 targetScale =
                    Vector3.one *
                    (isCurrent
                        ? activeScale
                        : inactiveScale);

                float targetAlpha =
                    isCurrent
                        ? activeAlpha
                        : inactiveAlpha;

                rect.localScale =
                    Vector3.Lerp(
                        startScales[i],
                        targetScale,
                        t
                    );

                if (group != null)
                {
                    group.alpha =
                        Mathf.Lerp(
                            startAlphas[i],
                            targetAlpha,
                            t
                        );
                }
            }

            yield return null;
        }
    }

    private void UpdateSlideVisualsImmediate()
    {
        for (int i = 0;
             i < slides.Count;
             i++)
        {
            bool isCurrent =
                i == currentSlideIndex;

            RectTransform rect =
                slides[i].RectTransform;

            CanvasGroup group =
                rect.GetComponent<CanvasGroup>();

            rect.localScale =
                Vector3.one *
                (isCurrent
                    ? activeScale
                    : inactiveScale);

            if (group != null)
            {
                group.alpha =
                    isCurrent
                        ? activeAlpha
                        : inactiveAlpha;
            }
        }
    }

    public void SkipCutscene()
    {
        if (isEnding)
            return;

        EndCutscene();
    }

    private void EndCutscene()
    {
        if (isEnding)
            return;

        if (string.IsNullOrWhiteSpace(
                nextSceneName))
        {
            Debug.LogError(
                "[CutSceneLoader] " +
                "Next Scene Name이 비어 있습니다."
            );

            return;
        }

        if (GameRoot.Instance == null ||
            GameRoot.Instance.SceneFlow == null)
        {
            Debug.LogError(
                "[CutSceneLoader] GameRoot 또는 " +
                "SceneFlowManager가 없습니다. " +
                "BootStrapScene부터 실행했는지 확인하세요."
            );

            return;
        }

        isEnding = true;
        isMoving = true;

        GameRoot.Instance.SceneFlow.LoadScene(
            nextSceneName
        );
    }
}