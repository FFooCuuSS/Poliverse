using System.Collections.Generic;

public static class PracticeTrackCatalog
{
    public static List<int> GetMinigames(
        int planetId,
        int trackId)
    {
        List<int> result = new List<int>();

        int maxMinigame =
            planetId == 1 ? 10 : 15;

        int startMinigame =
            (trackId - 1) * 3 + 1;

        for (int i = 0; i < 3; i++)
        {
            int minigameId =
                startMinigame + i;

            if (minigameId > maxMinigame)
                break;

            result.Add(minigameId);
        }

        return result;
    }
}