using System;

public enum GameMode
{
    None,
    Story,
    PlanetRun,
    Practice
}

[Serializable]
public class GameSessionData
{
    public GameMode gameMode = GameMode.None;

    // 씬 전환 동안만 유지되는 선택값
    public int selectedPlanetId = -1;
    public int selectedMinigameId = -1;

    // 해당 모드에서 나갈 때 돌아갈 씬
    public string returnSceneName = "";

    // 현재 플레이 한 회차에서만 사용하는 값
    public int currentScore = 0;
    public int currentLife = 0;
}