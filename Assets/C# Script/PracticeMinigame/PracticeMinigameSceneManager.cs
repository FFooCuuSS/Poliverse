using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PracticePhase
{
    None,
    Title,
    Practice,
    Transition,
    Finished
}

public enum PracticePlayMode
{
    Demo,
    Player
}

public class PracticeMinigameSceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private RhythmManager rhythmManager;
    [SerializeField]
    private PracticeDemoManager practiceDemoManager;

    [Tooltip(
        "로딩 및 시범 중 플레이어 입력을 막는 패널"
    )]
    [SerializeField]
    private GameObject blockInputPanel;

    [Tooltip(
        "미니게임이 위치를 변경할 수 있는 메인 카메라"
    )]
    [SerializeField]
    private Transform mainCameraTransform;

    [Header("Practice Music")]
    [Tooltip(
        "연습 모드에서 사용할 별도 음악. " +
        "RhythmManager의 AudioSource로 재생된다."
    )]
    [SerializeField]
    private AudioClip practiceMusic;

    [Header("Rhythm Chart")]
    [Tooltip(
        "모든 미니게임의 time/type 행이 들어 있는 전체 CSV"
    )]
    [SerializeField]
    private TextAsset chartCsv;

    [Header("Practice Guide")]
    [Tooltip(
        "Custom 시범 행동과 설명 해금 타이밍이 들어 있는 연습용 CSV"
    )]
    [SerializeField]
    private TextAsset practiceGuideCsv;

    [Header("Exit")]
    [Tooltip(
        "Session에 돌아갈 씬이 없을 경우 사용할 씬"
    )]
    [SerializeField]
    private string fallbackExitSceneName =
        "LobbyScene";

    [Header("Practice UI")]
    [SerializeField]
    private GameObject titlePanel;

    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private GameObject practicePanel;

    [SerializeField]
    private PracticeGuideTextController
        guideTextController;

    [SerializeField]
    private PracticePanelToggle
        practicePanelToggle;

    [SerializeField]
    private GameObject modeButton;

    [SerializeField]
    private TMP_Text modeButtonText;

    [SerializeField]
    private GameObject nextButton;

    [Header("Transition")]
    [SerializeField]
    private GameObject transitionPanel;

    [SerializeField, Min(0f)]
    private float titleFadeDuration = 0.35f;

    [SerializeField, Min(0f)]
    private float transitionFadeDuration = 0.4f;

    private CanvasGroup titleCanvasGroup;
    private CanvasGroup transitionCanvasGroup;

    private MiniGameBase currentMinigame;
    private GameObject currentMinigameObject;

    private Coroutine practiceCoroutine;
    private bool isPracticing;

    private int selectedPlanet;
    private int selectedTrack;

    private List<int> trackMinigames =
        new List<int>();

    private int currentTrackIndex;

    private int CurrentMinigameId =>
        trackMinigames[currentTrackIndex];

    private PracticePhase currentPhase =
        PracticePhase.None;

    private PracticePlayMode playMode =
        PracticePlayMode.Demo;

    private bool titleConfirmed;
    private bool modeChangeRequested;
    private bool nextRequested;

    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;

    private void Awake()
    {
        if (rhythmManager == null)
        {
            rhythmManager =
                FindObjectOfType<RhythmManager>();
        }

        if (mainCameraTransform == null &&
            Camera.main != null)
        {
            mainCameraTransform =
                Camera.main.transform;
        }

        if (mainCameraTransform != null)
        {
            initialCameraPosition =
                mainCameraTransform.position;

            initialCameraRotation =
                mainCameraTransform.rotation;
        }

        if (blockInputPanel != null)
            blockInputPanel.SetActive(true);

        if (titlePanel != null)
            titlePanel.SetActive(false);

        if (practicePanel != null)
            practicePanel.SetActive(false);

        if (modeButton != null)
            modeButton.SetActive(false);

        if (nextButton != null)
            nextButton.SetActive(false);

        if (titlePanel != null)
        {
            titleCanvasGroup =
                titlePanel.GetComponent<CanvasGroup>();

            if (titleCanvasGroup == null)
            {
                Debug.LogError(
                    "[Practice] TitlePanel에 CanvasGroup이 없습니다."
                );
            }
        }

        if (transitionPanel != null)
        {
            transitionCanvasGroup =
                transitionPanel.GetComponent<CanvasGroup>();

            if (transitionCanvasGroup == null)
            {
                Debug.LogError(
                    "[Practice] TransitionPanel에 CanvasGroup이 없습니다."
                );
            }
        }

        if (titleCanvasGroup != null)
        {
            titleCanvasGroup.alpha = 1f;
        }

        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
        }

        if (transitionPanel != null)
        {
            transitionPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // 기본 테스트값.
        // 로비에서 정상적으로 훈련 트랙을 전달받았다면
        // 아래에서 해당 값으로 덮어쓴다.
        selectedPlanet = 1;
        selectedTrack = 1;

        bool hasPracticeSelection = false;

        if (GameRoot.Instance != null &&
            GameRoot.Instance.Session != null &&
            GameRoot.Instance.Session.Data != null)
        {
            GameSessionData session =
                GameRoot.Instance.Session.Data;

            if (session.gameMode == GameMode.Practice &&
                session.selectedPlanetId > 0 &&
                session.selectedPracticeTrackId > 0)
            {
                selectedPlanet =
                    session.selectedPlanetId;

                selectedTrack =
                    session.selectedPracticeTrackId;

                hasPracticeSelection = true;
            }
        }

        if (!hasPracticeSelection)
        {
            Debug.LogWarning(
                "[Practice] 전달된 훈련 트랙이 없습니다. " +
                "테스트용 1_1 훈련을 실행합니다."
            );
        }

        if (selectedPlanet < 1 ||
            selectedPlanet > 4)
        {
            Debug.LogError(
                $"[Practice] 잘못된 행성 번호: " +
                $"{selectedPlanet}"
            );

            ReturnToExitScene();
            return;
        }

        int maxTrackCount =
            selectedPlanet == 1 ? 4 : 5;

        if (selectedTrack < 1 ||
            selectedTrack > maxTrackCount)
        {
            Debug.LogError(
                $"[Practice] 잘못된 훈련 트랙: " +
                $"{selectedPlanet}_{selectedTrack}"
            );

            ReturnToExitScene();
            return;
        }

        trackMinigames =
            PracticeTrackCatalog.GetMinigames(
                selectedPlanet,
                selectedTrack
            );

        if (trackMinigames.Count == 0)
        {
            Debug.LogError(
                "[Practice] 훈련 트랙에 " +
                "미니게임이 없습니다."
            );

            ReturnToExitScene();
            return;
        }

        currentTrackIndex = 0;

        if (rhythmManager == null)
        {
            Debug.LogError(
                "[Practice] RhythmManager가 없습니다."
            );

            ReturnToExitScene();
            return;
        }

        if (chartCsv == null)
        {
            Debug.LogError(
                "[Practice] 전체 리듬 CSV가 " +
                "할당되지 않았습니다."
            );

            ReturnToExitScene();
            return;
        }

        // PracticeScene을 직접 실행하는 테스트 상황에서는
        // GameRoot가 없을 수도 있으므로 null 검사.
        if (GameRoot.Instance != null &&
            GameRoot.Instance.Audio != null)
        {
            GameRoot.Instance.Audio.StopBgm();
        }

        rhythmManager.SetTimelineMusic(
            practiceMusic,
            false
        );

        if (practiceMusic == null)
        {
            Debug.LogWarning(
                "[Practice] Practice Music이 없습니다. " +
                "음악 없이 타임라인만 실행됩니다."
            );
        }

        isPracticing = true;

        practiceCoroutine =
            StartCoroutine(PracticeLoop());
    }

    private IEnumerator PracticeLoop()
    {
        for (currentTrackIndex = 0;
             currentTrackIndex <
             trackMinigames.Count;
             currentTrackIndex++)
        {
            if (!isPracticing)
                yield break;

            yield return CreateMinigame();

            if (!isPracticing ||
                currentMinigame == null)
            {
                yield break;
            }

            yield return ShowTitlePhase();

            if (!isPracticing)
                yield break;

            playMode =
                PracticePlayMode.Demo;

            yield return
                PracticeCurrentMinigame();

            if (!isPracticing)
                yield break;

            currentPhase =
                PracticePhase.Transition;

            /*
             * 현재 게임 화면을 검게 덮은 뒤
             * 프리팹을 교체한다.
             */
            yield return
                FadeTransitionTo(1f);

            DestroyCurrentMinigame();
            ResetCamera();

            yield return null;

            /*
             * 다음 for 반복에서
             * CreateMinigame()
             * ShowTitlePhase()
             *
             * 순으로 실행된다.
             *
             * ShowTitlePhase에서
             * FadeTransitionTo(0)가 실행된다.
             */
        }

        currentPhase =
            PracticePhase.Finished;

        practiceCoroutine = null;
    }

    private IEnumerator PracticeCurrentMinigame()
    {
        currentPhase =
            PracticePhase.Practice;

        nextRequested = false;

        if (practicePanel != null)
        {
            practicePanel.SetActive(true);
        }

        if (guideTextController != null &&
            currentMinigame != null)
        {
            guideTextController.Initialize(
                currentMinigame.GetMinigameExplains
            );
        }

        if (practicePanelToggle != null)
        {
            practicePanelToggle.OpenImmediate();
        }

        while (isPracticing &&
               !nextRequested)
        {
            modeChangeRequested = false;

            UpdatePracticeUI();

            bool isDemo =
                playMode ==
                PracticePlayMode.Demo;

            /*
             * 예시보기에서는 실제 플레이어 터치를 막는다.
             */
            if (blockInputPanel != null)
            {
                blockInputPanel.SetActive(
                    isDemo
                );
            }

            /*
             * 현재 미니게임 초기화.
             */
            currentMinigame.StartGame();

            yield return null;

            /*
             * 예시보기일 때만
             * RhythmManager의 입력 이벤트를 받아
             * 자동 행동을 실행한다.
             */
            if (isDemo)
            {
                if (practiceDemoManager != null)
                {
                    string minigameId =
                        $"{selectedPlanet}-{CurrentMinigameId}";

                    practiceDemoManager.Begin(
                        currentMinigame,
                        rhythmManager,
                        practiceGuideCsv,
                        minigameId,
                        UnlockGuideText
                    );
                }
            }
            else
            {
                if (practiceDemoManager != null)
                {
                    practiceDemoManager.Stop();
                }
            }

            /*
             * DemoManager를 먼저 연결한 뒤
             * 타임라인을 시작한다.
             */
            rhythmManager.StartSong();

            yield return new WaitUntil(
                () =>
                    !isPracticing ||
                    nextRequested ||
                    modeChangeRequested ||
                    rhythmManager.HasDispatchedAllEvents
            );

            /*
             * 이번 재생이 끝났으므로
             * 이벤트 구독을 반드시 해제한다.
             */
            if (practiceDemoManager != null)
            {
                practiceDemoManager.Stop();
            }

            if (!isPracticing ||
                nextRequested)
            {
                break;
            }

            /*
             * 한 번 재생이 끝났거나
             * Demo <-> Player 모드를 바꿨으면
             * 미니게임을 처음 상태로 다시 만든다.
             */
            DestroyCurrentMinigame();
            ResetCamera();

            yield return null;

            yield return CreateMinigame();

            if (!isPracticing ||
                currentMinigame == null)
            {
                yield break;
            }
        }

        if (practiceDemoManager != null)
        {
            practiceDemoManager.Stop();
        }

        if (practicePanel != null)
        {
            practicePanel.SetActive(false);
        }

        if (modeButton != null)
        {
            modeButton.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.SetActive(false);
        }
    }

    private IEnumerator CreateMinigame()
    {
        if (rhythmManager == null)
        {
            Debug.LogError(
                "[Practice] RhythmManager가 없습니다."
            );

            isPracticing = false;
            yield break;
        }

        if (chartCsv == null)
        {
            Debug.LogError(
                "[Practice] 전체 리듬 CSV가 없습니다."
            );

            isPracticing = false;
            yield break;
        }

        string planetFolderName =
            GetPlanetFolderName(
                selectedPlanet
            );

        if (string.IsNullOrEmpty(
                planetFolderName))
        {
            Debug.LogError(
                $"[Practice] 알 수 없는 행성 번호: " +
                $"{selectedPlanet}"
            );

            isPracticing = false;
            yield break;
        }

        int minigameNumber =
            CurrentMinigameId;

        string resourcePath =
            $"MinigamePrefab/{planetFolderName}/" +
            $"{selectedPlanet}_{minigameNumber}" +
            "minigame_remake";

        GameObject prefab =
            Resources.Load<GameObject>(
                resourcePath
            );

        if (prefab == null)
        {
            Debug.LogError(
                $"[Practice] 프리팹을 찾지 못했습니다.\n" +
                $"Resources/{resourcePath}"
            );

            isPracticing = false;
            yield break;
        }

        ResetCamera();

        currentMinigameObject =
            Instantiate(prefab);

        currentMinigame =
            currentMinigameObject
                .GetComponent<MiniGameBase>();

        if (currentMinigame == null)
        {
            Debug.LogError(
                "[Practice] MiniGameBase가 없습니다: " +
                resourcePath
            );

            Destroy(currentMinigameObject);

            currentMinigameObject = null;
            isPracticing = false;

            yield break;
        }

        string minigameId =
            $"{selectedPlanet}-{minigameNumber}";

        rhythmManager.ClearCurrent();

        var configureTask =
            rhythmManager.ConfigureForMinigameAsync(
                currentMinigame,
                minigameId,
                chartCsv
            );

        while (!configureTask.IsCompleted)
            yield return null;

        if (configureTask.IsFaulted)
        {
            Debug.LogError(
                $"[Practice] 리듬 차트 로드 실패\n" +
                $"ID: {minigameId}\n" +
                $"{configureTask.Exception}"
            );

            DestroyCurrentMinigame();

            isPracticing = false;
            yield break;
        }

        if (configureTask.IsCanceled)
        {
            Debug.LogWarning(
                "[Practice] 리듬 차트 로드 취소: " +
                minigameId
            );

            DestroyCurrentMinigame();

            isPracticing = false;
            yield break;
        }

        Debug.Log(
            $"[Practice] 미니게임 준비 완료\n" +
            $"ID: {minigameId}\n" +
            $"Prefab: {resourcePath}"
        );

        yield return null;
    }

    private IEnumerator ShowTitlePhase()
    {
        currentPhase =
            PracticePhase.Title;

        titleConfirmed = false;

        if (blockInputPanel != null)
        {
            blockInputPanel.SetActive(true);
        }

        if (practicePanel != null)
        {
            practicePanel.SetActive(false);
        }

        if (modeButton != null)
        {
            modeButton.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.SetActive(false);
        }

        if (titlePanel != null)
        {
            titlePanel.SetActive(true);
        }

        if (titleCanvasGroup != null)
        {
            titleCanvasGroup.alpha = 1f;
        }

        if (titleText != null &&
            currentMinigame != null)
        {
            titleText.gameObject.SetActive(true);

            titleText.alpha = 1f;

            titleText.text =
                $"{selectedPlanet}-" +
                $"{CurrentMinigameId} " +
                $"{currentMinigame.GetMinigameTitle}";
        }

        if (transitionPanel != null &&
            transitionPanel.activeSelf)
        {
            yield return
                FadeTransitionTo(0f);
        }

        while (isPracticing &&
       !titleConfirmed)
        {
            bool clicked =
                Input.GetMouseButtonDown(0);

            bool touched =
                Input.touchCount > 0 &&
                Input.GetTouch(0).phase ==
                TouchPhase.Began;

            if (clicked || touched)
            {
                titleConfirmed = true;
                break;
            }

            yield return null;
        }

        if (!isPracticing)
            yield break;

        yield return FadeTitleOut();

        if (titlePanel != null)
        {
            titlePanel.SetActive(false);
        }

        if (titleCanvasGroup != null)
        {
            // 다음 미니게임을 위해 복구.
            titleCanvasGroup.alpha = 1f;
        }
    }
    private IEnumerator FadeTitleOut()
    {
        if (titleCanvasGroup == null)
            yield break;

        if (titleFadeDuration <= 0f)
        {
            titleCanvasGroup.alpha = 0f;
            yield break;
        }

        float startAlpha =
            titleCanvasGroup.alpha;

        float elapsed = 0f;

        while (elapsed <
               titleFadeDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    titleFadeDuration
                );

            titleCanvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    0f,
                    t
                );

            yield return null;
        }

        titleCanvasGroup.alpha = 0f;
    }
    private IEnumerator FadeTransitionTo(
    float targetAlpha)
    {
        if (transitionPanel == null ||
            transitionCanvasGroup == null)
        {
            yield break;
        }

        transitionPanel.SetActive(true);

        if (transitionFadeDuration <= 0f)
        {
            transitionCanvasGroup.alpha =
                targetAlpha;

            if (targetAlpha <= 0f)
            {
                transitionPanel.SetActive(false);
            }

            yield break;
        }

        float startAlpha =
            transitionCanvasGroup.alpha;

        float elapsed = 0f;

        while (elapsed <
               transitionFadeDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    transitionFadeDuration
                );

            transitionCanvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    t
                );

            yield return null;
        }

        transitionCanvasGroup.alpha =
            targetAlpha;

        if (targetAlpha <= 0f)
        {
            transitionPanel.SetActive(false);
        }
    }
    private void UpdatePracticeUI()
    {
        if (practicePanel != null)
        {
            practicePanel.SetActive(true);
        }

        if (modeButton != null)
        {
            modeButton.SetActive(true);
        }

        if (nextButton != null)
        {
            nextButton.SetActive(true);
        }

        if (modeButtonText == null)
            return;

        if (playMode ==
            PracticePlayMode.Demo)
        {
            modeButtonText.text =
                "직접 해보기";
        }
        else
        {
            modeButtonText.text =
                "시범 다시 보기";
        }
    }

    public void ConfirmTitle()
    {
        if (currentPhase !=
            PracticePhase.Title)
        {
            return;
        }

        titleConfirmed = true;
    }

    public void TogglePracticeMode()
    {
        if (currentPhase !=
            PracticePhase.Practice)
        {
            return;
        }

        if (playMode ==
            PracticePlayMode.Demo)
        {
            playMode =
                PracticePlayMode.Player;
        }
        else
        {
            playMode =
                PracticePlayMode.Demo;
        }

        modeChangeRequested = true;
    }

    public void NextMinigame()
    {
        Debug.Log("[Practice] NEXT 클릭");

        if (currentPhase !=
            PracticePhase.Practice)
        {
            return;
        }

        if (GameRoot.Instance == null ||
            GameRoot.Instance.Confirm == null)
        {
            Debug.LogError(
                "[Practice] ConfirmManager가 없습니다."
            );

            return;
        }

        bool isLastMinigame =
            currentTrackIndex >=
            trackMinigames.Count - 1;

        if (isLastMinigame)
        {
            GameRoot.Instance.Confirm.Show(
                "훈련을 종료하시겠습니까?",
                onYes: ExitPractice
            );

            return;
        }

        GameRoot.Instance.Confirm.Show(
            "다음 미니게임으로 넘어가시겠습니까?",
            onYes: ConfirmNextMinigame
        );
    }

    private void ConfirmNextMinigame()
    {
        nextRequested = true;
    }

    private string GetPlanetFolderName(
        int planetNumber)
    {
        switch (planetNumber)
        {
            case 1:
                return "PolicePlanet";

            case 2:
                return "CandyPlanet";

            case 3:
                return "MafiaPlanet";

            case 4:
                return "MusicPlanet";

            default:
                return null;
        }
    }

    private void DestroyCurrentMinigame()
    {
        if (practiceDemoManager != null)
        {
            practiceDemoManager.Stop();
        }

        if (rhythmManager != null)
        {
            rhythmManager.ClearCurrent();
        }

        if (currentMinigameObject != null)
        {
            Destroy(currentMinigameObject);
        }

        currentMinigame = null;
        currentMinigameObject = null;
    }

    private void ResetCamera()
    {
        if (mainCameraTransform == null)
            return;

        mainCameraTransform.position =
            initialCameraPosition;

        mainCameraTransform.rotation =
            initialCameraRotation;
    }

    public void ExitPractice()
    {
        isPracticing = false;

        if (practiceCoroutine != null)
        {
            StopCoroutine(practiceCoroutine);
            practiceCoroutine = null;
        }

        DestroyCurrentMinigame();
        ResetCamera();

        ReturnToExitScene();
    }

    private void ReturnToExitScene()
    {
        string exitScene =
            fallbackExitSceneName;

        if (GameRoot.Instance != null &&
            GameRoot.Instance.Session != null)
        {
            string sessionReturnScene =
                GameRoot.Instance.Session.Data
                    .returnSceneName;

            if (!string.IsNullOrWhiteSpace(
                    sessionReturnScene))
            {
                exitScene =
                    sessionReturnScene;
            }

            GameRoot.Instance.Session.Clear();
        }

        if (string.IsNullOrWhiteSpace(
                exitScene))
        {
            Debug.LogWarning(
                "[Practice] 나갈 씬 이름이 없습니다."
            );

            return;
        }

        if (GameRoot.Instance != null &&
            GameRoot.Instance.SceneFlow != null)
        {
            GameRoot.Instance.SceneFlow.LoadScene(
                exitScene
            );

            return;
        }

        SceneManager.LoadScene(
            exitScene
        );
    }
    public void UnlockGuideText(
    int guideIndex)
    {
        if (currentPhase !=
            PracticePhase.Practice)
        {
            return;
        }

        if (guideTextController == null)
            return;

        guideTextController.UnlockGuide(
            guideIndex
        );
    }

    private void OnDestroy()
    {
        isPracticing = false;

        if (rhythmManager != null)
            rhythmManager.ClearCurrent();
    }
}