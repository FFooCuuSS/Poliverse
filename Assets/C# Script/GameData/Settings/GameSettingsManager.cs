using System;
using System.IO;
using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    public event Action<GameSettingsData>
        OnSettingsChanged;

    public GameSettingsData Data { get; private set; }

    public bool IsInitialized => Data != null;

    private string settingsFilePath;

    public void Initialize()
    {
        settingsFilePath = Path.Combine(
            Application.persistentDataPath,
            "settings.json"
        );

        bool loaded =
            JsonFileUtility.TryLoad(
                settingsFilePath,
                out GameSettingsData loadedData
            );

        Data = loaded && loadedData != null
            ? loadedData
            : GameSettingsData.CreateDefault();

        ValidateData();
        ApplySystemSettings();

        if (!loaded)
            SaveNow();
    }

    private void ValidateData()
    {
        if (Data == null)
            Data = GameSettingsData.CreateDefault();

        Data.masterVolume =
            Mathf.Clamp01(Data.masterVolume);

        Data.bgmVolume =
            Mathf.Clamp01(Data.bgmVolume);

        Data.sfxVolume =
            Mathf.Clamp01(Data.sfxVolume);

        Data.uiVolume =
            Mathf.Clamp01(Data.uiVolume);

        Data.rhythmOffsetMs =
            Mathf.Clamp(Data.rhythmOffsetMs, -500, 500);

        Data.resolutionWidth =
            Mathf.Max(640, Data.resolutionWidth);

        Data.resolutionHeight =
            Mathf.Max(360, Data.resolutionHeight);

        Data.targetFrameRate =
            Mathf.Clamp(Data.targetFrameRate, 30, 240);

        Data.vSyncCount =
            Mathf.Clamp(Data.vSyncCount, 0, 4);

        int qualityCount =
            QualitySettings.names.Length;

        if (qualityCount > 0)
        {
            Data.qualityLevel = Mathf.Clamp(
                Data.qualityLevel,
                0,
                qualityCount - 1
            );
        }

        Data.brightness =
            Mathf.Clamp(Data.brightness, 0.5f, 1.5f);

        Data.swipeSensitivity =
            Mathf.Clamp(
                Data.swipeSensitivity,
                0.5f,
                2f
            );

        Data.screenShakeStrength =
            Mathf.Clamp01(Data.screenShakeStrength);

        Data.effectIntensity =
            Mathf.Clamp01(Data.effectIntensity);

        Data.textScale =
            Mathf.Clamp(
                Data.textScale,
                0.75f,
                1.5f
            );

        if (string.IsNullOrWhiteSpace(
                Data.languageCode))
        {
            Data.languageCode = "ko";
        }
    }

    public void ApplySystemSettings()
    {
        if (Data == null)
            return;

        Screen.SetResolution(
            Data.resolutionWidth,
            Data.resolutionHeight,
            Data.fullScreenMode
        );

        QualitySettings.vSyncCount =
            Data.vSyncCount;

        Application.targetFrameRate =
            Data.targetFrameRate;

        if (QualitySettings.names.Length > 0)
        {
            QualitySettings.SetQualityLevel(
                Data.qualityLevel,
                true
            );
        }
    }

    private void NotifyChanged(
        bool applySystemSettings = false)
    {
        ValidateData();

        if (applySystemSettings)
            ApplySystemSettings();

        OnSettingsChanged?.Invoke(Data);
    }

    public void SetMasterVolume(float value)
    {
        EnsureInitialized();
        Data.masterVolume = value;
        NotifyChanged();
    }

    public void SetBgmVolume(float value)
    {
        EnsureInitialized();
        Data.bgmVolume = value;
        NotifyChanged();
    }

    public void SetSfxVolume(float value)
    {
        EnsureInitialized();
        Data.sfxVolume = value;
        NotifyChanged();
    }

    public void SetUiVolume(float value)
    {
        EnsureInitialized();
        Data.uiVolume = value;
        NotifyChanged();
    }

    public void SetRhythmOffset(int milliseconds)
    {
        EnsureInitialized();
        Data.rhythmOffsetMs = milliseconds;
        NotifyChanged();
    }

    public void SetVibration(bool enabled)
    {
        EnsureInitialized();
        Data.vibrationEnabled = enabled;
        NotifyChanged();
    }

    public void SetSwipeSensitivity(float value)
    {
        EnsureInitialized();
        Data.swipeSensitivity = value;
        NotifyChanged();
    }

    public void SetScreenShake(float value)
    {
        EnsureInitialized();
        Data.screenShakeStrength = value;
        NotifyChanged();
    }

    public void SetEffectIntensity(float value)
    {
        EnsureInitialized();
        Data.effectIntensity = value;
        NotifyChanged();
    }

    public void SetReduceFlashing(bool enabled)
    {
        EnsureInitialized();
        Data.reduceFlashing = enabled;
        NotifyChanged();
    }

    public void SetTextScale(float value)
    {
        EnsureInitialized();
        Data.textScale = value;
        NotifyChanged();
    }

    public void SetSubtitles(bool enabled)
    {
        EnsureInitialized();
        Data.subtitlesEnabled = enabled;
        NotifyChanged();
    }

    public void SetLanguage(string languageCode)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            Debug.LogWarning(
                "[Settings] 언어 코드가 비어 있습니다."
            );

            return;
        }

        Data.languageCode = languageCode;
        NotifyChanged();
    }

    public void SetResolution(
        int width,
        int height,
        FullScreenMode screenMode)
    {
        EnsureInitialized();

        Data.resolutionWidth = width;
        Data.resolutionHeight = height;
        Data.fullScreenMode = screenMode;

        NotifyChanged(true);
    }

    public void SetFrameRate(
        int frameRate,
        int vSyncCount)
    {
        EnsureInitialized();

        Data.targetFrameRate = frameRate;
        Data.vSyncCount = vSyncCount;

        NotifyChanged(true);
    }

    public void SetQualityLevel(int qualityLevel)
    {
        EnsureInitialized();

        Data.qualityLevel = qualityLevel;
        NotifyChanged(true);
    }

    public void SaveNow()
    {
        if (Data == null)
            return;

        if (string.IsNullOrWhiteSpace(
                settingsFilePath))
        {
            Debug.LogWarning(
                "[Settings] 초기화 전에 저장을 요청했습니다."
            );

            return;
        }

        JsonFileUtility.TrySave(
            settingsFilePath,
            Data
        );
    }

    public void ResetToDefault()
    {
        Data = GameSettingsData.CreateDefault();

        NotifyChanged(true);
        SaveNow();
    }

    private void EnsureInitialized()
    {
        if (Data == null)
            Initialize();
    }
}