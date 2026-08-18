using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 온도계 게이지의 시각적 움직임과, 두 구간(시스템 콜 / 플레이어 응답)의 진행·판정을 담당한다.
/// heatPattern에 라운드가 여러 개 등록되어 있으면, 리스트 순서대로 라운드를 이어서 재생한다.
///
/// 라운드 1개(총 6초)는 다음과 같이 진행된다.
///  - 시스템 페이즈 (0초 ~ heatPattern.PlayerPhaseOffset) :
///      해당 라운드의 dropTimes에 지정된 시각마다 게이지가 한 단계씩 누적으로 내려간다(원위치로 복귀하지 않음).
///      예) dropTimes = [1, 2, 3] 이면 온도계가 총 3번 내려간다.
///
///  - 플레이어 페이즈 (heatPattern.PlayerPhaseOffset ~ +heatPattern.PlayerPhaseOffset, 총 6초 지점까지) :
///      시스템 페이즈와 동일한 상대 타이밍(=dropTimes)에 맞춰 OnSwipe()가 호출되어야 한다.
///      각 입력은 perfectWindow / goodWindow / hitWindow 기준으로 Perfect / Good / Miss로 판정되며,
///      판정 결과와 무관하게 입력 1회당 온도계가 한 단계씩 올라간다(시스템이 내려간 만큼만).
///
/// 한 라운드가 끝나면(모든 노드 판정 완료) 다음 라운드가 있는 경우 자동으로 이어서 재생되고,
/// 마지막 라운드까지 끝나면 OnAllPatternsFinished가 호출된다.
/// </summary>
public class TemperatureController : MonoBehaviour
{
    [Header("패턴 (인스펙터에서 조정)")]
    [SerializeField] private HeatPattern heatPattern;

    [Header("온도계 연출")]
    [SerializeField] private GameObject gauge;
    [SerializeField] private float moveAmount = 10f;
    [SerializeField] private float duration = 0.2f;

    [Header("입력 판정 윈도우 (초)")]
    [Tooltip("Minigame_2_10의 perfectWindowOverride 등과 동일한 값으로 맞춰서 사용하는 것을 권장")]
    [SerializeField] private float perfectWindow = 0.15f;
    [SerializeField] private float goodWindow = 0.5f;
    [SerializeField] private float hitWindow = 1f;

    private Vector3 startPos;

    // 현재 온도계가 startPos에서 몇 단계 내려가 있는지 (시스템 콜로 내려간 횟수 - 플레이어 입력으로 올라간 횟수)
    private int currentStepsDown;

    // 게이지 이동 애니메이션 요청 큐 (+1: 한 단계 내려감, -1: 한 단계 올라감)
    // 입력이 짧은 간격으로 연달아 들어와도 트윈이 겹치지 않도록 순차 처리한다.
    private readonly Queue<int> pendingSteps = new Queue<int>();
    private bool isAnimatingSteps;

    private bool playerPhaseRunning;
    private float playerPhaseStartTime;

    // 플레이어 페이즈 기준(0초 = 플레이어 페이즈 시작 시각)으로 환산된 목표 시각들
    private float[] inputTimes;
    private bool[] inputConsumed;

    // 현재 재생 중인 라운드(패턴) 인덱스
    private int currentPatternIndex;

    public HeatPattern Pattern => heatPattern;

    /// <summary>현재 재생 중인 라운드(패턴) 인덱스 (0-base).</summary>
    public int CurrentPatternIndex => currentPatternIndex;

    /// <summary>등록된 전체 라운드(패턴) 개수.</summary>
    public int TotalPatternCount => heatPattern != null ? heatPattern.PatternCount : 0;

    /// <summary>Minigame_2_10의 perfect/good/hit 윈도우 오버라이드 값과 동기화할 때 사용.</summary>
    public void SetJudgementWindows(float perfect, float good, float hit)
    {
        perfectWindow = perfect;
        goodWindow = good;
        hitWindow = hit;
    }

