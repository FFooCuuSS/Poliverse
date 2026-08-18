using System;
using System.Collections.Generic;

public enum RunEvaluation
{
    None = 0,
    D = 1,
    C = 2,
    B = 3,
    A = 4,
    S = 5
}

[Serializable]
public class StoryProgressData
{
    public int progressIndex = 0;

    public string currentStoryId =
        "Prologue";

    public bool mainStoryCompleted = false;

    public List<string> completedStoryIds =
        new List<string>();
}

[Serializable]
public class PlanetProgressData
{
    public int planetId;

    public bool unlocked = false;

    // 해당 행성에서 완료한 튜토리얼 번호
    public List<int> completedTutorialIds =
        new List<int>();

    // 해당 행성에서 클리어한 연습 미니게임 번호
    public List<int> completedPracticeMinigameIds =
        new List<int>();

    public int fullRunClearCount = 0;

    public int lastFullRunScore = 0;

    public RunEvaluation lastFullRunEvaluation =
        RunEvaluation.None;

    public int bestFullRunScore = 0;

    public RunEvaluation bestFullRunEvaluation =
        RunEvaluation.None;
}

[Serializable]
public class GameSaveData
{
    public int schemaVersion = 2;

    // FIREBASE-LATER:
    // 클라우드 저장 활성화 후 Firebase UID 기록.
    public string ownerUid = "";

    // 저장할 때마다 증가.
    // 이후 클라우드 충돌 확인에 사용.
    public long revision = 0;

    public StoryProgressData story =
        new StoryProgressData();

    public List<PlanetProgressData> planets =
        new List<PlanetProgressData>();

    public string lastUpdatedUtc = "";
}