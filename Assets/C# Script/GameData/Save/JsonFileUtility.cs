using System;
using System.IO;
using UnityEngine;

public static class JsonFileUtility
{
    public static bool TryLoad<T>(
        string filePath,
        out T data)
    {
        data = default;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Debug.LogError(
                "[JsonFileUtility] 파일 경로가 비어 있습니다."
            );

            return false;
        }

        if (!File.Exists(filePath))
            return false;

        try
        {
            string json =
                File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(json))
                return false;

            data = JsonUtility.FromJson<T>(json);

            return data != null;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[JsonFileUtility] Load 실패\n" +
                $"Path: {filePath}\n" +
                $"{exception}"
            );

            data = default;
            return false;
        }
    }

    public static bool TrySave<T>(
        string filePath,
        T data)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Debug.LogError(
                "[JsonFileUtility] 파일 경로가 비어 있습니다."
            );

            return false;
        }

        if (data == null)
        {
            Debug.LogError(
                "[JsonFileUtility] 저장 데이터가 null입니다."
            );

            return false;
        }

        try
        {
            string directory =
                Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string json =
                JsonUtility.ToJson(data, true);

            string temporaryPath =
                filePath + ".tmp";

            File.WriteAllText(
                temporaryPath,
                json
            );

            File.Copy(
                temporaryPath,
                filePath,
                true
            );

            File.Delete(temporaryPath);

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[JsonFileUtility] Save 실패\n" +
                $"Path: {filePath}\n" +
                $"{exception}"
            );

            return false;
        }
    }
}