using UnityEngine;

// 3-13 무기 정리. 진행/판정은 CaseManager_3_13, 점수는 이 스크립트로 보고된다.
public class Minigame3_13_remake : MiniGameBase, IPracticeDemoInput
{
    // ===== 미니게임 기본 정보 =====
    protected override float TimerDuration => 10f;
    protected override string MinigameTitle => "무기 정리";

    protected override string MinigameExplain => "무기가 각 방향에서 날아옵니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "각 무기의 모양대로 화면을 터치해 수납하세요."
    };

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;

    // 판정은 직접 하되, 점수는 Perfect / Good 등급 그대로 집계한다.
    //  - 성공: AddJudgementCount(Perfect 또는 Good)
    //  - 실패: 보고 안 함 → 정산 때 (총 개수 - 성공 수)가 Miss로 채워짐
    //  - RhythmManager 판정(OnJudgement)은 무시
    protected override bool UseRhythmJudgementScore => true;
    [SerializeField] private int weaponCount = 4;

    [SerializeField] private CaseManager_3_13 caseManager;

    private bool isDemoMode = false;
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

        FindManager();
        if (caseManager != null)
            caseManager.SetDemoMode(isDemoMode);

        Debug.Log("[3-13] 데모 모드: " + isDemoMode);
    }

    protected override void Awake()
    {
        base.Awake();
        FindManager();
    }

    private void FindManager()
    {
        if (caseManager == null)
            caseManager = GetComponentInChildren<CaseManager_3_13>(true);
        if (caseManager == null)
            caseManager = FindObjectOfType<CaseManager_3_13>();
    }

    public override void StartGame()
    {
        base.StartGame();
        started = true;
        finalized = false;
        localTime = 0f;
        IsInputLocked = false;

        // 차트(3-13)에 판정 노트가 없으므로 총 개수를 직접 지정
        SetRuntimeTotalNodeCount(weaponCount);

        if (caseManager != null)
            caseManager.SetDemoMode(isDemoMode);
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

    // CaseManager_3_13에서 호출
    public void ReportHit(bool perfect)
    {
        AddJudgementCount(perfect ? JudgementResult.Perfect : JudgementResult.Good);
    }

    public void ReportMiss()
    {
        // 정산 때 자동으로 Miss 처리됨 (AutoFillRemainingAsMiss)
    }

    public override ScoreResult FinalizeScoreSession()
    {
        if (!finalized)
        {
            finalized = true;
            if (caseManager != null)
                caseManager.ResolveAllPending();
        }

        return base.FinalizeScoreSession();
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // RhythmManager 판정은 쓰지 않는다
    }

    // 시범은 CaseManager_3_13이 도착 순간 직접 터치한다
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
    }
}