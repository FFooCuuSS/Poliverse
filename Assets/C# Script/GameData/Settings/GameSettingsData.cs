using System;
using UnityEngine;

[Serializable]
public class GameSettingsData
{
    public int schemaVersion = 1;

    [Header("Audio")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Range(0f, 1f)]
    public float bgmVolume = 0.8f;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [Range(0f, 1f)]
    public float uiVolume = 1f;

    // 기기 및 오디오 장치마다 달라지므로 로컬 저장
    public int rhythmOffsetMs = 0;

    [Header("Display")]
    public FullScreenMode fullScreenMode =
        FullScreenMode.FullScreenWindow;

    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;

    public int targetFrameRate = 60;
    public int vSyncCount = 1;
    public int qualityLevel = 2;

    [Range(0.5f, 1.5f)]
    public float brightness = 1f;

    [Header("Controls")]
    [Range(0.5f, 2f)]
    public float swipeSensitivity = 1f;

    public bool vibrationEnabled = true;

    [Header("Gameplay")]
    [Range(0f, 1f)]
    public float screenShakeStrength = 1f;

    [Range(0f, 1f)]
    public float effectIntensity = 1f;

    public bool tutorialEnabled = true;

    [Header("Accessibility")]
    public bool reduceFlashing = false;

    [Range(0.75f, 1.5f)]
    public float textScale = 1f;

    public bool subtitlesEnabled = true;

    [Header("General")]
    public string languageCode = "ko";

    public static GameSettingsData CreateDefault()
    {
        GameSettingsData data =
            new GameSettingsData();

        Resolution currentResolution =
            Screen.currentResolution;

        if (currentResolution.width > 0)
            data.resolutionWidth = currentResolution.width;

        if (currentResolution.height > 0)
            data.resolutionHeight = currentResolution.height;

        data.qualityLevel =
            QualitySettings.GetQualityLevel();

        return data;
    }
}