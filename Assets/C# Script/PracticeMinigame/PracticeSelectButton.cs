using UnityEngine;
using UnityEngine.SceneManagement;

public class PracticeSelectButton : MonoBehaviour
{
    [Header("Selection")]
    [SerializeField, Min(1)]
    private int planetId = 1;

    [SerializeField, Min(1)]
    private int trackId = 1;

    [Header("Scene")]
    [SerializeField]
    private string practiceSceneName = "PracticeMinigameScene";

    public void SelectPractice()
    {
        if (GameRoot.Instance == null)
        {
            Debug.LogError(
                "[PracticeSelect] GameRoot가 없습니다."
            );

            return;
        }

        if (GameRoot.Instance.Session == null)
        {
            Debug.LogError(
                "[PracticeSelect] GameSessionManager가 없습니다."
            );

            return;
        }

        string returnSceneName =
            SceneManager.GetActiveScene().name;

        GameRoot.Instance.Session.SelectPracticeTrack(
            planetId,
            trackId,
            returnSceneName
        );

        if (GameRoot.Instance.SceneFlow != null)
        {
            GameRoot.Instance.SceneFlow.LoadScene(
                practiceSceneName
            );
        }
        else
        {
            SceneManager.LoadScene(
                practiceSceneName
            );
        }
    }
}