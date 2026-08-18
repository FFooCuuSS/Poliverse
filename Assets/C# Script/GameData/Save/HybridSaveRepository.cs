using System.Threading.Tasks;
using UnityEngine;

public class HybridSaveRepository
    : ISaveRepository
{
    public string RepositoryName =>
        cloudRepository == null
            ? "Local JSON"
            : "Local JSON + Firebase Firestore";

    private readonly LocalSaveRepository
        localRepository;

    private ISaveRepository
        cloudRepository;

    public bool HasCloudRepository =>
        cloudRepository != null;

    public HybridSaveRepository(
        LocalSaveRepository localRepository,
        ISaveRepository cloudRepository = null)
    {
        this.localRepository =
            localRepository;

        this.cloudRepository =
            cloudRepository;
    }

    public void SetCloudRepository(
        ISaveRepository repository)
    {
        cloudRepository =
            repository;
    }

    public void ClearCloudRepository()
    {
        cloudRepository = null;
    }

    public Task<GameSaveData> LoadLocalAsync()
    {
        return localRepository.LoadAsync();
    }

    public async Task<GameSaveData> LoadCloudAsync()
    {
        if (cloudRepository == null)
            return null;

        return await cloudRepository.LoadAsync();
    }

    public async Task<GameSaveData> LoadAsync()
    {
        GameSaveData localData =
            await LoadLocalAsync();

        if (cloudRepository == null)
            return localData;

        GameSaveData cloudData =
            await LoadCloudAsync();

        return GameSaveMerger.Merge(
            localData,
            cloudData
        );
    }

    public async Task<bool> SaveAsync(
        GameSaveData saveData)
    {
        bool localSucceeded =
            await localRepository
                .SaveAsync(saveData);

        if (!localSucceeded)
        {
            Debug.LogError(
                "[HybridSave] Local 저장 실패."
            );
        }

        if (cloudRepository == null)
            return localSucceeded;

        bool cloudSucceeded =
            await cloudRepository
                .SaveAsync(saveData);

        if (!cloudSucceeded)
        {
            Debug.LogWarning(
                "[HybridSave] Cloud 저장 실패. " +
                "Local 저장 데이터는 유지됩니다."
            );
        }

        // 클라우드가 실패해도
        // 로컬 저장이 성공했으면 게임 저장은 성공으로 본다.
        return localSucceeded;
    }
}