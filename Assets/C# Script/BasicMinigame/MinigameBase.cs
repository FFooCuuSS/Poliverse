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
        event Action<string> OnEventTriggered;           // 차트 타이밍 신호
        event Action<JudgementResult> OnPlayerJudged;    // 판정 결과 브로드캐스트

        // 미니게임이 "입력했음"만 알리면, 매니저가 판정한다
        void ReceivePlayerInput(string action = null);
        int GetTotalNodeCount();
    }

    protected IRhythmManager rhythmManager;

    [Header("Score Settings")]
    [Tooltip("true면 RhythmManager의 Input/Tap/Hold/Swipe 판정을 점수로 사용한다. false면 개별 미니게임이 ReportManualSuccess/Fail로 직접 보고한다.")]
    [SerializeField] private bool useRhythmJudgementScore = true;

    [Tooltip("Use Rhythm Judgement Score가 false일 때 사용할 총 점수 대상 수. -1이면 실제 보고된 수를 총량으로 사용한다.")]
    [SerializeField] private int manualTotalNodeCount = -1;

    [SerializeField] private bool printScoreDebugOnEnd = true;

    // 점수 집계 방식
    // true : RhythmManager의 Input/Tap/Hold/Swipe 판정을 점수로 사용
    // false : 개별 미니게임이 ReportManualSuccess/Fail로 직접 보고
    protected virtual bool UseRhythmJudgementScore => useRhythmJudgementScore;

    protected virtual int ManualTotalNodeCount => manualTotalNodeCount;

    // true면 처리되지 않은 노드는 최종 Miss로 보정
    protected virtual bool AutoFillRemainingAsMiss => true;

    // 미니게임당 점수판 집계용
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
    private int scoreMissCount;

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
        InitializeScoreTotalNodeCount();
    }

    protected virtual void InitializeScoreTotalNodeCount()
    {
        if (UseRhythmJudgementScore)
        {
            if (rhythmManager != null)
                totalNodeCount = rhythmManager.GetTotalNodeCount();
            else
                totalNodeCount = 0;

            return;
        }

        if (ManualTotalNodeCount >= 0)
            totalNodeCount = ManualTotalNodeCount;
        else
            totalNodeCount = 0;
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
        scoreMissCount = 0;

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

        switch (action)
        {
            case "Tap":
                break;

            case "Hold":
                break;

            case "Swipe":
                break;
        }
    }

    // RhythmManager → 미니게임 (Perfect/Good/Miss)
    public virtual void OnJudgement(JudgementResult judgement)
    {
        // Manual 모드에서는 RhythmManager 판정을 점수로 쓰지 않는다.
        // 따라서 여기서 Miss가 와도 Base 점수에는 절대 반영되지 않는다.
        if (!UseRhythmJudgementScore) return;

        bool counted = AddJudgementCount(judgement);

        if (counted)
            Debug.Log($"mb counted: {judgement}");
        else
            Debug.Log($"mb ignored: {judgement}");
    }

    protected virtual bool AddJudgementCount(JudgementResult judgement)
    {
        if (scoreFinalized) return false;

        // 이번 수정의 핵심:
        // Miss는 여기서 절대 직접 집계하지 않는다.
        // 최종 Miss는 FinalizeScoreSession()에서
        // totalNodeCount - perfectCount - goodCount로 계산한다.
        if (judgement == JudgementResult.Miss)
            return false;

        // 총 노드 수가 정해져 있다면 Perfect + Good도 총 노드 수를 넘지 못하게 막는다.
        // 광클로 Good/Perfect가 과하게 들어오는 상황에 대한 최소 방어.
        int currentHitCount = perfectCount + goodCount;

        if (totalNodeCount > 0 && currentHitCount >= totalNodeCount)
            return false;

        switch (judgement)
        {
            case JudgementResult.Perfect:
                perfectCount++;
                return true;

            case JudgementResult.Good:
                goodCount++;
                return true;
        }

        return false;
    }

    public virtual ScoreResult FinalizeScoreSession()
    {
        if (!scoreFinalized)
        {
            scoreFinalized = true;

            if (UseRhythmJudgementScore)
            {
                FinalizeRhythmScore();
            }
            else
            {
                FinalizeManualScore();
            }

            if (printScoreDebugOnEnd)
            {
                int judgedTotal = perfectCount + goodCount + scoreMissCount;

                Debug.Log(
                    $"[MiniGame Score] {gameObject.name}\n" +
                    $"- Use Rhythm  : {UseRhythmJudgementScore}\n" +
                    $"- ManualTotal : {ManualTotalNodeCount}\n" +
                    $"- Total Nodes : {totalNodeCount}\n" +
                    $"- Perfect     : {perfectCount}\n" +
                    $"- Good        : {goodCount}\n" +
                    $"- Miss        : {scoreMissCount}\n" +
                    $"- JudgedTotal : {judgedTotal}\n" +
                    $"- Manual S/F  : {manualSuccessCount}/{manualFailCount}"
                );
            }
        }

        return new ScoreResult(totalNodeCount, perfectCount, goodCount, scoreMissCount);
    }

    private void FinalizeRhythmScore()
    {
        int hitTotal = perfectCount + goodCount;

        // Rhythm 모드에서 totalNodeCount가 없다면 Miss를 계산할 기준이 없다.
        // 이 경우에는 실제 성공 판정 수를 총량으로 삼는다.
        if (totalNodeCount <= 0)
            totalNodeCount = hitTotal;

        // Miss는 직접 집계하지 않고 최종 계산한다.
        if (AutoFillRemainingAsMiss)
            scoreMissCount = Mathf.Max(0, totalNodeCount - hitTotal);
        else
            scoreMissCount = 0;
    }

    private void FinalizeManualScore()
    {
        // Manual 모드에서는 ReportManualSuccess/Fail만 점수에 반영한다.
        // RhythmManager에서 Miss가 와도 OnJudgement에서 return되므로 여기까지 영향을 주지 않는다.

        perfectCount = manualSuccessCount;
        goodCount = 0;

        int manualReportedTotal = manualSuccessCount + manualFailCount;

        // ManualTotalNodeCount가 지정되지 않았다면 실제 보고된 수를 총량으로 삼는다.
        if (totalNodeCount <= 0)
            totalNodeCount = manualReportedTotal;

        if (AutoFillRemainingAsMiss)
        {
            // 총량 기준으로 성공하지 못한 나머지를 Miss 처리.
            // manualFailCount + 미보고 항목까지 포함된다.
            scoreMissCount = Mathf.Max(0, totalNodeCount - perfectCount - goodCount);
        }
        else
        {
            // 자동 보정이 꺼져 있다면 명시적으로 보고된 실패만 Miss 처리.
            scoreMissCount = manualFailCount;
        }
    }

    // 미니게임 내부 오브젝트 → 미니게임(Base)
    // 여기서는 판정을 하지 말고, "입력했다"만 매니저에 전달
    public virtual void OnPlayerInput(string action = null)
    {
        if (IsInputLocked) return;
        rhythmManager?.ReceivePlayerInput(action);
    }

    // input 판정 안 쓰는 미니게임들 점수 처리
    public virtual void ReportManualSuccess()
    {
        if (scoreFinalized) return;

        if (UseRhythmJudgementScore)
        {
            Debug.LogWarning($"[MiniGameBase] ReportManualSuccess ignored because UseRhythmJudgementScore is true: {gameObject.name}");
            return;
        }

        int reportedTotal = manualSuccessCount + manualFailCount;

        if (totalNodeCount > 0 && reportedTotal >= totalNodeCount)
        {
            Debug.LogWarning($"[MiniGameBase] Extra manual success ignored: {gameObject.name}");
            return;
        }

        manualSuccessCount++;
    }

    public virtual void ReportManualFail()
    {
        if (scoreFinalized) return;

        if (UseRhythmJudgementScore)
        {
            Debug.LogWarning($"[MiniGameBase] ReportManualFail ignored because UseRhythmJudgementScore is true: {gameObject.name}");
            return;
        }

        int reportedTotal = manualSuccessCount + manualFailCount;

        if (totalNodeCount > 0 && reportedTotal >= totalNodeCount)
        {
            Debug.LogWarning($"[MiniGameBase] Extra manual fail ignored: {gameObject.name}");
            return;
        }

        manualFailCount++;
    }

    // 수동 미니게임에서 총 대상 수가 런타임에 정해지는 경우 사용
    // 예: 실제 생성된 오브젝트 수를 센 뒤 StartGame 이후에 세팅
    protected virtual void SetRuntimeTotalNodeCount(int count)
    {
        if (scoreFinalized) return;
        totalNodeCount = Mathf.Max(0, count);
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