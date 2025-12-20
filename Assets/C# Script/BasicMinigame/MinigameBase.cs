using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class MiniGameBase : MonoBehaviour
{
    public event Action OnSuccess;
    public event Action OnFail;

    public bool IsSuccess { get; protected set; }
    public bool IsInputLocked { get; protected set; } = false;

    protected virtual float TimerDuration => 10f;
    protected virtual string MinigameExplain => "기본 미니게임 설명";

    protected AudioSource sfxSource;
    private readonly Dictionary<string, AudioClip> sfxCache = new Dictionary<string, AudioClip>();

    public float GetTimerDuration => TimerDuration;
    public string GetMinigameExplain => MinigameExplain;

    // 여기서 판정 타입을 표준화 (외부 클래스 RhythmManager에 의존 X)
    public enum JudgementResult { Perfect, Good, Miss }

    // 리듬 매니저 계약(인터페이스)
    public interface IRhythmManager
    {
        event Action<string> OnEventTriggered;          // 차트 타이밍 신호
        event Action<JudgementResult> OnPlayerJudged;   // 판정 결과 브로드캐스트

        // 미니게임이 "입력했음"만 알리면, 매니저가 판정한다
        void ReceivePlayerInput(string action = null);
    }

    protected IRhythmManager rhythmManager;

    protected virtual void Awake()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    protected virtual void OnDestroy()
    {
        // 누수 방지: 파괴될 때 구독 해제
        BindRhythmManager(null);
    }

    public virtual void StartGame()
    {
        IsSuccess = false;
        IsInputLocked = false;

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

    public virtual void BindRhythmManager(IRhythmManager rm)
    {
        // 1) 기존 구독 해제
        if (rhythmManager != null)
        {
            rhythmManager.OnEventTriggered -= OnRhythmEvent;
            rhythmManager.OnPlayerJudged -= OnJudgement;
        }

        // 2) null이면 여기서 종료(=언바인드)
        rhythmManager = rm;
        if (rhythmManager == null)
        {
            Debug.Log($"[MiniGameBase] Unbound IRhythmManager from {gameObject.name}");
            return;
        }

        // 3) 새 구독 등록
        rhythmManager.OnEventTriggered += OnRhythmEvent;
        rhythmManager.OnPlayerJudged += OnJudgement;

        Debug.Log($"[MiniGameBase] Bound IRhythmManager to {gameObject.name}");
    }

    // RhythmManager → 미니게임 (타이밍 이벤트)
    public virtual void OnRhythmEvent(string action)
    {
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");
        
        // 이건 나중에 개별 미니게임에서 override하는 형태로
        switch (action)
        {
            case "Tap":
                //ShowTapPrompt();
                break;

            case "Hold":
                //ShowHoldPrompt();
                break;

            case "Swipe":
                //ShowSwipePrompt();
                break;
        }
    }

    // RhythmManager → 미니게임 (Perfect/Good/Miss)
    public virtual void OnJudgement(JudgementResult judgement)
    {
        Debug.Log($"{judgement}");
    }

    // 미니게임 내부 오브젝트 → 미니게임(Base)
    // 여기서는 판정을 하지 말고, "입력했다"만 매니저에 전달
    public virtual void OnPlayerInput(string action = null)
    {
        if (IsInputLocked) return;
        rhythmManager?.ReceivePlayerInput(action);
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
