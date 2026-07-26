using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField]
    private string firstSceneName = "MainScene";

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

        if (string.IsNullOrWhiteSpace(firstSceneName))
        {
            Debug.LogError(
                "[Bootstrap] First Scene Name이 비어 있습니다."
            );

            yield break;
        }

        if (!Application.CanStreamedLevelBeLoaded(firstSceneName))
        {
            Debug.LogError(
                $"[Bootstrap] Build Settings에서 씬을 찾을 수 없습니다: " +
                $"{firstSceneName}"
            );

            yield break;
        }

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                firstSceneName,
                LoadSceneMode.Single
            );

        if (operation == null)
        {
            Debug.LogError(
                $"[Bootstrap] 씬 로드 요청에 실패했습니다: " +
                $"{firstSceneName}"
            );

            yield break;
        }

        while (!operation.isDone)
            yield return null;
    }
}