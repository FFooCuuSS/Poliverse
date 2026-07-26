using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance { get; private set; }

    [Header("Managers")]
    [SerializeField] private GameSessionManager sessionManager;
    [SerializeField] private GameAccountManager accountManager;
    [SerializeField] private GameSettingsManager settingsManager;
    [SerializeField] private GameSaveManager saveManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameSceneFlowManager sceneFlowManager;

    public GameSessionManager Session => sessionManager;
    public GameAccountManager Account => accountManager;
    public GameSettingsManager Settings => settingsManager;
    public GameSaveManager Save => saveManager;
    public AudioManager Audio => audioManager;
    public GameSceneFlowManager SceneFlow => sceneFlowManager;

    public bool IsInitializing { get; private set; }
    public bool IsReady { get; private set; }
    public bool InitializationFailed { get; private set; }

    public event Action OnReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        FindMissingReferences();
    }

    private IEnumerator Start()
    {
        yield return InitializeRoutine();
    }

    private void FindMissingReferences()
    {
        if (sessionManager == null)
        {
            sessionManager =
                GetComponentInChildren<GameSessionManager>(true);
        }

        if (accountManager == null)
        {
            accountManager =
                GetComponentInChildren<GameAccountManager>(true);
        }

        if (settingsManager == null)
        {
            settingsManager =
                GetComponentInChildren<GameSettingsManager>(true);
        }

        if (saveManager == null)
        {
            saveManager =
                GetComponentInChildren<GameSaveManager>(true);
        }

        if (audioManager == null)
        {
            audioManager =
                GetComponentInChildren<AudioManager>(true);
        }

        if (sceneFlowManager == null)
        {
            sceneFlowManager =
                GetComponentInChildren<GameSceneFlowManager>(true);
        }
    }

    private bool ValidateRequiredReferences()
    {
        bool valid = true;

        if (sessionManager == null)
        {
            Debug.LogError("[GameRoot] SessionManager가 없습니다.");
            valid = false;
        }

        if (accountManager == null)
        {
            Debug.LogError("[GameRoot] AccountManager가 없습니다.");
            valid = false;
        }

        if (settingsManager == null)
        {
            Debug.LogError("[GameRoot] SettingsManager가 없습니다.");
            valid = false;
        }

        if (saveManager == null)
        {
            Debug.LogError("[GameRoot] SaveManager가 없습니다.");
            valid = false;
        }

        if (audioManager == null)
        {
            Debug.LogError("[GameRoot] AudioManager가 없습니다.");
            valid = false;
        }

        if (sceneFlowManager == null)
        {
            Debug.LogError("[GameRoot] SceneFlowManager가 없습니다.");
            valid = false;
        }

        return valid;
    }

    private IEnumerator InitializeRoutine()
    {
        if (IsInitializing || IsReady)
            yield break;

        IsInitializing = true;
        InitializationFailed = false;

        if (!ValidateRequiredReferences())
        {
            InitializationFailed = true;
            IsInitializing = false;
            yield break;
        }

        try
        {
            sessionManager.Initialize();
            settingsManager.Initialize();
            accountManager.InitializeLocalOnly();

            // FIREBASE-LATER:
            // 여기서 Firebase 초기화 및 인증을 먼저 기다린다.
            //
            // Task authTask =
            //     accountManager.InitializeFirebaseAsync();
            //
            // while (!authTask.IsCompleted)
            //     yield return null;
            //
            // 인증 후 Firebase/Hybrid Repository를 생성해서
            // saveManager.SetRepository(repository)를 호출한다.

            sceneFlowManager.Initialize();
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[GameRoot] 동기 초기화 실패\n{exception}"
            );

            InitializationFailed = true;
            IsInitializing = false;

            yield break;
        }

        Task saveInitializeTask =
            saveManager.InitializeAsync();

        while (!saveInitializeTask.IsCompleted)
            yield return null;

        if (saveInitializeTask.IsFaulted)
        {
            Debug.LogError(
                $"[GameRoot] SaveManager 초기화 실패\n" +
                $"{saveInitializeTask.Exception}"
            );

            InitializationFailed = true;
            IsInitializing = false;

            yield break;
        }

        if (saveInitializeTask.IsCanceled)
        {
            Debug.LogError(
                "[GameRoot] SaveManager 초기화가 취소되었습니다."
            );

            InitializationFailed = true;
            IsInitializing = false;

            yield break;
        }

        audioManager.Initialize(settingsManager);

        IsReady = true;
        IsInitializing = false;

        OnReady?.Invoke();

        Debug.Log(
            "[GameRoot] 모든 로컬 시스템 초기화 완료"
        );
    }

    private void SaveAll()
    {
        settingsManager?.SaveNow();
        saveManager?.RequestSave();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveAll();
    }

    private void OnApplicationQuit()
    {
        SaveAll();

        // FIREBASE-LATER:
        // 앱 종료 시 네트워크 저장 완료를 보장하기 어렵다.
        // 따라서 변경 시 로컬 즉시 저장 +
        // 실행 중 클라우드 백그라운드 동기화를 사용한다.
    }
}