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

    private int roundIndex;                 // 0~3
    private bool waitingShowForNextRound;   // round1~3 시작 대기
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

        // FitChecker에 minigame 연결
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
        // 스폰/리셋 (손/수갑 모두)
        if (sequence != null) sequence.SpawnRound();

        // 연출 시작: 왼손→0.2→오른손 자동
        if (sequence != null) sequence.StartRoundSequence();
    }

    private IEnumerator InputWindowCo()
    {
        IsInputWindowOpen = true;

        // 0.3초 동안만 스냅 허용
        yield return new WaitForSeconds(inputWindowSeconds);

        IsInputWindowOpen = false;

        // 0.3초 끝나면 무조건 디스폰(연출+수갑)
        if (sequence != null) sequence.DespawnRound(despawnFadeSeconds);

        // 다음 라운드로
        roundIndex++;

        if (roundIndex < TOTAL_ROUNDS)
        {
            // Round1~3은 Show를 기다린다
            waitingShowForNextRound = true;
        }

        inputJob = null;
    }

    // FitChecker들이 스냅될 때마다 호출
    public void TryResolveRound()
    {
        if (!IsInputWindowOpen) return;
        if (cuffs == null || cuffs.Length < 2) return;

        if (!cuffs[0].IsSnapped || !cuffs[1].IsSnapped) return;
        if (cuffs[0].SnappedHand == cuffs[1].SnappedHand) return;

        // 리듬 매니저에 "입력했다"만 전달 (게임 멈추는 권한 없음)
        OnPlayerInput("Input");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // 여기서 Success/Fail 금지 (세션 제어 X)
        Debug.Log($"Judge: {judgement}");
    }
}