using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 온도계 게이지의 시각적 움직임과, 두 구간(시스템 콜 / 플레이어 응답)의 진행·판정을 담당한다.
/// heatPattern에 라운드가 여러 개 등록되어 있으면, 리스트 순서대로 라운드를 이어서 재생한다.
///
/// [중요] 이 컨트롤러는 더 이상 Time.time을 쓰지 않는다. 음악(CSV) 타임라인과 동일하게
/// AudioSettings.dspTime(오디오 하드웨어 클럭) 기준으로 동작한다. BeginPattern()은
/// CSV에서 "PatternStart" 신호가 도착한 바로 그 프레임에 호출되어야 하며(Minigame_2_10.OnRhythmEvent
/// 참고), 그 순간의 dspTime이 곧 HeatPattern의 0초 기준점이 된다.
/// </summary>
public class TemperatureController : MonoBehaviour
{
    [Header("패턴 (인스펙터에서 조정)")]
    [SerializeField] private HeatPattern heatPattern;

    [Header("온도계 연출")]
    [SerializeField] private GameObject gauge;
    [SerializeField] private float moveAmount = 10f; // 레벨 1당 이동량
    [SerializeField] private float duration = 0.2f;

    [Header("입력 판정 윈도우 (초)")]
    [SerializeField] private float perfectWindow = 0.15f;
    [SerializeField] private float goodWindow = 0.5f;
    [SerializeField] private float hitWindow = 1f;

    private Vector3 startPos;

    private float logicalLevel;
    private readonly Queue<float> pendingLevels = new Queue<float>();
    private bool isAnimatingSteps;

    private bool playerPhaseRunning;
    // 음악 클럭(dspTime) 기준, 플레이어 페이즈가 시작된 절대 시각
    private double playerPhaseStartDsp;

    private float[] inputTimes;
    private bool[] inputConsumed;
    private float[] inputIncrements;

    private int currentPatternIndex;

    public HeatPattern Pattern => heatPattern;
    public int CurrentPatternIndex => currentPatternIndex;
    public int TotalPatternCount => heatPattern != null ? heatPattern.PatternCount : 0;

    public void SetJudgementWindows(float perfect, float good, float hit)
    {
        perfectWindow = perfect;
        goodWindow = good;
        hitWindow = hit;
    }

    public event Action<int> OnSystemDrop;
    public event Action OnPlayerPhaseStarted;
    public event Action<MiniGameBase.JudgementResult, int> OnInputJudged;
    public event Action<int> OnRoundFinished;
    public event Action OnAllPatternsFinished;

    private void Awake()
    {
        startPos = gauge.transform.localPosition;
    }

    /// <summary>
    /// CSV에서 이 미니게임의 "PatternStart" 신호(cue)가 도착한 바로 그 프레임에 호출해야 한다.
    /// 호출되는 순간의 AudioSettings.dspTime이 HeatPattern 1라운드째의 0초 기준이 된다.
    /// </summary>
    public void BeginPattern()
    {
        StopAllCoroutines();
        playerPhaseRunning = false;
        logicalLevel = 0f;
        isAnimatingSteps = false;
        pendingLevels.Clear();
        currentPatternIndex = 0;
        gauge.transform.localPosition = startPos;

        StartCoroutine(RunSequence());
    }

    public void StopRound()
    {
        playerPhaseRunning = false;
        StopAllCoroutines();
        isAnimatingSteps = false;
        pendingLevels.Clear();
    }

    private IEnumerator RunSequence()
    {
        int totalPatterns = heatPattern != null ? heatPattern.PatternCount : 0;

        for (currentPatternIndex = 0; currentPatternIndex < totalPatterns; currentPatternIndex++)
        {
            yield return StartCoroutine(RunOnePattern(currentPatternIndex));
        }

        OnAllPatternsFinished?.Invoke();
    }

    private IEnumerator RunOnePattern(int patternIndex)
    {
        pendingLevels.Clear();
        logicalLevel = 0f;
        gauge.transform.localPosition = startPos;

        // ----- 0 ~ playerPhaseOffset : 시스템 콜 구간 (dspTime 폴링) -----
        // 1라운드째는 BeginPattern()이 CSV PatternStart 신호에 맞춰 호출된 바로 그 프레임이라
        // 이 시점 = 음악 타임라인과 정확히 일치. 2라운드부터는 직전 라운드 종료 직후 이어지므로
        // dspTime이 끊기지 않고 계속 흐른다(추가 드리프트 없음).
        double phaseStartDsp = AudioSettings.dspTime;
        float[] dropTimes = heatPattern != null ? heatPattern.GetSortedDropTimes(patternIndex) : new float[0];

        for (int i = 0; i < dropTimes.Length; i++)
        {
            while (AudioSettings.dspTime - phaseStartDsp < dropTimes[i])
                yield return null;

            RequestLevel(dropTimes[i]);
            OnSystemDrop?.Invoke(i);
        }

        float playerPhaseOffset = heatPattern != null ? heatPattern.PlayerPhaseOffset : 4f;

        while (AudioSettings.dspTime - phaseStartDsp < playerPhaseOffset)
            yield return null;

        // ----- playerPhaseOffset ~ 2*playerPhaseOffset : 플레이어 응답 구간 -----
        StartPlayerPhase(dropTimes, AudioSettings.dspTime);

        while (playerPhaseRunning)
            yield return null;
    }

