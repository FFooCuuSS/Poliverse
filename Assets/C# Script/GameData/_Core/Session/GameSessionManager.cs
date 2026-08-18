using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    public GameSessionData Data { get; private set; }

    public bool IsInitialized => Data != null;

    public void Initialize()
    {
        Data = new GameSessionData();
    }

    public void SelectPracticeTrack(
        int planetId,
        int trackId,
        string returnSceneName)
    {
        EnsureInitialized();

        if (planetId <= 0 || trackId <= 0)
        {
            Debug.LogError(
                $"[Session] 잘못된 연습 트랙 선택값: " +
                $"{planetId}_{trackId}"
            );

            return;
        }

        Data.gameMode = GameMode.Practice;
        Data.selectedPlanetId = planetId;
        Data.selectedMinigameId = -1;
        Data.selectedPracticeTrackId = trackId;
        Data.returnSceneName = returnSceneName ?? "";

        Data.currentScore = 0;
        Data.currentLife = 0;
    }

    public void StartPlanetRun(
        int planetId,
        int startingLife,
        string returnSceneName)
    {
        EnsureInitialized();

        if (planetId <= 0)
        {
            Debug.LogError(
                $"[Session] 잘못된 행성 ID: {planetId}"
            );

            return;
        }

        Data.gameMode = GameMode.PlanetRun;
        Data.selectedPlanetId = planetId;
        Data.selectedMinigameId = -1;
        Data.selectedPracticeTrackId = -1;
        Data.returnSceneName = returnSceneName ?? "";

        Data.currentScore = 0;
        Data.currentLife = Mathf.Max(0, startingLife);
    }

    public void StartStory(string returnSceneName)
    {
        EnsureInitialized();

        Data.gameMode = GameMode.Story;
        Data.selectedPlanetId = -1;
        Data.selectedMinigameId = -1;
        Data.selectedPracticeTrackId = -1;
        Data.returnSceneName = returnSceneName ?? "";

        Data.currentScore = 0;
        Data.currentLife = 0;
    }

    public void AddScore(int value)
    {
        EnsureInitialized();
        Data.currentScore += value;
    }

    public void SetLife(int value)
    {
        EnsureInitialized();
        Data.currentLife = Mathf.Max(0, value);
    }

    public void LoseLife(int amount = 1)
    {
        EnsureInitialized();

        Data.currentLife = Mathf.Max(
            0,
            Data.currentLife - Mathf.Max(0, amount)
        );
    }

    public void Clear()
    {
        Data = new GameSessionData();
    }

    private void EnsureInitialized()
    {
        if (Data == null)
            Initialize();
    }
}