    /// <summary>시스템 페이즈에서 몇 번째(0-base) 박에 온도계가 내려갔는지 알려준다.</summary>
    public event Action<int> OnSystemDrop;

    /// <summary>플레이어 페이즈가 시작될 때 호출된다.</summary>
    public event Action OnPlayerPhaseStarted;

    /// <summary>플레이어 입력(또는 자동 미스)이 판정될 때마다 호출된다. (판정 결과, 몇 번째 노드인지)</summary>
    public event Action<MiniGameBase.JudgementResult, int> OnInputJudged;

    /// <summary>라운드(패턴) 하나의 모든 노드가 판정 완료되었을 때마다 호출된다. (몇 번째 라운드였는지)</summary>
    public event Action<int> OnRoundFinished;

    /// <summary>등록된 모든 라운드(패턴)가 순서대로 끝났을 때 한 번 호출된다.</summary>
    public event Action OnAllPatternsFinished;

    private void Awake()
    {
        startPos = gauge.transform.localPosition;
    }

    /// <summary>
    /// heatPattern에 등록된 라운드들을 처음(0번째)부터 순서대로 재생한다.
    /// Minigame_2_10.StartGame()에서 호출한다.
    /// </summary>
    public void BeginPattern()
    {
        StopAllCoroutines();
        playerPhaseRunning = false;
        currentStepsDown = 0;
        isAnimatingSteps = false;
        pendingSteps.Clear();
        currentPatternIndex = 0;
        gauge.transform.localPosition = startPos;

        StartCoroutine(RunSequence());
    }

    /// <summary>진행 중인 라운드를 즉시 종료한다(성공/실패가 확정된 직후 Minigame에서 호출).</summary>
    public void StopRound()
    {
        playerPhaseRunning = false;
        StopAllCoroutines();
        isAnimatingSteps = false;
        pendingSteps.Clear();
    }

    // heatPattern에 등록된 라운드를 0번째부터 순서대로 하나씩 재생하고, 전부 끝나면 OnAllPatternsFinished를 알린다.
    private IEnumerator RunSequence()
    {
        int totalPatterns = heatPattern != null ? heatPattern.PatternCount : 0;

        for (currentPatternIndex = 0; currentPatternIndex < totalPatterns; currentPatternIndex++)
        {
            yield return StartCoroutine(RunOnePattern(currentPatternIndex));
        }

        OnAllPatternsFinished?.Invoke();
    }

    // 라운드 하나(시스템 페이즈 -> 플레이어 페이즈)를 처음부터 끝까지 진행한다.
    private IEnumerator RunOnePattern(int patternIndex)
    {
        // 라운드가 바뀔 때마다 온도계를 원위치로 리셋한다.
        pendingSteps.Clear();
        currentStepsDown = 0;
        gauge.transform.localPosition = startPos;

        // ----- 0 ~ playerPhaseOffset : 시스템 콜 구간 -----
        float phaseStartTime = Time.time;
        float[] dropTimes = heatPattern != null ? heatPattern.GetSortedDropTimes(patternIndex) : new float[0];

        for (int i = 0; i < dropTimes.Length; i++)
        {
            float elapsed = Time.time - phaseStartTime;
            float wait = dropTimes[i] - elapsed;

            if (wait > 0f)
                yield return new WaitForSeconds(wait);

            RequestStep(+1);
            OnSystemDrop?.Invoke(i);
        }

        float playerPhaseOffset = heatPattern != null ? heatPattern.PlayerPhaseOffset : 3f;
        float remain = playerPhaseOffset - (Time.time - phaseStartTime);
        if (remain > 0f)
            yield return new WaitForSeconds(remain);

        // ----- playerPhaseOffset ~ 6초 : 플레이어 응답 구간 -----
        StartPlayerPhase(dropTimes);

        // 이 라운드의 모든 노드가 판정될 때까지(=playerPhaseRunning이 false가 될 때까지) 대기한 뒤
        // 다음 라운드로 넘어간다.
        while (playerPhaseRunning)
            yield return null;
    }

