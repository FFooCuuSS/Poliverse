using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PracticeMinigameSceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private RhythmManager rhythmManager;

    [Tooltip(
        "로딩 및 마디 대기 중 입력을 막는 패널"
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

    [Header("Repeat Timing")]
    [Tooltip(
        "한 마디의 길이(초). " +
        "예: 마디 경계가 0, 8, 16초라면 8"
    )]
    [SerializeField, Min(0.01f)]
    private float measureDuration = 8f;

    [Tooltip(
        "프레임 오차로 마디 경계를 조금 넘었을 때 " +
        "즉시 반복할 허용 범위"
    )]
    [SerializeField, Range(0f, 0.25f)]
    private float measureBoundaryTolerance = 0.05f;

    [Header("Exit")]
    [Tooltip(
        "Session에 돌아갈 씬이 없을 경우 사용할 씬"
    )]
    [SerializeField]
    private string fallbackExitSceneName =
        "LobbyScene";

    private MiniGameBase currentMinigame;
    private GameObject currentMinigameObject;

    private Coroutine practiceCoroutine;
    private bool isPracticing;

    private int selectedPlanet;
    private int selectedMinigame;

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
    }

    private void Start()
    {
        if (GameRoot.Instance == null)
        {
            Debug.LogError(
                "[Practice] GameRoot가 없습니다. " +
                "BootStrapScene부터 실행해야 합니다."
            );

            ReturnToExitScene();
            return;
        }

        GameSessionManager sessionManager =
            GameRoot.Instance.Session;

        if (sessionManager == null ||
            sessionManager.Data == null)
        {
            Debug.LogError(
                "[Practice] GameSessionManager가 " +
                "준비되지 않았습니다."
            );

            ReturnToExitScene();
            return;
        }

        GameSessionData session =
            sessionManager.Data;

        if (session.gameMode != GameMode.Practice)
        {
            Debug.LogError(
                "[Practice] 연습 모드 선택 정보가 없습니다."
            );

            ReturnToExitScene();
            return;
        }

        selectedPlanet =
            session.selectedPlanetId;

        selectedMinigame =
            session.selectedMinigameId;

        int maxMinigameCount =
            selectedPlanet == 1 ? 10 : 15;

        if (selectedPlanet < 1 ||
            selectedPlanet > 4 ||
            selectedMinigame < 1 ||
            selectedMinigame > maxMinigameCount)
        {
            Debug.LogError(
                $"[Practice] 잘못된 선택값: " +
                $"{selectedPlanet}-{selectedMinigame}"
            );

            ReturnToExitScene();
            return;
        }

        if (measureDuration <= 0f)
        {
            Debug.LogError(
                "[Practice] Measure Duration은 " +
                "0보다 커야 합니다."
            );

            ReturnToExitScene();
            return;
        }

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

        // 로비에서 재생 중이던 전역 BGM을 정지한다.
        if (GameRoot.Instance.Audio != null)
        {
            GameRoot.Instance.Audio.StopBgm();
        }

        // 연습 씬의 RhythmManager가
        // 연습용 음악을 직접 재생하게 한다.
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
        while (isPracticing)
        {
            if (blockInputPanel != null)
                blockInputPanel.SetActive(true);

            yield return CreateAndStartMinigame();

            if (!isPracticing ||
                currentMinigame == null ||
                currentMinigameObject == null)
            {
                practiceCoroutine = null;
                yield break;
            }

            // CSV의 마지막 노드가 실행될 때까지 기다린다.
            yield return new WaitUntil(() =>
                !isPracticing ||
                rhythmManager == null ||
                rhythmManager.HasDispatchedAllEvents
            );

            if (!isPracticing)
            {
                practiceCoroutine = null;
                yield break;
            }

            if (blockInputPanel != null)
                blockInputPanel.SetActive(true);

            // 마지막 노드 이후 다음 마디 경계까지 기다린다.
            yield return
                WaitUntilNextMeasureBoundary();

            if (!isPracticing)
            {
                practiceCoroutine = null;
                yield break;
            }

            DestroyCurrentMinigame();
            ResetCamera();

            // Destroy가 반영되는 프레임을 기다린다.
            yield return null;
        }

        practiceCoroutine = null;
    }

    private IEnumerator CreateAndStartMinigame()
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
            GetPlanetFolderName(selectedPlanet);

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

        string resourcePath =
            $"MinigamePrefab/{planetFolderName}/" +
            $"{selectedPlanet}_{selectedMinigame}" +
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

        // 프리팹에 저장된 원래 Transform으로 생성한다.
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

        // 예: 3번 행성의 12번 미니게임이면 "3-12"
        string minigameId =
            $"{selectedPlanet}-{selectedMinigame}";

        rhythmManager.ClearCurrent();

        // 전체 CSV 하나와 선택한 미니게임 ID를 전달한다.
        // RhythmManager가 3-12_time, 3-12_type 행을 찾는다.
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

        currentMinigame.StartGame();

        yield return null;

        if (!isPracticing ||
            currentMinigame == null)
        {
            yield break;
        }

        if (blockInputPanel != null)
            blockInputPanel.SetActive(false);

        rhythmManager.StartSong();

        Debug.Log(
            $"[Practice] 반복 시작\n" +
            $"ID: {minigameId}\n" +
            $"Prefab: {resourcePath}\n" +
            $"Measure: {measureDuration:F3}초"
        );
    }

    private IEnumerator
        WaitUntilNextMeasureBoundary()
    {
        if (rhythmManager == null)
            yield break;

        double currentSongTime =
            rhythmManager.SongTime;

        double measure =
            measureDuration;

        double remainder =
            currentSongTime % measure;

        double waitDuration =
            measure - remainder;

        bool nearPreviousBoundary =
            remainder <=
            measureBoundaryTolerance;

        bool nearNextBoundary =
            waitDuration <=
            measureBoundaryTolerance;

        if (nearPreviousBoundary ||
            nearNextBoundary)
        {
            waitDuration = 0.0;
        }

        double targetSongTime =
            currentSongTime + waitDuration;

        Debug.Log(
            $"[Practice] 마지막 노드: " +
            $"{currentSongTime:F3}초\n" +
            $"다음 마디 경계: " +
            $"{targetSongTime:F3}초\n" +
            $"대기시간: {waitDuration:F3}초"
        );

        while (isPracticing &&
               rhythmManager != null &&
               rhythmManager.IsRunning &&
               rhythmManager.SongTime <
               targetSongTime)
        {
            yield return null;
        }
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
        if (rhythmManager != null)
            rhythmManager.ClearCurrent();

        if (currentMinigameObject != null)
            Destroy(currentMinigameObject);

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

    /// <summary>
    /// 나가기 버튼 OnClick에 연결한다.
    /// </summary>
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

        if (string.IsNullOrWhiteSpace(exitScene))
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

        SceneManager.LoadScene(exitScene);
    }

    private void OnDestroy()
    {
        isPracticing = false;

        if (rhythmManager != null)
            rhythmManager.ClearCurrent();
    }
}