using UnityEngine;

// 3-11 서류 담기. 진행/판정은 GameManager_3_11, 점수는 이 스크립트로 보고된다.
public class Minigame_3_11_remake : MiniGameBase, IPracticeDemoInput
{
    // ===== 미니게임 기본 정보 =====
    protected override float TimerDuration => 10f;
    protected override string MinigameTitle => "서류 담기";

    protected override string MinigameExplain => "서류 혹은 폭탄이 날아옵니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "폭탄은 피하고, 서류가 박스에 도착하면 화면을 터치하세요."
    };

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;

    // 판정은 직접 하되, 점수는 Perfect / Good 등급 그대로 집계 (물체마다 1개)
    //  - 성공: AddJudgementCount(Perfect 또는 Good)
    //  - 실패: 보고 안 함 → 정산 때 (총 개수 - 성공 수)가 Miss로 채워짐
    protected override bool UseRhythmJudgementScore => true;

    [Header("진행 / 판정")]
    [SerializeField] private GameManager_3_11 gameManager;

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

        FindManager();
        if (gameManager != null)
            gameManager.SetDemoMode(isDemoMode);

        Debug.Log("[3-11] 데모 모드: " + isDemoMode);
    }

    protected override void Awake()
    {
        base.Awake();
        FindManager();
    }

    private void FindManager()
    {
        if (gameManager == null)
            gameManager = GetComponentInChildren<GameManager_3_11>(true);
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager_3_11>();
    }

    public override void StartGame()
    {
        base.StartGame();
        started = true;
        finalized = false;
        localTime = 0f;

        // 차트(3-11)에 판정 노트가 없으므로 총 개수를 직접 지정
        FindManager();
        if (gameManager != null)
            SetRuntimeTotalNodeCount(gameManager.TotalCount);

        if (gameManager != null)
            gameManager.SetDemoMode(isDemoMode);
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

    // GameManager_3_11에서 호출
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
            if (gameManager != null)
                gameManager.ResolveAllPending();
        }

        return base.FinalizeScoreSession();
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // RhythmManager 판정은 쓰지 않는다
    }

    // 시범은 GameManager_3_11이 도착 시각에 직접 터치한다
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
    }
}