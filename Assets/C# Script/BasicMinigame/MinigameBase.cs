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


    public virtual float perfectWindowOverride => 0.1f;
    public virtual float goodWindowOverride => 0.3f;
    public virtual float hitWindowOverride => 1f;


    // 여기서 판정 타입을 표준화 (외부 클래스 RhythmManager에 의존 X)
    public enum JudgementResult { Perfect, Good, Miss }

    // 리듬 매니저 계약(인터페이스)
    public interface IRhythmManager
    {
        event Action<string> OnEventTriggered;          // 차트 타이밍 신호
        event Action<JudgementResult> OnPlayerJudged;   // 판정 결과 브로드캐스트

        // 미니게임이 "입력했음"만 알리면, 매니저가 판정한다
        void ReceivePlayerInput(string action = null);
        int GetTotalNodeCount();
    }

    protected IRhythmManager rhythmManager;


    [Header("Score")]
    [SerializeField] private bool printScoreDebugOnEnd = true;
    // 미니게임당 점수판 집계용 (미니게임 하나 끝난 후 uimanager가 읽어갈 것임)
    public struct ScoreResult
    {
        public int totalNode;
        public int perfect;
        public int good;
        public int miss;

        public ScoreResult(int totalNode, int perfect, int good, int miss)
        {
            this.totalNode = totalNode;
            this.perfect = perfect;
            this.good = good;
            this.miss = miss;
        }
    }
    private int totalNodeCount;
    private int perfectCount;
    private int goodCount;
    private int missCount;

    private int manualSuccessCount;
    private int manualFailCount;

    private bool scoreFinalized = false;


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

        ResetScoreSession();

        if (rhythmManager != null)
            totalNodeCount = rhythmManager.GetTotalNodeCount();
    }

    public virtual void ResetGame()
    {
        IsSuccess = false;
        OnSuccess = null;
        OnFail = null;

        ResetScoreSession();
    }

    protected virtual void ResetScoreSession()
    {
        totalNodeCount = 0;
        perfectCount = 0;
        goodCount = 0;
        missCount = 0;

        manualSuccessCount = 0;
        manualFailCount = 0;

        scoreFinalized = false;
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
        AddJudgementCount(judgement);
        Debug.Log($"{judgement}");
    }
    protected virtual void AddJudgementCount(JudgementResult judgement)
    {
        if (scoreFinalized) return;

        switch (judgement)
        {
            case JudgementResult.Perfect:
                perfectCount++;
                break;

            case JudgementResult.Good:
                goodCount++;
                break;

            case JudgementResult.Miss:
                missCount++;
                break;
        }
    }
    public virtual ScoreResult FinalizeScoreSession()
    {
        if (!scoreFinalized)
        {
            scoreFinalized = true;

            perfectCount += manualSuccessCount;
            missCount += manualFailCount;

            int judgedTotal = perfectCount + goodCount + missCount;

            if (totalNodeCount <= 0)
                totalNodeCount = judgedTotal;

            if (printScoreDebugOnEnd)
            {
                Debug.Log(
                    $"[MiniGame Score] {gameObject.name}\n" +
                    $"- Total Nodes : {totalNodeCount}\n" +
                    $"- Perfect     : {perfectCount}\n" +
                    $"- Good        : {goodCount}\n" +
                    $"- Miss        : {missCount}\n" +
                    $"- JudgedTotal : {judgedTotal}\n" +
                    $"- Manual S/F  : {manualSuccessCount}/{manualFailCount}"
                );
            }
        }

        return new ScoreResult(totalNodeCount, perfectCount, goodCount, missCount);
    }

    // 미니게임 내부 오브젝트 → 미니게임(Base)
    // 여기서는 판정을 하지 말고, "입력했다"만 매니저에 전달
    public virtual void OnPlayerInput(string action = null)
    {
        if (IsInputLocked) return;
        rhythmManager?.ReceivePlayerInput(action);
    }

    // input 판정 안쓰는 미니게임들 점수 처리
    public virtual void ReportManualSuccess()
    {
        if (scoreFinalized) return;
        manualSuccessCount++;
    }

    public virtual void ReportManualFail()
    {
        if (scoreFinalized) return;
        manualFailCount++;
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
