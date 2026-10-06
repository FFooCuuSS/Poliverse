using System.Collections;
using UnityEngine;

public class Minigame_1_2 : MiniGameBase, IPracticeDemoInput
{
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;

    protected override float TimerDuration => 15f;
    protected override string MinigameTitle => "수갑 채우기";
    protected override string MinigameExplain => "왼쪽에서 수갑이 채워져 타이밍을 알려줍니다";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "동일한 타이밍에 오른손에 수갑을 드래그하여 채우세요."
    };
    [Header("Demo")]
    [SerializeField] private float demoHoldSeconds = 0.25f;  // 채워진 모습 보여주는 시간
    [SerializeField] private float demoFadeSeconds = 0.25f; // 사라지는 시간

    [Header("Sequence Controller")]
    [SerializeField] private HandcuffSequenceController sequence;

    [Header("Round Objects (2 cuffs used in this minigame)")]
    [SerializeField] private HandcuffFitChecker[] cuffs;

    [Header("Timing")]
    [SerializeField] private float inputWindowSeconds = 0.3f;
    [SerializeField] private float despawnFadeSeconds = 0.05f;

    private const int TOTAL_ROUNDS = 4;

    private int roundIndex;
    private bool waitingShowForNextRound;

    private Coroutine inputJob;

    private bool isDemoMode = false;

    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        Debug.Log(
            $"[1-2] SetDemoMode = {isDemoMode}"
        );
    }

    public override void StartGame()
    {
        base.StartGame();

        roundIndex = 0;
        waitingShowForNextRound = false;

        if (inputJob != null)
        {
            StopCoroutine(inputJob);
            inputJob = null;
        }

        if (cuffs != null)
        {
            foreach (var cuff in cuffs)
            {
                if (cuff == null)
                    continue;

                cuff.minigame = this;
            }
        }

        if (isDemoMode)
        {
            Debug.Log("[1-2] Demo Start - CSV 시범 대기 중...");
            waitingShowForNextRound = true;
        }
        else
        {
            StartRoundNow();
        }
    }


    public override void OnRhythmEvent(string action)
    {
        if (string.IsNullOrEmpty(action))
            return;

        action = action.Trim();

        // 데모 모드일 때는 PracticeDemoManager가 ExecutePracticeAction을 호출하므로,
        // 여기서는 오직 'Show' 이벤트 타이밍에만 라운드를 스폰하도록 처리합니다.
        if (isDemoMode)
        {
            if (action == "Show")
            {
                if (inputJob != null)
                {
                    StopCoroutine(inputJob);
                    inputJob = null;
                }

                if (sequence != null)
                    sequence.DespawnRound(0f);   // 즉시 정리 (토글 오브젝트도 원래대로)

                waitingShowForNextRound = false;
                StartRoundNow();
            }
            return;
        }

        // 일반 플레이어 모드
        if (action == "Show")
        {
            if (!waitingShowForNextRound)
                return;

            if (roundIndex >= TOTAL_ROUNDS)
                return;

            waitingShowForNextRound = false;

            StartRoundNow();

            return;
        }

        if (action == "Input")
        {
            if (roundIndex >= TOTAL_ROUNDS)
                return;

            if (inputJob != null)
                StopCoroutine(inputJob);

            inputJob = StartCoroutine(InputWindowCo());

            return;
        }
    }


    private void StartRoundNow()
    {
        Debug.Log(
            $"[1-2] StartRoundNow() Round={roundIndex}, IsDemo={isDemoMode}"
        );

        if (sequence == null)
        {
            Debug.LogError(
                "[1-2] HandcuffSequenceController가 연결되지 않았습니다."
            );

            return;
        }

        sequence.SpawnRound();
        sequence.StartRoundSequence();
    }


    private IEnumerator InputWindowCo()
    {
        yield return new WaitForSeconds(
            inputWindowSeconds
        );

        if (sequence != null)
        {
            sequence.DespawnRound(
                despawnFadeSeconds
            );
        }

        roundIndex++;

        if (roundIndex < TOTAL_ROUNDS)
        {
            waitingShowForNextRound = true;
        }

        inputJob = null;
    }


    public void TryResolveRound()
    {
        // 규칙: 실제 입력을 막는 방어 코드 추가
        if (isDemoMode)
        {
            Debug.Log("[1-2] 시범 모드 중이므로 플레이어 입력 무시");
            return;
        }

        ResolveRound(true);
    }

    private void ResolveRound(bool sendPlayerInput)
    {
        if (roundIndex >= TOTAL_ROUNDS)
            return;

        if (cuffs == null || cuffs.Length < 2)
            return;

        Debug.Log($"[1-2] ResolveRound() PlayerInput={sendPlayerInput}");

        if (sequence != null)
        {
            sequence.BeginSnapFadeAll();
        }

        if (sendPlayerInput)
        {
            OnPlayerInput("Input");
        }
    }


    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (string.IsNullOrEmpty(actionType) || actionType.Trim() != "Input") return;
        if (roundIndex >= TOTAL_ROUNDS) return;

        if (sequence != null)
            sequence.PlayDemoSnap();

        OnPlayerInput("Input");

        // 라운드는 여기서 바로 넘김 → 다음 Show를 놓치지 않음
        roundIndex++;
        if (roundIndex < TOTAL_ROUNDS)
            waitingShowForNextRound = true;

        if (inputJob != null) StopCoroutine(inputJob);
        inputJob = StartCoroutine(DemoInputWindowCo());
    }

    private IEnumerator DemoInputWindowCo()
    {
        // 연출만 담당. 라운드 정리는 다음 Show에서 함
        yield return new WaitForSeconds(demoHoldSeconds);

        if (sequence != null)
            sequence.BeginSnapFadeAll();

        // 마지막 라운드는 다음 Show가 없으니 직접 정리
        if (roundIndex >= TOTAL_ROUNDS && sequence != null)
        {
            yield return new WaitForSeconds(0.2f);
            sequence.DespawnRound(despawnFadeSeconds);
        }

        inputJob = null;
    }

    /*
    public override void OnJudgement(
        JudgementResult judgement)
    {
        base.OnJudgement(judgement);

        Debug.Log(
            $"Judge: {judgement}"
        );
    }
    */
}