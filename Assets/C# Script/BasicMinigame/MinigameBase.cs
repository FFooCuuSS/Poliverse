using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MiniGameBase : MonoBehaviour
{
    public event Action OnSuccess;
    public event Action OnFail;

    public bool IsSuccess { get; protected set; }
    public bool IsInputLocked { get; protected set; } = false;


    // 내부에서 override할 수 있게 유지
    protected virtual float TimerDuration => 10f;
    protected virtual string MinigameExplain => "기본 미니게임 설명";
    protected RhythmManager rhythmManager;
    protected AudioSource sfxSource;
    private Dictionary<string, AudioClip> sfxCache = new Dictionary<string, AudioClip>();


    // 외부에서 읽을 수 있도록 public getter 제공
    public float GetTimerDuration => TimerDuration;
    public string GetMinigameExplain => MinigameExplain;

    protected virtual void Awake()
    {
        // 각 미니게임 prefab에 자동으로 AudioSource 생성
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    public virtual void StartGame()
    {
        IsSuccess = false;
        IsInputLocked = false;

        rhythmManager.LoadChart(gameObject.name);
        rhythmManager.StartSong();

        Debug.Log($"{gameObject.name} 게임 시작!");
        Debug.Log($"설명: {MinigameExplain}");
        Debug.Log($"타이머: {TimerDuration}초");
    }

    public virtual void ResetGame()
    {
        IsSuccess = false;
        OnSuccess = null;
        OnFail = null;
    }

    public virtual void BindRhythmManager(RhythmManager rm)
    {
        rhythmManager = rm;
        rhythmManager.OnEventTriggered += OnRhythmEvent;
        rhythmManager.OnPlayerJudged += OnJudgement;
    }

    // 리듬 이벤트 훅
    // RhythmManager → 미니게임 (타이밍 이벤트)
    public virtual void OnRhythmEvent(string action)
    {
        // 파생 미니게임에서 override
    }

    // 판정 훅
    // RhythmManager → 미니게임 (Perfect/Good/Miss)
    public virtual void OnJudgement(RhythmManager.RhythmJudgement judgement)
    {
        // 파생 미니게임에서 override
    }

    // 플레이어 입력 훅
    // 미니게임 내부 오브젝트 → 미니게임(Base)
    // 리듬매니저 호출은 PlayScene/Manager에서 수행
    public virtual void OnPlayerInput()
    {
        // 파생 미니게임에서 override
    }


    public virtual void Success()
    {
        if (IsSuccess) return;
        IsSuccess = true;

        Debug.Log($"{gameObject.name} 성공!");
        OnSuccess?.Invoke();

        StartCoroutine(LockInputTemporarily(3f));
    }

    public virtual void Fail()
    {
        if (IsSuccess) return;

        Debug.Log($"{gameObject.name} 실패!");
        OnFail?.Invoke();

        StartCoroutine(LockInputTemporarily(3f));
    }

    protected IEnumerator LockInputTemporarily(float duration)
    {
        IsInputLocked = true;
        yield return new WaitForSeconds(duration);
        IsInputLocked = false;
    }

    // SFX 재생 함수
    protected void PlaySFX(string clipName)
    {
        if (string.IsNullOrEmpty(clipName)) return;

        if (sfxCache.TryGetValue(clipName, out var clip))
        {
            sfxSource.PlayOneShot(clip);
            return;
        }

        clip = Resources.Load<AudioClip>($"SFX/{clipName}");

        if (clip == null)
        {
            Debug.LogWarning($"[MiniGameBase] SFX '{clipName}' not found.");
            return;
        }

        sfxCache[clipName] = clip;
        sfxSource.PlayOneShot(clip);
    }
}
