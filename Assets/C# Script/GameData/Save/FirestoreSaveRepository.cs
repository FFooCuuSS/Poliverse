using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

public class FirestoreSaveRepository
    : ISaveRepository
{
    public string RepositoryName =>
        "Firebase Firestore";

    private readonly FirebaseFirestore firestore;
    private readonly string uid;

    public FirestoreSaveRepository(
        string uid)
    {
        if (string.IsNullOrWhiteSpace(uid))
        {
            throw new ArgumentException(
                "Firebase UID가 비어 있습니다.",
                nameof(uid)
            );
        }

        this.uid = uid;

        firestore =
            FirebaseFirestore.DefaultInstance;
    }

    private DocumentReference GetSaveDocument()
    {
        return firestore
            .Collection("users")
            .Document(uid)
            .Collection("saves")
            .Document("main");
    }

    public async Task<GameSaveData> LoadAsync()
    {
        try
        {
            DocumentSnapshot snapshot =
                await GetSaveDocument()
                    .GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Debug.Log(
                    $"[FirestoreSave] 클라우드 세이브 없음 | " +
                    $"UID: {uid}"
                );

                return null;
            }

            bool hasJson =
                snapshot.TryGetValue(
                    "json",
                    out string json
                );

            if (!hasJson ||
                string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning(
                    "[FirestoreSave] json 필드가 없습니다."
                );

                return null;
            }

            GameSaveData saveData =
                JsonUtility
                    .FromJson<GameSaveData>(
                        json
                    );

            if (saveData == null)
            {
                Debug.LogError(
                    "[FirestoreSave] JSON 변환 결과가 null입니다."
                );

                return null;
            }

            Debug.Log(
                $"[FirestoreSave] Load 성공 | " +
                $"UID: {uid} | " +
                $"Revision: {saveData.revision}"
            );

            return saveData;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[FirestoreSave] Load 실패\n" +
                $"UID: {uid}\n" +
                $"{exception}"
            );

            return null;
        }
    }

    public async Task<bool> SaveAsync(
        GameSaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError(
                "[FirestoreSave] 저장 데이터가 null입니다."
            );

            return false;
        }

        try
        {
            string json =
                JsonUtility.ToJson(
                    saveData,
                    false
                );

            Dictionary<string, object> document =
                new Dictionary<string, object>
                {
                    {
                        "json",
                        json
                    },
                    {
                        "ownerUid",
                        uid
                    },
                    {
                        "schemaVersion",
                        saveData.schemaVersion
                    },
                    {
                        "revision",
                        saveData.revision
                    },
                    {
                        "lastUpdatedUtc",
                        saveData.lastUpdatedUtc ?? ""
                    }
                };

            await GetSaveDocument()
                .SetAsync(document);

            Debug.Log(
                $"[FirestoreSave] Save 성공 | " +
                $"UID: {uid} | " +
                $"Revision: {saveData.revision}"
            );

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[FirestoreSave] Save 실패\n" +
                $"UID: {uid}\n" +
                $"{exception}"
            );

            return false;
        }
    }
}