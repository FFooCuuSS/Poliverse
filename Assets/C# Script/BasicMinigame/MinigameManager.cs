using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public static int stage; // �κ������ stage (0~3) �Է�

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
                Debug.LogError("�߸��� �������� ��ȣ�Դϴ�.");
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
            Debug.Log("��� �̴ϰ��� �Ϸ�!");
            return;
        }

        string path = $"MinigamePrefab/{miniGameNames[currentIndex]}";
        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError($"������ �ε� ����: {path}");
            currentIndex++;
            LoadNextGame();
            return;
        }

        GameObject gameInstance = Instantiate(prefab);
        MiniGameBase game = gameInstance.GetComponent<MiniGameBase>();

        if (game == null)
        {
            Debug.LogError($"MiniGameBase ����: {gameInstance.name}");
            Destroy(gameInstance);
            currentIndex++;
            LoadNextGame();
            return;
        }

        // UI ǥ��
        uiManager.HideResult();
        uiManager.ShowGuide(game.GetMinigameExplain, 2f);
        uiManager.StartTimer(game.GetTimerDuration);

        // �̺�Ʈ ó��
        game.OnSuccess += () =>
        {
            uiManager.ShowResult("����!");
            Destroy(gameInstance);
            currentIndex++;
            LoadNextGame();
        };

        game.OnFail += () =>
        {
            uiManager.ShowResult("����!");
            Destroy(gameInstance);
            currentIndex++;
            LoadNextGame();
        };

        game.StartGame();
    }
}
