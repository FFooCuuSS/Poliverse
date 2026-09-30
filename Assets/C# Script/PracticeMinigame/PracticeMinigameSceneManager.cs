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

    [Header("Next / Exit 확인 UI")]
    [SerializeField] private GameObject nextConfirmPanel;
    [SerializeField] private TMP_Text nextConfirmText;

    [Header("Button Sprite Feedback")]

    [Tooltip("현재 모드 버튼 클릭 Sprite")]
    [SerializeField] private Sprite modeButtonPressedSprite;

    [Tooltip("다음 버튼 클릭 Sprite")]
    [SerializeField] private Sprite nextButtonPressedSprite;

    [Tooltip("네 버튼 클릭 Sprite")]
    [SerializeField] private Sprite yesButtonPressedSprite;

    [Tooltip("아니요 버튼 클릭 Sprite")]
    [SerializeField] private Sprite noButtonPressedSprite;

    [SerializeField, Min(0f)]
    private float buttonPressedDuration = 0.12f;

    [Header("Button References")]
    [SerializeField] private GameObject yesButton;
    [SerializeField] private GameObject noButton;

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

        if (nextConfirmPanel != null)
        {
            nextConfirmPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // 기본값
        selectedPlanet = 1;
        selectedTrack = 1;

        bool hasPracticeSelection = false;


        // 로비에서 PlayerPrefs로 전달받은 정보 읽기
        if (PlayerPrefs.HasKey("PracticePlanetId") &&
            PlayerPrefs.HasKey("PracticeTrackId"))
        {
            selectedPlanet =
                PlayerPrefs.GetInt(
                    "PracticePlanetId",
                    1
                );

            selectedTrack =
                PlayerPrefs.GetInt(
                    "PracticeTrackId",
                    1
                );

            hasPracticeSelection = true;
        }


        // 전달받은 값 확인
        if (!hasPracticeSelection)
        {
            Debug.LogWarning(
                "[Practice] 전달된 훈련 트랙이 없습니다. " +
                "테스트용 1-1 훈련을 실행합니다."
            );
        }
        else
        {
            Debug.Log(
                $"[Practice] 훈련 선택 정보 받음 - " +
                $"Planet={selectedPlanet}, " +
                $"Track={selectedTrack}"
            );
        }


        // 행성 번호 확인
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


        // 행성별 훈련 개수

        // 1번 행성 = 훈련 4개
        // 나머지 행성 = 훈련 5개
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


        // 선택한 훈련의 미니게임 목록 가져오기
        trackMinigames =
            PracticeTrackCatalog.GetMinigames(
                selectedPlanet,
                selectedTrack
            );


        if (trackMinigames == null ||
            trackMinigames.Count == 0)
        {
            Debug.LogError(
                "[Practice] 훈련 트랙에 " +
                "미니게임이 없습니다."
            );

            ReturnToExitScene();
            return;
        }


        Debug.Log(
            $"[Practice] 실행할 훈련: " +
            $"{selectedPlanet}-{selectedTrack}"
        );

        Debug.Log(
            $"[Practice] 미니게임 목록: " +
            string.Join(
                ", ",
                trackMinigames
            )
        );


        currentTrackIndex = 0;


        // RhythmManager 확인
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


        // Audio
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


        // 연습 시작
        isPracticing = true;

        practiceCoroutine =
            StartCoroutine(
                PracticeLoop()
            );
    }

    private IEnumerator PracticeLoop()
    {
        for (currentTrackIndex = 0;
             currentTrackIndex < trackMinigames.Count;
             currentTrackIndex++)
        {
            if (!isPracticing)
                yield break;

            yield return CreateMinigame();

            if (!isPracticing || currentMinigame == null)
            {
                yield break;
            }

            yield return ShowTitlePhase();

            if (!isPracticing)
                yield break;

            playMode = PracticePlayMode.Demo;

            yield return PracticeCurrentMinigame();

            if (!isPracticing)
                yield break;

            currentPhase = PracticePhase.Transition;

            yield return FadeTransitionTo(1f);

            DestroyCurrentMinigame();
            ResetCamera();

            yield return null;
        }

        currentPhase = PracticePhase.Finished;
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

            if (blockInputPanel != null)
            {
                blockInputPanel.SetActive(isDemo);
            }

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
                이제 모든 준비(입력 차단 및 데모 매니저 바인딩)가 끝난 후
                미니게임을 시작합니다.
            */
            IPracticeDemoInput demoInput =
                currentMinigame as IPracticeDemoInput;

            if (demoInput != null)
            {
                demoInput.SetDemoMode(isDemo);
            }

            currentMinigame.StartGame();

            // 데모 매니저 시작을 위한 한 프레임 대기
            yield return null;

            /*
               DemoManager를 먼저 연결한 뒤
               타임라인을 시작한다.
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

        //if (GameRoot.Instance == null ||
        //    GameRoot.Instance.Confirm == null)
        //{
        //    Debug.LogError(
        //        "[Practice] ConfirmManager가 없습니다."
        //    );

        //    return;
        //}

        if (currentPhase != PracticePhase.Practice)
        {
            Debug.LogWarning(
                "[Practice] 현재 Practice 상태가 아닙니다."
            );

            return;
        }

        bool isLastMinigame =
            currentTrackIndex >=
            trackMinigames.Count - 1;

        if (nextConfirmText != null)
        {
            if (isLastMinigame)
            {
                nextConfirmText.text =
                    "훈련을 종료하시겠습니까?";
            }
            else
            {
                nextConfirmText.text =
                    "다음 미니게임으로 넘어가시겠습니까?";
            }
        }

        if (nextConfirmPanel != null)
        {
            nextConfirmPanel.SetActive(true);

            Debug.Log("[Practice] 확인 패널을 열었습니다.");
        }
        else
        {
            Debug.LogError(
                "[Practice] nextConfirmPanel이 연결되지 않았습니다!"
            );
        }

        /*
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
            */
    }

    public void ConfirmNextOrExit()
    {
        if (nextConfirmPanel != null)
        {
            nextConfirmPanel.SetActive(false);
        }

        // 마지막 미니게임인지 다시 확인
        bool isLastMinigame =
            currentTrackIndex >= trackMinigames.Count - 1;

        if (isLastMinigame)
        {
            // 마지막이면 훈련 종료
            ExitPractice();
        }
        else
        {
            // 마지막이 아니면 다음 미니게임
            nextRequested = true;
        }
    }
    public void CancelNextOrExit()
    {
        if (nextConfirmPanel != null)
        {
            nextConfirmPanel.SetActive(false);
        }
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

        string savedReturnScene =
            PlayerPrefs.GetString(
            "PracticeReturnScene",
            ""
        );

        if (!string.IsNullOrWhiteSpace(savedReturnScene))
        {
            exitScene = savedReturnScene;
        }


        // GameRoot가 있다면 기존 Session도 정리
        if (GameRoot.Instance != null &&
            GameRoot.Instance.Session != null)
        {
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

    public void OnModeButtonClick()
    {
        ChangeButtonSprite(
            modeButton,
            modeButtonPressedSprite
        );

        TogglePracticeMode();
    }

    public void OnNextButtonClick()
    {
        ChangeButtonSprite(
            nextButton,
            nextButtonPressedSprite
        );

        NextMinigame();
    }

    public void OnYesButtonClick()
    {
        ChangeButtonSprite(
            yesButton,
            yesButtonPressedSprite
        );

        ConfirmNextOrExit();
    }

    public void OnNoButtonClick()
    {
        ChangeButtonSprite(
            noButton,
            noButtonPressedSprite
        );

        CancelNextOrExit();
    }

    private void ChangeButtonSprite(
    GameObject buttonObject,
    Sprite pressedSprite)
    {
        if (buttonObject == null)
            return;

        if (pressedSprite == null)
            return;

        UnityEngine.UI.Image buttonImage =
            buttonObject.GetComponent<UnityEngine.UI.Image>();

        if (buttonImage == null)
            return;

        // 현재 버튼에 설정되어 있는 기본 Sprite 저장
        Sprite originalSprite =
            buttonImage.sprite;

        // 클릭 Sprite로 변경
        buttonImage.sprite =
            pressedSprite;

        StartCoroutine(
            RestoreButtonSprite(
                buttonImage,
                originalSprite
            )
        );
    }

    private IEnumerator RestoreButtonSprite(
        UnityEngine.UI.Image buttonImage,
        Sprite originalSprite)
    {
        yield return new WaitForSecondsRealtime(
            buttonPressedDuration
        );

        if (buttonImage != null)
        {
            buttonImage.sprite =
                originalSprite;
        }
    }
}