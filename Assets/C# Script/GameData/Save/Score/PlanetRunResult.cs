using System;

[Serializable]
public class PlanetRunResult
{
    public int planetId;

    public int completedMinigameCount;

    public int totalNode;
    public int perfect;
    public int good;
    public int miss;

    public int score;

    public RunEvaluation evaluation =
        RunEvaluation.None;

    public bool isCleared;

    public bool HasScoreData =>
        totalNode > 0;

    public bool CanBeSaved =>
        planetId > 0 &&
        isCleared &&
        HasScoreData;
}