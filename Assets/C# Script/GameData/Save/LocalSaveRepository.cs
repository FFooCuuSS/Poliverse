using System.Threading.Tasks;

public class LocalSaveRepository : ISaveRepository
{
    public string RepositoryName =>
        "Local JSON";

    private readonly string filePath;

    public LocalSaveRepository(string filePath)
    {
        this.filePath = filePath;
    }

    public Task<GameSaveData> LoadAsync()
    {
        bool loaded =
            JsonFileUtility.TryLoad(
                filePath,
                out GameSaveData saveData
            );

        return Task.FromResult(
            loaded ? saveData : null
        );
    }

    public Task<bool> SaveAsync(
        GameSaveData saveData)
    {
        bool succeeded =
            JsonFileUtility.TrySave(
                filePath,
                saveData
            );

        return Task.FromResult(succeeded);
    }
}