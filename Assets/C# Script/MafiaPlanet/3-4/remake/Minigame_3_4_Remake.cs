using UnityEngine;

/// <summary>
/// Minigame 3-4 밑장 빼기
/// 0 ~ 6초 : 카드 5장을 1.2초 간격으로 돌린다. 함정 카드가 나올 때 상대 표정이 바뀐다.
/// 6.6초~  : 손이 1.2초마다 카드 한 장씩 위로 이동 (7.2 / 8.4 / 9.6 / 10.8 / 12.0초에 카드 위)
/// 플레이어는 손이 '함정 카드' 위에 왔을 때만 터치해야 한다.
///
/// 판정(직접): 함정 카드 박자 ±goodWindow 안에 터치 + 다른 카드에서 터치 안 함 → 성공 (대상 1개)
/// 모든 시간은 노래 시간(RhythmManager.SongTime) 기준.
/// CSV 3-4 노트는 판정에 쓰지 않는다 (타입 Beat = 카드 박자 표시용).
/// </summary>
public class Minigame_3_4_Remake : MiniGameBase, IPracticeDemoInput
{
    [Header("Refs")]
    [SerializeField] private Transform handTransform;
    [SerializeField] private HandMover handMover;          // 구버전 클릭 처리 (이제 사용 안 함)
    [SerializeField] private DrawCard3_4remake drawCard;   // 비우면 자동 탐색

    [Header("(구버전) Finish Condition")]
    [SerializeField] private float handStopX = 6f;

    [Header("(구버전) Judge Rule")]
    [SerializeField] private int expectedInputCount = 5;
    [SerializeField] private int requiredGoodOrPerfectCount = 1;

    [Header("카드 박자 (노래 시간, 초)")]
    [Tooltip("손이 첫 번째 카드 위에 오는 시각")]
    [SerializeField] private float firstCardTime = 7.2f;
    [SerializeField] private float cardInterval = 1.2f;
    [SerializeField] private int cardCount = 5;

    [Header("판정 범위 (초)")]
    [SerializeField] private float perfectWindow = 0.1f;
    [SerializeField] private float goodWindow = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    protected override string MinigameTitle => "밑장 빼기";
    protected override string MinigameExplain => "화살표가 왼쪽에서 오른쪽으로 이동합니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "타이밍에 맞춰 화면을 터치하세요."
    };

    // 수동 판정 (대상 1개: 밑장 잡기)
    protected override bool UseRhythmJudgementScore => false;
    protected override int ManualTotalNodeCount => 1;

    public GameObject arrow;

    private bool isDemoMode = false;
    public new bool IsDemoMode => isDemoMode;

    private bool ended;
    private bool trapHit;
    private bool wrongClick;
    private readonly bool[] slotUsed = new bool[16];

    private float localTime;

    // 노래 시간 (없으면 로컬 시간)
    public float SongTime
    {
        get
        {
            if (rhythmManager is RhythmManager rm)
            {
                // 노래 시작 전(프리팹 준비 중)에는 아무것도 진행하지 않는다
                return rm.IsRunning ? (float)rm.SongTime : -1f;
            }

            // 리듬매니저 없이 단독 테스트할 때
            return localTime;
        }
    }

    private void JudgeLog(string msg)
    {
        if (judgeLog) Debug.Log(msg);
    }

    public override void SetDemoMode(bool isDemo)
    {
        base.SetDemoMode(isDemo);
        isDemoMode = isDemo;
        Debug.Log($"[3-4] SetDemoMode = {isDemoMode}");
    }

    public override void StartGame()
    {
        base.StartGame();

        if (arrow != null) arrow.SetActive(true);

        ended = false;
        trapHit = false;
        wrongClick = false;
        localTime = 0f;

        for (int i = 0; i < slotUsed.Length; i++)
            slotUsed[i] = false;

        if (drawCard == null)
            drawCard = GetComponentInChildren<DrawCard3_4remake>(true);

        // 클릭은 이 스크립트가 직접 받는다 (HandMover 중복 입력 방지)
        if (handMover != null)
            handMover.start = false;
    }

    private void Update()
    {
        localTime += Time.deltaTime;

        if (ended) return;

        float t = SongTime;

        // 마지막 카드 판정 범위가 지나면 종료
        float lastCard = firstCardTime + cardInterval * (cardCount - 1);
        if (t > lastCard + goodWindow)
        {
            Finish();
            return;
        }

        if (isDemoMode)
        {
            // 시범: 함정 카드 박자가 되면 직접 터치 (차트 노트 불필요)
            int trap = drawCard != null ? drawCard.TrapIndex : -1;
            if (trap >= 0 && !trapHit && t >= firstCardTime + cardInterval * trap)
                SubmitInput();
            return;
        }

        if (IsInputLocked) return;

        if (Input.GetMouseButtonDown(0))
            SubmitInput();
    }

    // 클릭 1회 처리 (플레이어/시범 공통)
    public void SubmitInput()
    {
        if (ended) return;

        float t = SongTime;

        // 손이 움직이기 전(카드 돌리는 중)의 클릭은 무시
        if (t < firstCardTime - cardInterval * 0.5f)
        {
            JudgeLog($"[3-4 Judge] 카드 돌리는 중 클릭 무시 song={t:F3}");
            return;
        }

        int slot = Mathf.Clamp(
            Mathf.RoundToInt((t - firstCardTime) / cardInterval),
            0, cardCount - 1);

        float diff = t - (firstCardTime + cardInterval * slot);
        float abs = Mathf.Abs(diff);

        if (abs > goodWindow)
        {
            JudgeLog($"[3-4 Judge] 카드 사이 클릭 무시 (카드{slot} diff={diff * 1000:F0}ms)");
            return;
        }

        if (slotUsed[slot]) return;
        slotUsed[slot] = true;

        int trapIndex = drawCard != null ? drawCard.TrapIndex : -1;
        string grade = abs <= perfectWindow ? "Perfect" : "Good";

        if (slot == trapIndex)
        {
            trapHit = true;
            JudgeLog($"[3-4 Judge] 함정 카드{slot} 터치 diff={diff * 1000:F0}ms → {grade}");
        }
        else
        {
            wrongClick = true;
            JudgeLog($"[3-4 Judge] 일반 카드{slot} 터치 → 실패 (함정은 카드{trapIndex})");
        }
    }

    private void Finish()
    {
        if (ended) return;
        ended = true;

        bool success = trapHit && !wrongClick;

        if (success) ReportManualSuccess();
        else ReportManualFail();

        JudgeLog($"[3-4 Judge] 결과 {(success ? "성공" : "실패")} (함정 터치={trapHit}, 오터치={wrongClick})");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // RhythmManager 판정은 쓰지 않는다 (수동 판정)
    }

    // 시범은 Update에서 카드 박자로 직접 처리한다 (차트 노트 사용 안 함)
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
    }

    // 구버전 호환
    public void FinalJudge()
    {
        Finish();
    }
}