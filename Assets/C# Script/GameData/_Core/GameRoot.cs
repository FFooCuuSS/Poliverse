using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance
    {
        get;
        private set;
    }

    [Header("Managers")]
    [SerializeField]
    private GameSessionManager sessionManager;

    [SerializeField]
    private GameAccountManager accountManager;

    [SerializeField]
    private GameSettingsManager settingsManager;

    [SerializeField]
    private GameSaveManager saveManager;

    [SerializeField]
    private AudioManager audioManager;

    [SerializeField]
    private GameSceneFlowManager sceneFlowManager;

    [Header("Global UI")]
    [SerializeField]
    private GameConfirmDialog confirmDialog;

    public GameSessionManager Session =>
        sessionManager;

    public GameAccountManager Account =>
        accountManager;

    public GameSettingsManager Settings =>
        settingsManager;

    public GameSaveManager Save =>
        saveManager;

    public AudioManager Audio =>
        audioManager;

    public GameSceneFlowManager SceneFlow =>
        sceneFlowManager;

    public GameConfirmDialog Confirm =>
        confirmDialog;

    public bool IsInitializing
    {
        get;
        private set;
    }

    public bool IsReady
    {
        get;
        private set;
    }

    public bool InitializationFailed
    {
        get;
        private set;
    }

    public event Action OnReady;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(
            gameObject
        );

        FindMissingReferences();
    }

    private IEnumerator Start()
    {
        yield return
            InitializeRoutine();
    }

    private void FindMissingReferences()
    {
        if (sessionManager == null)
        {
            sessionManager =
                GetComponentInChildren
                    <GameSessionManager>(
                        true
                    );
        }

        if (accountManager == null)
        {
            accountManager =
                GetComponentInChildren
                    <GameAccountManager>(
                        true
                    );
        }

        if (settingsManager == null)
        {
            settingsManager =
                GetComponentInChildren
                    <GameSettingsManager>(
                        true
                    );
        }

        if (saveManager == null)
        {
            saveManager =
                GetComponentInChildren
                    <GameSaveManager>(
                        true
                    );
        }

        if (audioManager == null)
        {
            audioManager =
                GetComponentInChildren
                    <AudioManager>(
                        true
                    );
        }

        if (sceneFlowManager == null)
        {
            sceneFlowManager =
                GetComponentInChildren
                    <GameSceneFlowManager>(
                        true
                    );
        }

        if (confirmDialog == null)
        {
            confirmDialog =
                GetComponentInChildren
                    <GameConfirmDialog>(
                        true
                    );
        }
    }

    private bool ValidateRequiredReferences()
    {
        bool valid = true;

        if (sessionManager == null)
        {
            Debug.LogError(
                "[GameRoot] SessionManager가 없습니다."
            );

            valid = false;
        }

        if (accountManager == null)
        {
            Debug.LogError(
                "[GameRoot] AccountManager가 없습니다."
            );

            valid = false;
        }

        if (settingsManager == null)
        {
            Debug.LogError(
                "[GameRoot] SettingsManager가 없습니다."
            );

            valid = false;
        }

        if (saveManager == null)
        {
            Debug.LogError(
                "[GameRoot] SaveManager가 없습니다."
            );

            valid = false;
        }

        if (audioManager == null)
        {
            Debug.LogError(
                "[GameRoot] AudioManager가 없습니다."
            );

            valid = false;
        }

        if (sceneFlowManager == null)
        {
            Debug.LogError(
                "[GameRoot] SceneFlowManager가 없습니다."
            );

            valid = false;
        }

        return valid;
    }

    private IEnumerator InitializeRoutine()
    {
        if (IsInitializing ||
            IsReady)
        {
            yield break;
        }

        IsInitializing = true;
        InitializationFailed = false;

        if (!ValidateRequiredReferences())
        {
            FailInitialization();
            yield break;
        }

        // 1. 기본 로컬 시스템
        try
        {
            sessionManager.Initialize();

            settingsManager.Initialize();

            accountManager
                .InitializeLocalOnly();

            saveManager
                .SetAccountManager(
                    accountManager
                );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[GameRoot] 기본 시스템 초기화 실패\n" +
                $"{exception}"
            );

            FailInitialization();
            yield break;
        }

        // 2. Firebase SDK 초기화
        Task<bool> firebaseTask =
            accountManager
                .InitializeFirebaseAsync();

        while (!firebaseTask.IsCompleted)
            yield return null;

        bool firebaseAvailable =
            false;

        if (firebaseTask.IsFaulted)
        {
            Debug.LogError(
                $"[GameRoot] Firebase 초기화 예외\n" +
                $"{firebaseTask.Exception}"
            );
        }
        else if (firebaseTask.IsCanceled)
        {
            Debug.LogWarning(
                "[GameRoot] Firebase 초기화 취소."
            );
        }
        else
        {
            firebaseAvailable =
                firebaseTask.Result;
        }

        // 3. Firebase 계정 확보
        //
        // 기존 로그인 세션:
        // 그대로 사용
        //
        // 로그인 세션 없음:
        // Firebase Anonymous Guest 생성
        if (firebaseAvailable)
        {
            Task<bool> signInTask =
                accountManager
                    .EnsureSignedInAsync();

            while (!signInTask.IsCompleted)
                yield return null;

            if (signInTask.IsFaulted)
            {
                Debug.LogError(
                    $"[GameRoot] Firebase 계정 확보 실패\n" +
                    $"{signInTask.Exception}"
                );
            }
            else if (signInTask.IsCanceled)
            {
                Debug.LogWarning(
                    "[GameRoot] Firebase 계정 확보 취소."
                );
            }
            else if (signInTask.Result)
            {
                Debug.Log(
                    $"[GameRoot] Firebase 계정 준비 완료 | " +
                    $"UID: {accountManager.Data.uid} | " +
                    $"Guest: " +
                    $"{accountManager.Data.isAnonymous}"
                );
            }
            else
            {
                Debug.LogWarning(
                    "[GameRoot] Firebase 계정을 만들 수 없습니다. " +
                    "Local 저장으로 계속합니다."
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "[GameRoot] Firebase 사용 불가. " +
                "Local 저장으로 계속합니다."
            );
        }

        // 4. Save 초기화
        //
        // Firebase UID 있음:
        // Local + Firestore
        //
        // Firebase UID 없음:
        // Local Only
        Task saveInitializeTask =
            saveManager
                .InitializeAsync();

        while (!saveInitializeTask.IsCompleted)
            yield return null;

        if (saveInitializeTask.IsFaulted)
        {
            Debug.LogError(
                $"[GameRoot] SaveManager 초기화 실패\n" +
                $"{saveInitializeTask.Exception}"
            );

            FailInitialization();
            yield break;
        }

        if (saveInitializeTask.IsCanceled)
        {
            Debug.LogError(
                "[GameRoot] SaveManager 초기화가 취소되었습니다."
            );

            FailInitialization();
            yield break;
        }

        // 5. 후속 시스템
        try
        {
            sceneFlowManager.Initialize();

            audioManager.Initialize(
                settingsManager
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[GameRoot] 후속 시스템 초기화 실패\n" +
                $"{exception}"
            );

            FailInitialization();
            yield break;
        }

        IsReady = true;
        IsInitializing = false;

        OnReady?.Invoke();

        Debug.Log(
            "[GameRoot] 모든 시스템 초기화 완료"
        );
    }

    private void FailInitialization()
    {
        InitializationFailed = true;
        IsInitializing = false;
        IsReady = false;
    }

    private void SaveAll()
    {
        settingsManager?.SaveNow();

        saveManager?.RequestSave();
    }

    private void OnApplicationPause(
        bool paused)
    {
        if (paused)
            SaveAll();
    }

    private void OnApplicationQuit()
    {
        SaveAll();
    }
}