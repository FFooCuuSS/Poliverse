using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    public GameSaveData Data { get; private set; }

    public bool IsInitialized { get; private set; }
    public bool IsSaving => saveRoutineRunning;

    private ISaveRepository repository;

    private bool saveQueued;
    private bool saveRoutineRunning;

    public void SetRepository(
        ISaveRepository targetRepository)
    {
        if (IsInitialized)
        {
            Debug.LogWarning(
                "[Save] 초기화 이후에는 저장소를 교체할 수 없습니다."
            );

            return;
        }

        repository = targetRepository;
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized)
            return;

        if (repository == null)
        {
            string saveFilePath = Path.Combine(
                Application.persistentDataPath,
                "save.json"
            );

            repository =
                new LocalSaveRepository(saveFilePath);
        }

        try
        {
            Data = await repository.LoadAsync();
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[Save] 저장 데이터 로드 실패\n" +
                $"Repository: {repository.RepositoryName}\n" +
                $"{exception}"
            );

            Data = null;
        }

        if (Data == null)
        {
            Data = CreateDefaultSave();
        }

        EnsureRequiredData();

        IsInitialized = true;

        // 새 파일이거나 손상 복구된 데이터일 수 있으므로 저장
        RequestSave();

        // FIREBASE-LATER:
        // GameRoot가 Firebase 초기화와 인증을 완료한 뒤
        // HybridSaveRepository를 SetRepository로 먼저 넣어준다.
        //
        // repository = new HybridSaveRepository(
        //     localRepository,
        //     firebaseRepository
        // );
        //
        // 이후 LoadAsync에서 로컬/클라우드를 불러와 병합한다.
    }

    private GameSaveData CreateDefaultSave()
    {
        GameSaveData data =
            new GameSaveData();

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

        if (Data.story == null)
            Data.story = new StoryProgressData();

        if (Data.story.completedStoryIds == null)
        {
            Data.story.completedStoryIds =
                new List<string>();
        }

        if (Data.planets == null)
            Data.planets = new List<PlanetProgressData>();

        for (int planetId = 1;
             planetId <= 4;
             planetId++)
        {
            PlanetProgressData planet =
                GetPlanetProgress(planetId);

            if (planet.completedTutorialIds == null)
            {
                planet.completedTutorialIds =
                    new List<int>();
            }
        }
    }

    public PlanetProgressData GetPlanetProgress(
        int planetId)
    {
        EnsureDataExists();

        if (planetId <= 0)
        {
            Debug.LogError(
                $"[Save] 잘못된 행성 ID: {planetId}"
            );

            return null;
        }

        PlanetProgressData found =
            Data.planets.Find(
                planet => planet.planetId == planetId
            );

        if (found != null)
            return found;

        PlanetProgressData created =
            new PlanetProgressData
            {
                planetId = planetId,
                unlocked = planetId == 1
            };

        Data.planets.Add(created);

        return created;
    }

    public bool IsPlanetUnlocked(int planetId)
    {
        PlanetProgressData planet =
            GetPlanetProgress(planetId);

        return planet != null &&
               planet.unlocked;
    }

    public void UnlockPlanet(int planetId)
    {
        PlanetProgressData planet =
            GetPlanetProgress(planetId);

        if (planet == null || planet.unlocked)
            return;

        planet.unlocked = true;
        RequestSave();
    }

    public bool IsTutorialCompleted(
        int planetId,
        int tutorialId)
    {
        PlanetProgressData planet =
            GetPlanetProgress(planetId);

        if (planet == null || tutorialId <= 0)
            return false;

        return planet.completedTutorialIds.Contains(
            tutorialId
        );
    }

    public void CompleteTutorial(
        int planetId,
        int tutorialId)
    {
        PlanetProgressData planet =
            GetPlanetProgress(planetId);

        if (planet == null || tutorialId <= 0)
        {
            Debug.LogWarning(
                $"[Save] 잘못된 튜토리얼 값: " +
                $"{planetId}-{tutorialId}"
            );

            return;
        }

        if (planet.completedTutorialIds.Contains(
                tutorialId))
        {
            return;
        }

        planet.completedTutorialIds.Add(tutorialId);
        RequestSave();
    }

    public void RecordFullRunResult(
        int planetId,
        int score,
        RunEvaluation evaluation)
    {
        PlanetProgressData planet =
            GetPlanetProgress(planetId);

        if (planet == null)
            return;

        score = Mathf.Max(0, score);

        planet.fullRunClearCount++;

        planet.lastFullRunScore = score;
        planet.lastFullRunEvaluation = evaluation;

        if (score > planet.bestFullRunScore)
            planet.bestFullRunScore = score;

        if (evaluation > planet.bestFullRunEvaluation)
        {
            planet.bestFullRunEvaluation =
                evaluation;
        }

        RequestSave();
    }

    public void SetStoryProgress(
        int progressIndex,
        string storyId)
    {
        EnsureDataExists();

        Data.story.progressIndex = Mathf.Max(
            Data.story.progressIndex,
            progressIndex
        );

        if (!string.IsNullOrWhiteSpace(storyId))
            Data.story.currentStoryId = storyId;

        RequestSave();
    }

    public void CompleteStorySection(string storyId)
    {
        EnsureDataExists();

        if (string.IsNullOrWhiteSpace(storyId))
            return;

        if (!Data.story.completedStoryIds.Contains(
                storyId))
        {
            Data.story.completedStoryIds.Add(storyId);
            RequestSave();
        }
    }

    public void CompleteMainStory()
    {
        EnsureDataExists();

        if (Data.story.mainStoryCompleted)
            return;

        Data.story.mainStoryCompleted = true;
        RequestSave();
    }

    public void DeleteSaveData()
    {
        Data = CreateDefaultSave();
        RequestSave();
    }

    public void RequestSave()
    {
        if (!IsInitialized || repository == null)
        {
            Debug.LogWarning(
                "[Save] 초기화 전에 저장이 요청되었습니다."
            );

            return;
        }

        saveQueued = true;

        if (saveRoutineRunning)
            return;

        saveRoutineRunning = true;
        StartCoroutine(SaveRoutine());
    }

    private IEnumerator SaveRoutine()
    {
        while (saveQueued)
        {
            saveQueued = false;

            PrepareSaveMetadata();

            Task<bool> saveTask = null;

            try
            {
                saveTask =
                    repository.SaveAsync(Data);
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
                    $"Repository: {repository.RepositoryName}\n" +
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
        }

        saveRoutineRunning = false;
    }

    private void PrepareSaveMetadata()
    {
        if (Data == null)
            return;

        Data.revision++;
        Data.lastUpdatedUtc =
            DateTime.UtcNow.ToString("O");

        // FIREBASE-LATER:
        // 인증 완료 후에만 ownerUid를 UID로 설정한다.
        //
        // if (GameRoot.Instance.Account.HasCloudAccount)
        //     Data.ownerUid =
        //         GameRoot.Instance.Account.Data.uid;
    }

    private void EnsureDataExists()
    {
        if (Data == null)
            Data = CreateDefaultSave();
    }

    // FIREBASE-LATER:
    // 클라우드 병합 규칙은 저장소 계층에서 처리한다.
    //
    // story.progressIndex        = Max(local, cloud)
    // planet.unlocked            = local || cloud
    // completedTutorialIds       = Union(local, cloud)
    // bestFullRunScore           = Max(local, cloud)
    // bestFullRunEvaluation      = Max(local, cloud)
    // last 결과                  = 최신 server timestamp
}