    private void StartPlayerPhase(float[] dropTimes, double startDsp)
    {
        inputTimes = dropTimes;
        inputConsumed = new bool[dropTimes.Length];

        inputIncrements = new float[dropTimes.Length];
        for (int i = 0; i < dropTimes.Length; i++)
        {
            float prevTime = i > 0 ? dropTimes[i - 1] : 0f;
            inputIncrements[i] = dropTimes[i] - prevTime;
        }

        playerPhaseStartDsp = startDsp;
        playerPhaseRunning = true;

        OnPlayerPhaseStarted?.Invoke();
    }

    private void Update()
    {
        if (!playerPhaseRunning) return;
        CheckMisses();
    }

    private void CheckMisses()
    {
        if (!playerPhaseRunning) return;

        double now = AudioSettings.dspTime - playerPhaseStartDsp;

        for (int i = 0; i < inputTimes.Length; i++)
        {
            if (inputConsumed[i]) continue;
            if (now <= inputTimes[i] + hitWindow) continue;

            inputConsumed[i] = true;
            OnInputJudged?.Invoke(MiniGameBase.JudgementResult.Miss, i);

            if (!playerPhaseRunning) return;
        }

        CheckPlayerPhaseComplete();
    }

    public void OnSwipe()
    {
        if (!playerPhaseRunning) return;

        double now = AudioSettings.dspTime - playerPhaseStartDsp;

        int nearestIndex = -1;
        double bestDelta = double.MaxValue;

        for (int i = 0; i < inputTimes.Length; i++)
        {
            if (inputConsumed[i]) continue;

            double delta = Math.Abs(inputTimes[i] - now);
            if (delta > hitWindow) continue;

            if (delta < bestDelta)
            {
                bestDelta = delta;
                nearestIndex = i;
            }
        }

        MiniGameBase.JudgementResult judgement;

        if (nearestIndex < 0)
        {
            judgement = MiniGameBase.JudgementResult.Miss;
        }
        else if (bestDelta <= perfectWindow)
        {
            judgement = MiniGameBase.JudgementResult.Perfect;
        }
        else if (bestDelta <= goodWindow)
        {
            judgement = MiniGameBase.JudgementResult.Good;
        }
        else
        {
            judgement = MiniGameBase.JudgementResult.Miss;
        }

        if (nearestIndex >= 0)
        {
            inputConsumed[nearestIndex] = true;

            float newLevel = logicalLevel - inputIncrements[nearestIndex];
            RequestLevel(newLevel);
        }

        OnInputJudged?.Invoke(judgement, nearestIndex);

        if (playerPhaseRunning)
            CheckPlayerPhaseComplete();
    }

    private void CheckPlayerPhaseComplete()
    {
        if (!playerPhaseRunning) return;

        for (int i = 0; i < inputConsumed.Length; i++)
        {
            if (!inputConsumed[i]) return;
        }

        playerPhaseRunning = false;
        OnRoundFinished?.Invoke(currentPatternIndex);
    }

    private void RequestLevel(float targetLevel)
    {
        float maxLevel = heatPattern != null ? heatPattern.PlayerPhaseOffset : targetLevel;
        targetLevel = Mathf.Clamp(targetLevel, 0f, maxLevel);

        logicalLevel = targetLevel;
        pendingLevels.Enqueue(targetLevel);

        if (!isAnimatingSteps)
            StartCoroutine(ProcessLevelQueue());
    }

    private IEnumerator ProcessLevelQueue()
    {
        isAnimatingSteps = true;

        while (pendingLevels.Count > 0)
        {
            float targetLevel = pendingLevels.Dequeue();

            Vector3 from = gauge.transform.localPosition;
            Vector3 to = startPos + Vector3.down * moveAmount * targetLevel;

            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gauge.transform.localPosition = Vector3.Lerp(from, to, t / duration);
                yield return null;
            }

            gauge.transform.localPosition = to;
        }

        isAnimatingSteps = false;
    }
}