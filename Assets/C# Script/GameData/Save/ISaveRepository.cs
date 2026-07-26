using System.Threading.Tasks;

public interface ISaveRepository
{
    string RepositoryName { get; }

    Task<GameSaveData> LoadAsync();

    Task<bool> SaveAsync(
        GameSaveData saveData
    );

    // FIREBASE-LATER:
    // FirebaseSaveRepository가 같은 인터페이스를 구현한다.
    //
    // HybridSaveRepository는 내부에
    // LocalSaveRepository + FirebaseSaveRepository를 가진다.
}