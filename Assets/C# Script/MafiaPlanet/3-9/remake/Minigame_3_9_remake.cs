using UnityEngine;

// 3-9 암호 맞추기
// 패턴 6개가 순서대로 켜지고(노래 시간 기준), 입력 단계에서 같은 순서로 누른다.
// 판정: 입력 1개당 정답/오답 (총 6개). 시간 안에 못 누른 칸은 미니게임 종료 시 실패 처리.
public class Minigame_3_9_remake : MiniGameBase, IPracticeDemoInput
{
    // ===== 미니게임 기본 정보 =====

    protected override float TimerDuration => 20f;

    protected override string MinigameTitle => "암호 맞추기";

    protected override string MinigameExplain => "표시된 순서대로 암호가 빨간색으로 변하면 입력하세요.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "각 암호를 표시된 순서대로 입력하세요."
    };

    // RhythmManager 판정 사용 안 함 (ReportManualSuccess / Fail)
    protected override bool UseRhythmJudgementScore => false;
    protected override int ManualTotalNodeCount => 6;

    [SerializeField] private Manager3_9 manager;   // 비우면 자동 탐색

    private bool isDemoMode;
    public new bool IsDemoMode => isDemoMode;

    private bool started;
    private bool finalized;
    private float localTime;

    // 노래 시간 (노래 시작 전 -1, 리듬매니저 없으면 로컬 시간)
    public float SongTime
    {
        get
        {
            if (rhythmManager is RhythmManager rm)
                return rm.IsRunning ? (float)rm.SongTime : -1f;

            return localTime;
        }
    }

    public override void SetDemoMode(bool isDemo)
    {
        base.SetDemoMode(isDemo);
        isDemoMode = isDemo;
        Debug.Log("[3-9] 데모 모드: " + isDemoMode);
    }

    protected override void Awake()
    {
        base.Awake();

        if (manager == null)
            manager = GetComponentInChildren<Manager3_9>(true);
    }

    public override void StartGame()
    {
        base.StartGame();
        started = true;
        finalized = false;
        localTime = 0f;
        IsInputLocked = false;

        SetRuntimeTotalNodeCount(6);
    }

    private void Start()
    {
        // 외부에서 StartGame을 안 불러준 경우 대비
        if (!started)
            StartGame();
    }

    private void Update()
    {
        localTime += Time.deltaTime;
    }

    // 시범 자동 입력은 Manager3_9가 처리한다
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
    }

    /// <summary>Manager3_9에서 정답 입력 시 호출</summary>
    public void ReportPatternSuccess()
    {
        ReportManualSuccess();
        Debug.Log("[3-9 Judge] 정답 +1");
    }

    /// <summary>Manager3_9에서 오답 입력 시 호출</summary>
    public void ReportPatternFail()
    {
        ReportManualFail();
        Debug.Log("[3-9 Judge] 오답 +1");
    }

    /// <summary>6번의 입력이 전부 끝난 후 호출</summary>
    public void FinishGame()
    {
        ScoreResult result = FinalizeScoreSession();

        Debug.Log(
            "[3-9] 최종 결과 / Total = " + result.totalNode +
            " / Success = " + result.perfect +
            " / Fail = " + result.miss);
    }

    public override ScoreResult FinalizeScoreSession()
    {
        // 시간 안에 다 못 누른 칸은 실패로 채운다 (한 번만)
        if (!finalized)
        {
            finalized = true;

            int remain = manager != null ? manager.RemainingInputs : 0;
            for (int i = 0; i < remain; i++)
                ReportManualFail();

            if (remain > 0)
                Debug.Log($"[3-9 Judge] 시간 내 미입력 {remain}개 → 실패");
        }

        return base.FinalizeScoreSession();
    }
}