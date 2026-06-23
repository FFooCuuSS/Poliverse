using UnityEngine;
using DG.Tweening;

public class Minigame_1_3 : MiniGameBase
{
    protected override float TimerDuration => 18f;
    protected override string MinigameExplain => "조심해라!";

    // 1-3은 RhythmManager의 Perfect/Good/Miss 판정을 점수로 쓰지 않음.
    // 실패 횟수는 직접 ReportManualFail로 올리고,
    // 남은 점수칸은 종료 시 Success로 채움.
    protected override bool UseRhythmJudgementScore => false;
    protected override int ManualTotalNodeCount => scoreTargetCount;

    [Header("Score")]
    [SerializeField] private int scoreTargetCount = 10;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float backDistance = 0.4f;
    [SerializeField] private float backDuration = 0.15f;
    [SerializeField] private float dashDistance = 20f;
    [SerializeField] private float dashDuration = 0.35f;
    [SerializeField] private float overshootDistance = 0.6f;

    private float currentTime;
    private bool isTimerRunning;

    private float fixedY;
    private bool lockY;

    private int failCount;
    private bool scoreFilled;

    private void Start()
    {
        // StartGame은 MinigameUIManager에서 호출
    }

    public override void StartGame()
    {
        base.StartGame();

        currentTime = TimerDuration;
        isTimerRunning = true;

        failCount = 0;
        scoreFilled = false;

        fixedY = 0f;
        lockY = false;
    }

    private void Update()
    {
        RunTimer();
        LockPlayerYIfNeeded();
    }

    private void RunTimer()
    {
        if (!isTimerRunning || IsSuccess) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            isTimerRunning = false;
        }
    }

    private void LockPlayerYIfNeeded()
    {
        if (!lockY) return;
        if (player == null) return;

        Vector3 pos = player.position;
        pos.y = fixedY;
        player.position = pos;
    }

    public override void OnRhythmEvent(string action)
    {
        base.OnRhythmEvent(action);

        if (string.IsNullOrEmpty(action)) return;

        action = action.Trim();

        if (action == "End")
        {
            FillRemainingAsSuccess();
            PlayEndAnimation();
        }
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // 1-3은 RhythmManager 판정 점수를 사용하지 않음.
        // 실패는 PlayerReachChecker에서 ReportStageFail()로 직접 보고.
    }

    public void ReportStageFail()
    {
        if (scoreFilled) return;

        failCount++;
        ReportManualFail();
    }

    public override ScoreResult FinalizeScoreSession()
    {
        FillRemainingAsSuccess();
        return base.FinalizeScoreSession();
    }

    private void FillRemainingAsSuccess()
    {
        if (scoreFilled) return;
        scoreFilled = true;

        int safeTotal = Mathf.Max(0, scoreTargetCount);
        int safeFail = Mathf.Clamp(failCount, 0, safeTotal);
        int successCount = Mathf.Max(0, safeTotal - safeFail);

        for (int i = 0; i < successCount; i++)
        {
            ReportManualSuccess();
        }
    }

    // =========================
    // End 연출
    // =========================
    private void PlayEndAnimation()
    {
        if (player == null) return;

        Vector3 forward = Vector3.right;
        Vector3 startPos = player.position;

        player.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(player.DOMove(startPos - forward * backDistance, backDuration)
            .SetEase(Ease.OutSine));

        seq.Append(player.DOMove(startPos + forward * dashDistance, dashDuration)
            .SetEase(Ease.InSine));

        seq.Append(player.DOMove(startPos + forward * (dashDistance + overshootDistance), 0.06f)
            .SetEase(Ease.OutQuad));

        seq.OnComplete(() =>
        {
            fixedY = player.position.y;
            lockY = true;
        });
    }
}