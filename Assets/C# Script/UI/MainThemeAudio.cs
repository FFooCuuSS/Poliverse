using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MainThemeAudio : MonoBehaviour
{
    [Header("Clip")]
    public AudioClip clip;

    [Header("Start Fade In")]
    [Range(0f, 1f)] public float startTargetVolume = 1f;
    public float fadeInDuration = 1f;

    [Header("Manual Loop (clip length + extra seconds)")]
    public float extraLoopDelay = 5f;

    private AudioSource _audio;
    private Coroutine _volumeRoutine;
    private Coroutine _loopRoutine;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.loop = false; // 길이+5f 후 재시작이므로 AudioSource loop는 끔
    }

    void Start()
    {
        if (clip == null)
        {
            Debug.LogWarning("[MainThemeAudio] AudioClip is not assigned.");
            return;
        }

        _audio.clip = clip;

        // 시작 시 볼륨 0에서 시작해서 페이드인
        _audio.volume = 0f;
        _audio.Play();

        StartFadeIn();
        StartManualLoop();
    }

    private void StartFadeIn()
    {
        if (_volumeRoutine != null) StopCoroutine(_volumeRoutine);
        _volumeRoutine = StartCoroutine(CoFadeVolume(_audio.volume, startTargetVolume, fadeInDuration));
    }

    private void StartManualLoop()
    {
        if (_loopRoutine != null) StopCoroutine(_loopRoutine);
        _loopRoutine = StartCoroutine(CoManualLoop());
    }

    private IEnumerator CoManualLoop()
    {
        // clip.length + 5f 기다렸다가 다시 재생을 무한 반복
        while (true)
        {
            float wait = (clip != null ? clip.length : 0f) + extraLoopDelay;
            if (wait < 0f) wait = 0f;

            yield return new WaitForSeconds(wait);

            if (clip == null) yield break;

            // 이미 재생 중이어도 정확히 “길이+5f마다” 다시 시작하고 싶으면 아래처럼 강제 리스타트
            _audio.Stop();
            _audio.time = 0f;
            _audio.Play();
        }
    }

    /// <summary>
    /// 목표 볼륨으로 0.2초 동안 부드럽게 변경
    /// </summary>
    public void SetVolumeSmooth(float targetVolume)
    {
        targetVolume = Mathf.Clamp01(targetVolume);

        if (_volumeRoutine != null) StopCoroutine(_volumeRoutine);
        _volumeRoutine = StartCoroutine(CoFadeVolume(_audio.volume, targetVolume, 0.2f));
    }

    private IEnumerator CoFadeVolume(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            _audio.volume = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // 일시정지(Time.timeScale=0)에도 자연스럽게 움직이게
            float a = Mathf.Clamp01(t / duration);
            _audio.volume = Mathf.Lerp(from, to, a);
            yield return null;
        }

        _audio.volume = to;
    }
}