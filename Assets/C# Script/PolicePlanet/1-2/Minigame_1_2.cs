using System.Collections;
using UnityEngine;

public class Minigame_1_2 : MiniGameBase, IPracticeDemoInput
{
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;

    protected override float TimerDuration => 15f;
    protected override string MinigameTitle => "수갑 채우기";
    protected override string MinigameExplain =>
        "왼쪽에서 수갑이 채워져 타이밍을 알려줍니다";

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
            Debug.Log("[1-2] Demo Start - CSV 입력을 기다립니다.");
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

        if (isDemoMode)
        {
            if (action == "Show")
            {
                if (waitingShowForNextRound && roundIndex < TOTAL_ROUNDS)
                {
                    waitingShowForNextRound = false;
                    StartRoundNow();
                }
            }
            return;
        }

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
            $"[1-2] StartRoundNow() Round={roundIndex}"
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

    public override void ExecutePracticeAction(
    int actionIndex,
    string actionType)
    {
        Debug.Log(
            $"[1-2 Demo] ExecutePracticeAction 호출 " +
            $"Index={actionIndex}, Type={actionType}, " +
            $"Demo={isDemoMode}, Round={roundIndex}"
        );

        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim();

        if (actionType != "Input")
        {
            Debug.Log(
                $"[1-2 Demo] Input이 아니어서 무시: {actionType}"
            );

            return;
        }

        if (roundIndex >= TOTAL_ROUNDS)
            return;

        Debug.Log(
            $"[1-2 Demo] 수갑 시범 실행! Round={roundIndex}"
        );

        ResolveRound(false);

        if (inputJob != null)
            StopCoroutine(inputJob);

        inputJob =
            StartCoroutine(
                DemoInputWindowCo()
            );
    }

    private IEnumerator DemoInputWindowCo()
    {
        yield return new WaitForSeconds(inputWindowSeconds);

        if (sequence != null)
        {
            sequence.DespawnRound(despawnFadeSeconds);
        }

        roundIndex++;

        if (roundIndex < TOTAL_ROUNDS)
        {
            // 다음 라운드를 위해 바로 스폰 대기 상태로 전환
            waitingShowForNextRound = false;
            StartRoundNow(); // 데모에서는 'Show' 신호를 기다리지 않고 곧바로 다음 라운드를 스폰하여 시범 끊김 방지
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