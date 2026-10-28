using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField]
    private string firstSceneName = "MainScene";

    // 필드는 항상 직렬화하고, 동작만 에디터/개발 빌드에서 켠다.
    [Header("Debug Jump (에디터/개발 빌드 전용)")]
    [Tooltip("켜면 MainScene/로비를 건너뛰고 바로 미니게임 씬으로 간다")]
    [SerializeField] private bool debugJumpEnabled = false;

    [Tooltip("1=경찰, 2=도넛, 3=마피아, 4=음악")]
    [SerializeField, Range(1, 4)] private int debugPlanetId = 1;

    [Tooltip("몇 번째 미니게임부터 시작할지 (1 = 1-1, 5 = 1-5)")]
    [SerializeField, Min(1)] private int debugMinigameNumber = 1;

    [SerializeField] private string minigameSceneName = "MinigameLoad";
    [SerializeField] private string returnSceneName = "LobbyScene";
    [SerializeField, Min(0)] private int debugStartingLife = 4;

    private IEnumerator Start()
    {
        while (GameRoot.Instance == null)
            yield return null;

        while (!GameRoot.Instance.IsReady &&
               !GameRoot.Instance.InitializationFailed)
        {
            yield return null;
        }

        if (GameRoot.Instance.InitializationFailed)
        {
            Debug.LogError(
                "[Bootstrap] GameRoot 초기화에 실패했습니다."
            );

            yield break;
        }

        string sceneToLoad = firstSceneName;

        bool canDebug = Application.isEditor || Debug.isDebugBuild;

        if (debugJumpEnabled && canDebug)
        {
            GameSessionManager session = GameRoot.Instance.Session;

            if (session != null)
            {
                session.StartPlanetRun(
                    debugPlanetId,
                    debugStartingLife,
                    returnSceneName
                );

                DebugJump.Request(debugMinigameNumber - 1);
                sceneToLoad = minigameSceneName;

                Debug.LogWarning(
                    $"[Bootstrap] DebugJump: {debugPlanetId}-{debugMinigameNumber}부터 " +
                    $"{minigameSceneName} 직행"
                );
            }
            else
            {
                Debug.LogError("[Bootstrap] DebugJump 실패: Session이 없습니다.");
            }
        }

        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogError(
                "[Bootstrap] First Scene Name이 비어 있습니다."
            );

            yield break;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError(
                $"[Bootstrap] Build Settings에서 씬을 찾을 수 없습니다: " +
                $"{sceneToLoad}"
            );

            yield break;
        }

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                sceneToLoad,
                LoadSceneMode.Single
            );

        if (operation == null)
        {
            Debug.LogError(
                $"[Bootstrap] 씬 로드 요청에 실패했습니다: " +
                $"{sceneToLoad}"
            );

            yield break;
        }

        while (!operation.isDone)
            yield return null;
    }
}