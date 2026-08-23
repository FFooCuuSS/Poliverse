using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneFlowManager : MonoBehaviour
{
    public event Action<bool> OnLoadingStateChanged;

    [Header("Loading")]
    [SerializeField]
    private GameLoadingOverlay loadingOverlay;

    [Tooltip("로딩 내용이 완전히 표시된 후 유지할 최소 시간")]
    [SerializeField, Min(0f)]
    private float minimumLoadingDuration = 3f;

    [SerializeField]
    private PracticeDemoManager practiceDemoManager;
    public bool IsLoading { get; private set; }

    private void Awake()
    {
        FindLoadingOverlay();

        if (practiceDemoManager == null)
        {
            practiceDemoManager =
                GetComponent<PracticeDemoManager>();
        }

        if (practiceDemoManager == null)
        {
            practiceDemoManager =
                gameObject.AddComponent<
                    PracticeDemoManager>();
        }
    }

    public void Initialize()
    {
        IsLoading = false;
        FindLoadingOverlay();
    }

    public void LoadScene(string sceneName)
    {
        if (IsLoading)
        {
            Debug.LogWarning(
                "[SceneFlow] 이미 씬을 로드 중입니다."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError(
                "[SceneFlow] 씬 이름이 비어 있습니다."
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"[SceneFlow] Build Settings에서 씬을 찾을 수 없습니다: " +
                $"{sceneName}"
            );

            return;
        }

        FindLoadingOverlay();

        if (loadingOverlay == null)
        {
            Debug.LogError(
                "[SceneFlow] GameLoadingOverlay가 없습니다. " +
                "GameRoot의 LoadingCanvas를 확인하세요."
            );

            return;
        }

        StartCoroutine(
            LoadSceneRoutine(sceneName)
        );
    }

    private IEnumerator LoadSceneRoutine(
        string sceneName)
    {
        IsLoading = true;
        OnLoadingStateChanged?.Invoke(true);

        Time.timeScale = 1f;

        // 로딩 캔버스를 활성화하고
        // 캔버스 페이드인을 시작한다.
        loadingOverlay.Show();

        AsyncOperation operation = null;

        try
        {
            operation = SceneManager.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Single
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[SceneFlow] 씬 로드 요청 실패\n" +
                $"Scene: {sceneName}\n" +
                $"{exception}"
            );
        }

        if (operation == null)
        {
            loadingOverlay.HideImmediate();

            IsLoading = false;
            OnLoadingStateChanged?.Invoke(false);

            yield break;
        }

        // 자동으로 새 씬이 활성화되지 않도록 막는다.
        operation.allowSceneActivation = false;

        // 로딩창 등장 애니메이션이 끝날 때까지 기다린다.
        // 씬 비동기 로딩은 이 시간에도 진행된다.
        yield return loadingOverlay.WaitForEntrance();

        // 실제 씬 로딩과 최소 표시 시간을 모두 기다린다.
        while (operation.progress < 0.9f ||
               loadingOverlay.ContentVisibleDuration <
               minimumLoadingDuration)
        {
            float normalizedProgress =
                Mathf.Clamp01(operation.progress / 0.9f);

            loadingOverlay.SetProgress(
                normalizedProgress
            );

            yield return null;
        }

        loadingOverlay.SetProgress(1f);

        // 로딩 내용 위로 검은 FadePanel을 다시 올린다.
        yield return loadingOverlay.FadeToBlack();

        // 검은 화면인 상태에서 새 씬을 활성화한다.
        operation.allowSceneActivation = true;

        while (!operation.isDone)
            yield return null;

        // 새 씬의 카메라와 UI가 한 프레임 초기화될 시간을 준다.
        yield return null;

        // 검은 로딩 캔버스 전체를 서서히 지운다.
        yield return loadingOverlay.RevealSceneAndHide();

        IsLoading = false;
        OnLoadingStateChanged?.Invoke(false);
    }

    private void FindLoadingOverlay()
    {
        if (loadingOverlay != null)
            return;

        Transform root = transform.root;

        if (root != null)
        {
            loadingOverlay =
                root.GetComponentInChildren
                    <GameLoadingOverlay>(true);
        }
    }
}