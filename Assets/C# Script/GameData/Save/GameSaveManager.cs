using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    private const int CurrentSchemaVersion = 2;

    public GameSaveData Data { get; private set; }

    public bool IsInitialized { get; private set; }

    public bool IsSaving =>
        saveRoutineRunning;

    private ISaveRepository repository;

    private HybridSaveRepository
        hybridRepository;

    private GameAccountManager
        accountManager;

    private bool saveQueued;
    private bool saveRoutineRunning;

    private bool accountChangeRunning;

    public void SetAccountManager(
        GameAccountManager targetManager)
    {
        if (accountManager != null)
        {
            accountManager.OnAccountChanged -=
                HandleAccountChanged;
        }

        accountManager =
            targetManager;

        if (accountManager != null)
        {
            accountManager.OnAccountChanged +=
                HandleAccountChanged;
        }
    }

    public void SetRepository(
        ISaveRepository targetRepository)
    {
        if (IsInitialized)
        {
            Debug.LogWarning(
                "[Save] 초기화 이후에는 저장소를 " +
                "직접 교체할 수 없습니다."
            );

            return;
        }

        repository =
            targetRepository;

        hybridRepository =
            targetRepository
                as HybridSaveRepository;
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized)
            return;

        if (repository == null)
            BuildDefaultRepository();

        try
        {
            Data =
                await LoadInitialDataAsync();
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[Save] 저장 데이터 로드 실패\n" +
                $"Repository: " +
                $"{repository.RepositoryName}\n" +
                $"{exception}"
            );

            Data = null;
        }

        if (Data == null)
            Data = CreateDefaultSave();

        EnsureRequiredData();

        ApplyCurrentOwnerUid();

        IsInitialized = true;

        // 신규 데이터, 병합 데이터,
        // 구버전 보정 데이터를 다시 저장한다.
        RequestSave();
    }

    private void BuildDefaultRepository()
    {
        string saveFilePath =
            Path.Combine(
                Application.persistentDataPath,
                "save.json"
            );

        LocalSaveRepository local =
            new LocalSaveRepository(
                saveFilePath
            );

        ISaveRepository cloud = null;

        if (CanUseCloud())
        {
            cloud =
                new FirestoreSaveRepository(
                    accountManager.Data.uid
                );
        }

        hybridRepository =
            new HybridSaveRepository(
                local,
                cloud
            );

        repository =
            hybridRepository;
    }

    private async Task<GameSaveData>
        LoadInitialDataAsync()
    {
        if (hybridRepository == null)
            return await repository.LoadAsync();

        GameSaveData localData =
            await hybridRepository
                .LoadLocalAsync();

        if (!CanUseCloud())
            return localData;

        GameSaveData cloudData =
            await hybridRepository
                .LoadCloudAsync();

        return ResolveDataForAccount(
            localData,
            cloudData,
            accountManager.Data.uid
        );
    }

    private GameSaveData ResolveDataForAccount(
        GameSaveData localData,
        GameSaveData cloudData,
        string targetUid)
    {
        if (string.IsNullOrWhiteSpace(targetUid))
        {
            return localData ??
                   cloudData;
        }

        GameSaveData result;

        if (localData == null)
        {
            result =
                cloudData ??
                CreateDefaultSave();
        }
        else if (string.IsNullOrWhiteSpace(
                     localData.ownerUid) ||
                 localData.ownerUid ==
                 targetUid)
        {
            result =
                GameSaveMerger.Merge(
                    localData,
                    cloudData
                );

            if (result == null)
                result = CreateDefaultSave();
        }
        else
        {
            // 다른 Firebase 계정의 로컬 데이터를
            // 현재 로그인 계정과 자동 병합하지 않는다.
            Debug.LogWarning(
                $"[Save] 다른 계정의 로컬 세이브 발견\n" +
                $"- Local Owner: " +
                $"{localData.ownerUid}\n" +
                $"- Current UID: {targetUid}"
            );

            result =
                cloudData ??
                CreateDefaultSave();
        }

        result.ownerUid =
            targetUid;

        return result;
    }

    private GameSaveData CreateDefaultSave()
    {
        GameSaveData data =
            new GameSaveData
            {
                schemaVersion =
                    CurrentSchemaVersion
            };

        if (CanUseCloud())
        {
            data.ownerUid =
                accountManager.Data.uid;
        }

        for (int planetId = 1;
             planetId <= 4;
             planetId++)
        {
            data.planets.Add(
                new PlanetProgressData
                {
                    planetId = planetId,
                    unlocked = planetId == 1
                }
            );
        }

        return data;
    }

    private void EnsureRequiredData()
    {
        if (Data == null)
            Data = CreateDefaultSave();

        Data.schemaVersion =
            CurrentSchemaVersion;

        if (Data.story == null)
        {
            Data.story =
                new StoryProgressData();
        }

        if (Data.story.completedStoryIds == null)
        {
            Data.story.completedStoryIds =
                new List<string>();
        }

        if (Data.planets == null)
        {
            Data.planets =
                new List<PlanetProgressData>();
        }

        for (int planetId = 1;
             planetId <= 4;
             planetId++)
        {
            PlanetProgressData planet =
                GetPlanetProgress(
                    planetId
                );

            if (planet == null)
                continue;

            EnsurePlanetLists(
                planet
            );
        }
    }

    public PlanetProgressData GetPlanetProgress(
        int planetId)
    {
        EnsureDataExists();

        if (planetId <= 0)
        {
            Debug.LogError(
                $"[Save] 잘못된 행성 ID: " +
                $"{planetId}"
            );

            return null;
        }

        PlanetProgressData found =
            Data.planets.Find(
                planet =>
                    planet != null &&
                    planet.planetId == planetId
            );

        if (found != null)
            return found;

        PlanetProgressData created =
            new PlanetProgressData
            {
                planetId = planetId,
                unlocked = planetId == 1
            };

        Data.planets.Add(
            created
        );

        return created;
    }

    public bool IsPlanetUnlocked(
        int planetId)
    {
        PlanetProgressData planet =
            GetPlanetProgress(
                planetId
            );

        return planet != null &&
               planet.unlocked;
    }

    public void UnlockPlanet(
        int planetId)
    {
        PlanetProgressData planet =
            GetPlanetProgress(
                planetId
            );

        if (planet == null ||
            planet.unlocked)
        {
            return;
        }

        planet.unlocked = true;

        RequestSave();
    }

    public bool IsTutorialCompleted(
        int planetId,
        int tutorialId)
    {
        PlanetProgressData planet =
            GetPlanetProgress(
                planetId
            );

        if (planet == null ||
            tutorialId <= 0)
        {
            return false;
        }

        EnsurePlanetLists(
            planet
        );

        return planet
            .completedTutorialIds
            .Contains(tutorialId);
    }

    public void CompleteTutorial(
        int planetId,
        int tutorialId)
    {
        PlanetProgressData planet =
            GetPlanetProgress(
                planetId
            );

        if (planet == null ||
            tutorialId <= 0)
        {
            Debug.LogWarning(
                $"[Save] 잘못된 튜토리얼 값: " +
                $"{planetId}-{tutorialId}"
            );

            return;
        }

        EnsurePlanetLists(
            planet
        );

        if (planet
            .completedTutorialIds
            .Contains(tutorialId))
        {
            return;
        }

        planet
            .completedTutorialIds
            .Add(tutorialId);

        RequestSave();
    }

    public bool IsPracticeMinigameCleared(
        int planetId,
        int minigameId)
    {
        PlanetProgressData planet =
            GetPlanetProgress(
                planetId
            );

        if (planet == null ||
            minigameId <= 0)
        {
            return false;
        }

        EnsurePlanetLists(
            planet
        );

        return planet
            .completedPracticeMinigameIds
            .Contains(minigameId);
    }

    public void CompletePracticeMinigame(
        int planetId,
        int minigameId)
    {
        PlanetProgressData planet =
            GetPlanetProgress(
                planetId
            );

        if (planet == null ||
            minigameId <= 0)
        {
            Debug.LogWarning(
                $"[Save] 잘못된 연습 미니게임 값: " +
                $"{planetId}-{minigameId}"
            );

            return;
        }

        EnsurePlanetLists(
            planet
        );

        if (planet
            .completedPracticeMinigameIds
            .Contains(minigameId))
        {
            return;
        }

        planet
            .completedPracticeMinigameIds
            .Add(minigameId);

        Debug.Log(
            $"[Save] 연습 미니게임 완료: " +
            $"{planetId}-{minigameId}"
        );

        RequestSave();
    }

    public void RecordFullRunResult(
        PlanetRunResult result)
    {
        if (result == null)
        {
            Debug.LogError(
                "[Save] PlanetRunResult가 null입니다."
            );

            return;
        }

        if (!result.CanBeSaved)
        {
            Debug.LogWarning(
                $"[Save] 저장할 수 없는 행성 결과입니다.\n" +
                $"- Planet: {result.planetId}\n" +
                $"- Cleared: {result.isCleared}\n" +
                $"- Total Node: {result.totalNode}"
            );

            return;
        }

        PlanetProgressData planet =
            GetPlanetProgress(
                result.planetId
            );

        if (planet == null)
            return;

        int score =
            Mathf.Clamp(
                result.score,
                0,
                100
            );

        planet.fullRunClearCount++;

        planet.lastFullRunScore =
            score;

        planet.lastFullRunEvaluation =
            result.evaluation;

        if (score >
            planet.bestFullRunScore)
        {
            planet.bestFullRunScore =
                score;

            planet.bestFullRunEvaluation =
                result.evaluation;
        }
        else if (
            score ==
            planet.bestFullRunScore &&
            result.evaluation >
            planet.bestFullRunEvaluation)
        {
            planet.bestFullRunEvaluation =
                result.evaluation;
        }

        Debug.Log(
            $"[Save] 행성 전체 플레이 결과 저장\n" +
            $"- Planet: {result.planetId}\n" +
            $"- Score: {score}\n" +
            $"- Evaluation: " +
            $"{result.evaluation}\n" +
            $"- Clear Count: " +
            $"{planet.fullRunClearCount}"
        );

        RequestSave();
    }

    public void RecordFullRunResult(
        int planetId,
        int score,
        RunEvaluation evaluation)
    {
        PlanetRunResult result =
            new PlanetRunResult
            {
                planetId = planetId,
                totalNode = 1,
                score =
                    Mathf.Clamp(
                        score,
                        0,
                        100
                    ),
                evaluation = evaluation,
                isCleared = true
            };

        RecordFullRunResult(
            result
        );
    }

    public void SetStoryProgress(
        int progressIndex,
        string storyId)
    {
        EnsureDataExists();

        Data.story.progressIndex =
            Mathf.Max(
                Data.story.progressIndex,
                progressIndex
            );

        if (!string.IsNullOrWhiteSpace(
                storyId))
        {
            Data.story.currentStoryId =
                storyId;
        }

        RequestSave();
    }

    public void CompleteStorySection(
        string storyId)
    {
        EnsureDataExists();

        if (string.IsNullOrWhiteSpace(
                storyId))
        {
            return;
        }

        if (Data.story
            .completedStoryIds
            .Contains(storyId))
        {
            return;
        }

        Data.story
            .completedStoryIds
            .Add(storyId);

        RequestSave();
    }

    public void CompleteMainStory()
    {
        EnsureDataExists();

        if (Data.story.mainStoryCompleted)
            return;

        Data.story.mainStoryCompleted =
            true;

        RequestSave();
    }

    public void DeleteSaveData()
    {
        Data =
            CreateDefaultSave();

        RequestSave();
    }

    public void RequestSave()
    {
        if (!IsInitialized ||
            repository == null)
        {
            Debug.LogWarning(
                "[Save] 초기화 전에 저장이 요청되었습니다."
            );

            return;
        }

        saveQueued = true;

        if (saveRoutineRunning)
            return;

        saveRoutineRunning =
            true;

        StartCoroutine(
            SaveRoutine()
        );
    }

    private IEnumerator SaveRoutine()
    {
        while (saveQueued)
        {
            saveQueued = false;

            PrepareSaveMetadata();

            Task<bool> saveTask =
                null;

            try
            {
                saveTask =
                    repository
                        .SaveAsync(Data);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[Save] 저장 요청 생성 실패\n" +
                    $"{exception}"
                );
            }

            if (saveTask == null)
                continue;

            while (!saveTask.IsCompleted)
                yield return null;

            if (saveTask.IsFaulted)
            {
                Debug.LogError(
                    $"[Save] 저장 실패\n" +
                    $"Repository: " +
                    $"{repository.RepositoryName}\n" +
                    $"{saveTask.Exception}"
                );
            }
            else if (saveTask.IsCanceled)
            {
                Debug.LogWarning(
                    "[Save] 저장 작업이 취소되었습니다."
                );
            }
            else if (!saveTask.Result)
            {
                Debug.LogError(
                    $"[Save] 저장소가 실패를 반환했습니다: " +
                    $"{repository.RepositoryName}"
                );
            }
            else
            {
                Debug.Log(
                    $"[Save Debug] 데이터 저장 완료 | " +
                    $"Repository: " +
                    $"{repository.RepositoryName} | " +
                    $"Revision: {Data.revision} | " +
                    $"Updated: {Data.lastUpdatedUtc}"
                );
            }
        }

        saveRoutineRunning =
            false;
    }

    private void PrepareSaveMetadata()
    {
        if (Data == null)
            return;

        Data.revision++;

        Data.lastUpdatedUtc =
            DateTime.UtcNow
                .ToString("O");

        ApplyCurrentOwnerUid();
    }

    private void ApplyCurrentOwnerUid()
    {
        if (Data == null ||
            !CanUseCloud())
        {
            return;
        }

        Data.ownerUid =
            accountManager.Data.uid;
    }

    private bool CanUseCloud()
    {
        return accountManager != null &&
               accountManager
                   .IsFirebaseReady &&
               accountManager
                   .HasCloudAccount;
    }

    private async void HandleAccountChanged()
    {
        if (!IsInitialized ||
            hybridRepository == null ||
            accountChangeRunning)
        {
            return;
        }

        accountChangeRunning =
            true;

        try
        {
            if (!CanUseCloud())
            {
                hybridRepository
                    .ClearCloudRepository();

                Debug.Log(
                    "[Save] Cloud 저장 연결 해제. " +
                    "Local 저장을 유지합니다."
                );

                return;
            }

            string uid =
                accountManager.Data.uid;

            FirestoreSaveRepository cloud =
                new FirestoreSaveRepository(
                    uid
                );

            hybridRepository
                .SetCloudRepository(
                    cloud
                );

            GameSaveData cloudData =
                await cloud.LoadAsync();

            GameSaveData resolved =
                ResolveDataForAccount(
                    Data,
                    cloudData,
                    uid
                );

            if (resolved != null)
                Data = resolved;

            EnsureRequiredData();

            Data.ownerUid =
                uid;

            Debug.Log(
                $"[Save] Cloud 계정 연결 완료 | " +
                $"UID: {uid}"
            );

            RequestSave();
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[Save] 계정 변경 처리 실패\n" +
                $"{exception}"
            );
        }
        finally
        {
            accountChangeRunning =
                false;
        }
    }

    private void EnsureDataExists()
    {
        if (Data == null)
            Data = CreateDefaultSave();

        if (Data.planets == null)
        {
            Data.planets =
                new List<PlanetProgressData>();
        }

        if (Data.story == null)
        {
            Data.story =
                new StoryProgressData();
        }

        if (Data.story.completedStoryIds == null)
        {
            Data.story.completedStoryIds =
                new List<string>();
        }
    }

    private void EnsurePlanetLists(
        PlanetProgressData planet)
    {
        if (planet == null)
            return;

        if (planet.completedTutorialIds == null)
        {
            planet.completedTutorialIds =
                new List<int>();
        }

        if (planet
            .completedPracticeMinigameIds == null)
        {
            planet
                .completedPracticeMinigameIds =
                new List<int>();
        }
    }

    private void OnDestroy()
    {
        if (accountManager != null)
        {
            accountManager.OnAccountChanged -=
                HandleAccountChanged;
        }
    }
}