    private void StartPlayerPhase(float[] dropTimes)
    {
        inputTimes = dropTimes;
        inputConsumed = new bool[dropTimes.Length];

        playerPhaseStartTime = Time.time;
        playerPhaseRunning = true;

        OnPlayerPhaseStarted?.Invoke();
    }

    private void Update()
    {
        if (!playerPhaseRunning) return;

        CheckMisses();
    }

    // 판정 윈도우를 넘긴 노드를 자동으로 Miss 처리한다.
    private void CheckMisses()
    {
        if (!playerPhaseRunning) return;

        float now = Time.time - playerPhaseStartTime;

        for (int i = 0; i < inputTimes.Length; i++)
        {
            if (inputConsumed[i]) continue;
            if (now <= inputTimes[i] + hitWindow) continue;

            inputConsumed[i] = true;
            OnInputJudged?.Invoke(MiniGameBase.JudgementResult.Miss, i);

            if (!playerPhaseRunning) return; // 콜백 안에서 라운드가 종료됐을 수 있음
        }

        CheckPlayerPhaseComplete();
    }

    /// <summary>플레이어가 스와이프(입력)했을 때 호출한다. (ScoopDrag -> Minigame_2_10 경유)</summary>
    public void OnSwipe()
    {
        if (!playerPhaseRunning) return;

        float now = Time.time - playerPhaseStartTime;

        int nearestIndex = -1;
        float bestDelta = float.MaxValue;

        for (int i = 0; i < inputTimes.Length; i++)
        {
            if (inputConsumed[i]) continue;

            float delta = Mathf.Abs(inputTimes[i] - now);
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
            inputConsumed[nearestIndex] = true;

        // 판정 결과와 무관하게, 유효한 입력 1회당 온도계가 한 단계 올라간다.
        RequestStep(-1);

        OnInputJudged?.Invoke(judgement, nearestIndex);

        if (playerPhaseRunning)
            CheckPlayerPhaseComplete();
    }

    private void CheckPlayerPhaseComplete()
    {
        if (!playerPhaseRunning) return;

        for (int i = 0; i < inputConsumed.Length; i++)
        {
            if (!inputConsumed[i]) return; // 아직 판정되지 않은 노드가 남아있음
        }

        playerPhaseRunning = false;
        OnRoundFinished?.Invoke(currentPatternIndex);
    }

    // 게이지 이동 요청을 큐에 넣는다. (+1: 한 단계 내려감 / -1: 한 단계 올라감)
    // 이미 처리 중인 애니메이션이 있으면 큐에만 쌓아두고, 없으면 큐 처리 코루틴을 새로 시작한다.
    private void RequestStep(int direction)
    {
        pendingSteps.Enqueue(direction);

        if (!isAnimatingSteps)
            StartCoroutine(ProcessStepQueue());
    }

    // 큐에 쌓인 이동 요청을 하나씩 순차적으로 애니메이션 처리한다.
    // (동시에 여러 코루틴이 gauge.transform.localPosition을 건드려 애니메이션이 서로 충돌하는 것을 방지)
    private IEnumerator ProcessStepQueue()
    {
        isAnimatingSteps = true;

        while (pendingSteps.Count > 0)
        {
            int direction = pendingSteps.Dequeue();

            // 위로는 시스템이 내려간 만큼(currentStepsDown)만 올라갈 수 있다.
            if (direction < 0 && currentStepsDown <= 0)
                continue;

            Vector3 from = gauge.transform.localPosition;
            Vector3 to = from + (direction > 0 ? Vector3.down : Vector3.up) * moveAmount;

            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                gauge.transform.localPosition = Vector3.Lerp(from, to, t / duration);
                yield return null;
            }

            gauge.transform.localPosition = to;
            currentStepsDown += direction;
        }

        isAnimatingSteps = false;
    }
}