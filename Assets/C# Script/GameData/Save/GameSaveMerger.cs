using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class GameSaveMerger
{
    public static GameSaveData Merge(
        GameSaveData local,
        GameSaveData cloud)
    {
        if (local == null &&
            cloud == null)
        {
            return null;
        }

        if (local == null)
            return Clone(cloud);

        if (cloud == null)
            return Clone(local);

        bool cloudIsNewer =
            IsSecondNewer(
                local,
                cloud
            );

        GameSaveData merged =
            Clone(local);

        merged.schemaVersion =
            Math.Max(
                local.schemaVersion,
                cloud.schemaVersion
            );

        merged.revision =
            Math.Max(
                local.revision,
                cloud.revision
            );

        merged.lastUpdatedUtc =
            SelectLatestTimestamp(
                local.lastUpdatedUtc,
                cloud.lastUpdatedUtc
            );

        merged.ownerUid =
            !string.IsNullOrWhiteSpace(
                cloud.ownerUid)
                ? cloud.ownerUid
                : local.ownerUid;

        merged.story =
            MergeStory(
                local.story,
                cloud.story,
                cloudIsNewer
            );

        merged.planets =
            MergePlanets(
                local.planets,
                cloud.planets,
                cloudIsNewer
            );

        return merged;
    }

    private static StoryProgressData MergeStory(
        StoryProgressData local,
        StoryProgressData cloud,
        bool cloudIsNewer)
    {
        if (local == null &&
            cloud == null)
        {
            return new StoryProgressData();
        }

        if (local == null)
            return Clone(cloud);

        if (cloud == null)
            return Clone(local);

        StoryProgressData merged =
            Clone(local);

        merged.progressIndex =
            Math.Max(
                local.progressIndex,
                cloud.progressIndex
            );

        merged.mainStoryCompleted =
            local.mainStoryCompleted ||
            cloud.mainStoryCompleted;

        merged.completedStoryIds =
            Union(
                local.completedStoryIds,
                cloud.completedStoryIds
            );

        if (cloud.progressIndex >
            local.progressIndex)
        {
            merged.currentStoryId =
                cloud.currentStoryId;
        }
        else if (cloud.progressIndex ==
                 local.progressIndex &&
                 cloudIsNewer)
        {
            merged.currentStoryId =
                cloud.currentStoryId;
        }

        return merged;
    }

    private static List<PlanetProgressData>
        MergePlanets(
            List<PlanetProgressData> local,
            List<PlanetProgressData> cloud,
            bool cloudIsNewer)
    {
        Dictionary<int, PlanetProgressData>
            localById =
                ToPlanetDictionary(local);

        Dictionary<int, PlanetProgressData>
            cloudById =
                ToPlanetDictionary(cloud);

        HashSet<int> allIds =
            new HashSet<int>();

        foreach (int id in localById.Keys)
            allIds.Add(id);

        foreach (int id in cloudById.Keys)
            allIds.Add(id);

        List<int> sortedIds =
            new List<int>(allIds);

        sortedIds.Sort();

        List<PlanetProgressData> result =
            new List<PlanetProgressData>();

        foreach (int id in sortedIds)
        {
            localById.TryGetValue(
                id,
                out PlanetProgressData localPlanet
            );

            cloudById.TryGetValue(
                id,
                out PlanetProgressData cloudPlanet
            );

            if (localPlanet == null)
            {
                result.Add(
                    Clone(cloudPlanet)
                );

                continue;
            }

            if (cloudPlanet == null)
            {
                result.Add(
                    Clone(localPlanet)
                );

                continue;
            }

            PlanetProgressData merged =
                Clone(localPlanet);

            merged.planetId = id;

            merged.unlocked =
                localPlanet.unlocked ||
                cloudPlanet.unlocked;

            merged.completedTutorialIds =
                Union(
                    localPlanet.completedTutorialIds,
                    cloudPlanet.completedTutorialIds
                );

            merged.completedPracticeMinigameIds =
                Union(
                    localPlanet
                        .completedPracticeMinigameIds,
                    cloudPlanet
                        .completedPracticeMinigameIds
                );

            merged.fullRunClearCount =
                Math.Max(
                    localPlanet.fullRunClearCount,
                    cloudPlanet.fullRunClearCount
                );

            MergeBestResult(
                merged,
                localPlanet,
                cloudPlanet
            );

            if (cloudIsNewer)
            {
                merged.lastFullRunScore =
                    cloudPlanet.lastFullRunScore;

                merged.lastFullRunEvaluation =
                    cloudPlanet
                        .lastFullRunEvaluation;
            }
            else
            {
                merged.lastFullRunScore =
                    localPlanet.lastFullRunScore;

                merged.lastFullRunEvaluation =
                    localPlanet
                        .lastFullRunEvaluation;
            }

            result.Add(merged);
        }

        return result;
    }

    private static void MergeBestResult(
        PlanetProgressData merged,
        PlanetProgressData local,
        PlanetProgressData cloud)
    {
        if (cloud.bestFullRunScore >
            local.bestFullRunScore)
        {
            merged.bestFullRunScore =
                cloud.bestFullRunScore;

            merged.bestFullRunEvaluation =
                cloud.bestFullRunEvaluation;

            return;
        }

        if (local.bestFullRunScore >
            cloud.bestFullRunScore)
        {
            merged.bestFullRunScore =
                local.bestFullRunScore;

            merged.bestFullRunEvaluation =
                local.bestFullRunEvaluation;

            return;
        }

        merged.bestFullRunScore =
            local.bestFullRunScore;

        merged.bestFullRunEvaluation =
            (RunEvaluation)Math.Max(
                (int)local.bestFullRunEvaluation,
                (int)cloud.bestFullRunEvaluation
            );
    }

    private static Dictionary<int, PlanetProgressData>
        ToPlanetDictionary(
            List<PlanetProgressData> planets)
    {
        Dictionary<int, PlanetProgressData>
            result =
                new Dictionary<int, PlanetProgressData>();

        if (planets == null)
            return result;

        foreach (PlanetProgressData planet
                 in planets)
        {
            if (planet == null ||
                planet.planetId <= 0)
            {
                continue;
            }

            result[planet.planetId] =
                planet;
        }

        return result;
    }

    private static List<T> Union<T>(
        List<T> first,
        List<T> second)
    {
        HashSet<T> set =
            new HashSet<T>();

        if (first != null)
        {
            foreach (T value in first)
                set.Add(value);
        }

        if (second != null)
        {
            foreach (T value in second)
                set.Add(value);
        }

        return new List<T>(set);
    }

    private static bool IsSecondNewer(
        GameSaveData first,
        GameSaveData second)
    {
        bool firstValid =
            TryParseUtc(
                first.lastUpdatedUtc,
                out DateTime firstTime
            );

        bool secondValid =
            TryParseUtc(
                second.lastUpdatedUtc,
                out DateTime secondTime
            );

        if (firstValid && secondValid)
            return secondTime > firstTime;

        if (!firstValid && secondValid)
            return true;

        if (firstValid && !secondValid)
            return false;

        return second.revision >
               first.revision;
    }

    private static string SelectLatestTimestamp(
        string first,
        string second)
    {
        bool firstValid =
            TryParseUtc(
                first,
                out DateTime firstTime
            );

        bool secondValid =
            TryParseUtc(
                second,
                out DateTime secondTime
            );

        if (firstValid && secondValid)
        {
            return secondTime > firstTime
                ? second
                : first;
        }

        if (secondValid)
            return second;

        return first ?? "";
    }

    private static bool TryParseUtc(
        string value,
        out DateTime result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            return false;
        }

        return DateTime.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out result
        );
    }

    private static T Clone<T>(
        T source)
    {
        if (source == null)
            return default;

        string json =
            JsonUtility.ToJson(source);

        return JsonUtility.FromJson<T>(
            json
        );
    }
}