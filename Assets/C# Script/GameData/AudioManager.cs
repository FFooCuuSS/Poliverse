using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup uiMixerGroup;

    [Header("Exposed Parameter Names")]
    [SerializeField]
    private string masterVolumeParameter =
        "MasterVolume";

    [SerializeField]
    private string bgmVolumeParameter =
        "BGMVolume";

    [SerializeField]
    private string sfxVolumeParameter =
        "SFXVolume";

    [SerializeField]
    private string uiVolumeParameter =
        "UIVolume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;

    private GameSettingsManager settingsManager;

    public AudioMixerGroup BgmMixerGroup =>
        bgmMixerGroup;

    public bool IsInitialized { get; private set; }

    public void Initialize(
        GameSettingsManager targetSettingsManager)
    {
        if (IsInitialized)
            return;

        settingsManager = targetSettingsManager;

        CreateMissingAudioSources();

        if (settingsManager != null)
        {
            settingsManager.OnSettingsChanged +=
                ApplySettings;

            ApplySettings(settingsManager.Data);
        }
        else
        {
            Debug.LogWarning(
                "[Audio] SettingsManager가 없습니다."
            );
        }

        IsInitialized = true;
    }

    private void CreateMissingAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = CreateAudioSource(
                "BGM Source",
                bgmMixerGroup
            );

            bgmSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = CreateAudioSource(
                "SFX Source",
                sfxMixerGroup
            );
        }

        if (uiSource == null)
        {
            uiSource = CreateAudioSource(
                "UI Source",
                uiMixerGroup
            );
        }
    }

    private AudioSource CreateAudioSource(
        string sourceName,
        AudioMixerGroup mixerGroup)
    {
        GameObject sourceObject =
            new GameObject(sourceName);

        sourceObject.transform.SetParent(
            transform,
            false
        );

        AudioSource source =
            sourceObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.outputAudioMixerGroup = mixerGroup;

        return source;
    }

    public void ApplySettings(
        GameSettingsData settings)
    {
        if (settings == null)
            return;

        if (audioMixer == null)
        {
            Debug.LogWarning(
                "[Audio] AudioMixer가 할당되지 않았습니다."
            );

            return;
        }

        SetMixerVolume(
            masterVolumeParameter,
            settings.masterVolume
        );

        SetMixerVolume(
            bgmVolumeParameter,
            settings.bgmVolume
        );

        SetMixerVolume(
            sfxVolumeParameter,
            settings.sfxVolume
        );

        SetMixerVolume(
            uiVolumeParameter,
            settings.uiVolume
        );
    }

    private void SetMixerVolume(
        string parameterName,
        float linearValue)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
            return;

        bool succeeded = audioMixer.SetFloat(
            parameterName,
            LinearToDecibel(linearValue)
        );

        if (!succeeded)
        {
            Debug.LogWarning(
                $"[Audio] AudioMixer 파라미터를 찾지 못했습니다: " +
                $"{parameterName}"
            );
        }
    }

    private float LinearToDecibel(float value)
    {
        value = Mathf.Clamp01(value);

        if (value <= 0.0001f)
            return -80f;

        return Mathf.Log10(value) * 20f;
    }

    public void PlayBgm(
        AudioClip clip,
        bool loop = true)
    {
        if (clip == null || bgmSource == null)
            return;

        if (bgmSource.clip == clip &&
            bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        if (bgmSource == null)
            return;

        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayUiSound(AudioClip clip)
    {
        if (clip == null || uiSource == null)
            return;

        uiSource.PlayOneShot(clip);
    }

    private void OnDestroy()
    {
        if (settingsManager != null)
        {
            settingsManager.OnSettingsChanged -=
                ApplySettings;
        }
    }
}