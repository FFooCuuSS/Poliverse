using System.Collections;
using UnityEngine;

public class Minigame_1_2 : MiniGameBase
{
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;

    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "체포해라!";

    [Header("Sequence Controller")]
    [SerializeField] private HandcuffSequenceController sequence;

    [Header("Round Objects (2 cuffs used in this minigame)")]
    [SerializeField] private HandcuffFitChecker[] cuffs; // 반드시 2개 연결

    [Header("Timing")]
    [SerializeField] private float inputWindowSeconds = 0.3f;
    [SerializeField] private float despawnFadeSeconds = 0.05f;

    private const int TOTAL_ROUNDS = 4;

    private int roundIndex;
    private bool waitingShowForNextRound;
    public bool IsInputWindowOpen { get; private set; }

    private Coroutine inputJob;

    private void Start()
    {
        StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();
        roundIndex = 0;
        waitingShowForNextRound = false;
        IsInputWindowOpen = false;

        if (inputJob != null) StopCoroutine(inputJob);
        inputJob = null;

        if (cuffs != null)
        {
            foreach (var c in cuffs)
            {
                if (c == null) continue;
                c.minigame = this;
            }
        }

        StartRoundNow();
    }

    public override void OnRhythmEvent(string action)
    {
        action = action.Trim();

        if (action == "Show")
        {
            if (!waitingShowForNextRound) return;
            if (roundIndex >= TOTAL_ROUNDS) return;

            waitingShowForNextRound = false;
            StartRoundNow();
            return;
        }

        if (action == "Input")
        {
            if (roundIndex >= TOTAL_ROUNDS) return;
            if (IsInputWindowOpen) return;

            if (inputJob != null) StopCoroutine(inputJob);
            inputJob = StartCoroutine(InputWindowCo());
            return;
        }
    }

    private void StartRoundNow()
    {
        if (sequence != null) sequence.SpawnRound();
        if (sequence != null) sequence.StartRoundSequence();
    }

    private IEnumerator InputWindowCo()
    {
        IsInputWindowOpen = true;

        yield return new WaitForSeconds(inputWindowSeconds);

        IsInputWindowOpen = false;

        if (sequence != null) sequence.DespawnRound(despawnFadeSeconds);

        roundIndex++;

        if (roundIndex < TOTAL_ROUNDS)
            waitingShowForNextRound = true;

        inputJob = null;
    }

    public void TryResolveRound()
    {
        if (!IsInputWindowOpen) return;
        if (cuffs == null || cuffs.Length < 2) return;

        if (!cuffs[0].IsSnapped || !cuffs[1].IsSnapped) return;
        if (cuffs[0].SnappedHand == cuffs[1].SnappedHand) return;

        if (sequence != null)
            sequence.BeginSnapFadeAll();

        OnPlayerInput("Input");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        Debug.Log($"Judge: {judgement}");
    }
}