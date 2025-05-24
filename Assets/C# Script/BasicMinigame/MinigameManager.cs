using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public static int stage; // 로비씬에서 stage (0~3) 입력

    [SerializeField] private MinigameUImanager uiManager;

    private string[] miniGameNames;
    private int currentIndex = 0;

    void Start()
    {
        GenerateMiniGameList(stage);
        LoadNextGame();
    }

    void GenerateMiniGameList(int stageIndex)
    {
        string planet = "";
        int count = 0;

        switch (stageIndex)
        {
            case 0: planet = "PolicePlanet"; count = 10; break;
            case 1: planet = "CandyPlanet"; count = 15; break;
            case 2: planet = "MafiaPlanet"; count = 15; break;
            case 3: planet = "MusicPlanet"; count = 15; break;
            default:
                Debug.LogError("잘못된 스테이지 번호입니다.");
                return;
        }

        miniGameNames = new string[count];
        for (int i = 0; i < count; i++)
        {
            miniGameNames[i] = $"{planet}/{i + 1}_{count}minigame";
        }
    }

    void LoadNextGame()
    {
        if (currentIndex >= miniGameNames.Length)
        {
            Debug.Log("모든 미니게임 완료!");
            return;
        }

        string path = $"MinigamePrefab/{miniGameNames[currentIndex]}";
        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError($"프리팹 로드 실패: {path}");
            currentIndex++;
            LoadNextGame();
            return;
        }

        GameObject gameInstance = Instantiate(prefab);
        MiniGameBase game = gameInstance.GetComponent<MiniGameBase>();

        if (game == null)
        {
            Debug.LogError($"MiniGameBase 누락: {gameInstance.name}");
            Destroy(gameInstance);
            currentIndex++;
            LoadNextGame();
            return;
        }

        // UI 표시
        uiManager.HideResult();
        uiManager.ShowGuide(game.GetMinigameExplain, 2f);
        uiManager.StartTimer(game.GetTimerDuration);

        // 이벤트 처리
        game.OnSuccess += () =>
        {
            uiManager.ShowResult("성공!");
            Destroy(gameInstance);
            currentIndex++;
            LoadNextGame();
        };

        game.OnFail += () =>
        {
            uiManager.ShowResult("실패!");
            Destroy(gameInstance);
            currentIndex++;
            LoadNextGame();
        };

        game.StartGame();
    }
}
