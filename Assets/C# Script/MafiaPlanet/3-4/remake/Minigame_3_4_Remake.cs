
using UnityEngine;

/// <summary>
/// Minigame 3-4 밑장 빼기
///
/// 0 ~ 6초:
/// 카드 5장을 1.2초 간격으로 돌린다.
/// 함정 카드가 나올 때 상대 표정이 바뀐다.
///
/// 6.6초~:
/// 손이 1.2초마다 카드 한 장씩 위로 이동
/// (7.2 / 8.4 / 9.6 / 10.8 / 12.0초)
///
/// 함정 카드 박자에 터치하고
/// 일반 카드에서는 터치하지 않으면 성공.
///
/// 클릭 판정된 카드는 1.2배 확대된 상태로 유지한다.
///
/// 모든 시간은 RhythmManager.SongTime 기준.
/// CSV 3-4 노트는 판정에 사용하지 않는다.
/// </summary>
public class Minigame_3_4_Remake : MiniGameBase, IPracticeDemoInput
{
    [Header("Refs")]
    [SerializeField] private Transform handTransform;
    [SerializeField] private HandMover handMover;
    [SerializeField] private DrawCard3_4remake drawCard;

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

    protected override string MinigameExplain =>
        "화살표가 왼쪽에서 오른쪽으로 이동합니다.";

    protected override string[] AdditionalMinigameExplains =>
        new string[]
        {
            "타이밍에 맞춰 화면을 터치하세요."
        };

    // 수동 판정 사용
    protected override bool UseRhythmJudgementScore => false;
    protected override int ManualTotalNodeCount => 1;

    public GameObject arrow;

    private bool isDemoMode = false;
    public new bool IsDemoMode => isDemoMode;

    private bool ended;
    private bool trapHit;
    private bool wrongClick;

    // 카드별 클릭 여부 기록
    private readonly bool[] slotUsed = new bool[16];

    private float localTime;

    /// <summary>
    /// 노래 시간 반환.
    /// RhythmManager가 없으면 로컬 시간을 사용한다.
    /// </summary>
    public float SongTime
    {
        get
        {
            if (rhythmManager is RhythmManager rm)
            {
                // 노래 시작 전에는 진행하지 않음
                return rm.IsRunning
                    ? (float)rm.SongTime
                    : -1f;
            }

            return localTime;
        }
    }

    private void JudgeLog(string msg)
    {
        if (judgeLog)
        {
            Debug.Log(msg);
        }
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

        if (arrow != null)
        {
            arrow.SetActive(true);
        }

        // 게임 상태 초기화
        ended = false;
        trapHit = false;
        wrongClick = false;
        localTime = 0f;

        // 이전 클릭 기록 초기화
        for (int i = 0; i < slotUsed.Length; i++)
        {
            slotUsed[i] = false;
        }

        // 카드 관리 스크립트 자동 탐색
        if (drawCard == null)
        {
            drawCard =
                GetComponentInChildren<DrawCard3_4remake>(true);
        }

        // 구버전 HandMover 입력 방지
        if (handMover != null)
        {
            handMover.start = false;
        }
    }

    private void Update()
    {
        localTime += Time.deltaTime;

        if (ended)
            return;

        float t = SongTime;

        // 마지막 카드 판정 범위가 지나면 종료
        float lastCard =
            firstCardTime + cardInterval * (cardCount - 1);

        if (t > lastCard + goodWindow)
        {
            Finish();
            return;
        }

        // 시범 모드
        if (isDemoMode)
        {
            int trap =
                drawCard != null ? drawCard.TrapIndex : -1;

            // 함정 카드 박자에 자동 클릭
            if (trap >= 0 &&
                !trapHit &&
                t >= firstCardTime + cardInterval * trap)
            {
                SubmitInput();
            }

            return;
        }

        if (IsInputLocked)
            return;

        // 마우스 왼쪽 버튼 클릭
        if (Input.GetMouseButtonDown(0))
        {
            SubmitInput();
        }
    }

    /// <summary>
    /// 클릭 1회 처리.
    ///
    /// 클릭한 시각으로 카드 인덱스를 계산한다.
    /// 유효한 카드 박자에 클릭하면 해당 카드를 확대한다.
    ///
    /// 함정 카드 클릭: 성공 기록
    /// 일반 카드 클릭: 실패 기록
    /// </summary>
    public void SubmitInput()
    {
        if (ended)
            return;

        float t = SongTime;

        // 카드 돌리는 중에는 클릭 무시
        if (t < firstCardTime - cardInterval * 0.5f)
        {
            JudgeLog(
                $"[3-4 Judge] 카드 돌리는 중 클릭 무시 song={t:F3}");

            return;
        }

        // 현재 클릭한 카드 인덱스 계산
        int slot = Mathf.Clamp(
            Mathf.RoundToInt(
                (t - firstCardTime) / cardInterval),
            0,
            cardCount - 1
        );

        // 카드 박자와 클릭 시각의 차이
        float diff =
            t - (firstCardTime + cardInterval * slot);

        float abs = Mathf.Abs(diff);

        // 판정 범위 밖 클릭 무시
        if (abs > goodWindow)
        {
            JudgeLog(
                $"[3-4 Judge] 카드 사이 클릭 무시 " +
                $"(카드{slot} diff={diff * 1000:F0}ms)");

            return;
        }

        // 이미 클릭한 카드라면 중복 처리하지 않음
        if (slotUsed[slot])
            return;

        slotUsed[slot] = true;

        // 함정 카드 인덱스 확인
        int trapIndex =
            drawCard != null ? drawCard.TrapIndex : -1;

        // Perfect / Good 판정
        string grade =
            abs <= perfectWindow ? "Perfect" : "Good";

        // ★ 클릭 판정된 카드를 1.2배 확대
        // 함정 카드와 일반 카드 모두 확대
        // 확대 상태는 DrawCard에서 계속 유지
        if (drawCard != null)
        {
            drawCard.ScaleClickedCard(slot);
        }

        if (slot == trapIndex)
        {
            // 함정 카드 클릭 성공
            trapHit = true;

            JudgeLog(
                $"[3-4 Judge] 함정 카드{slot} 터치 " +
                $"diff={diff * 1000:F0}ms → {grade}");
        }
        else
        {
            // 일반 카드 클릭 실패
            wrongClick = true;

            JudgeLog(
                $"[3-4 Judge] 일반 카드{slot} 터치 → 실패 " +
                $"(함정은 카드{trapIndex})");
        }
    }

    /// <summary>
    /// 미니게임 종료 판정.
    /// 함정 카드를 클릭하고 일반 카드를 클릭하지 않아야 성공.
    /// </summary>
    private void Finish()
    {
        if (ended)
            return;

        ended = true;

        bool success = trapHit && !wrongClick;

        if (success)
        {
            ReportManualSuccess();
        }
        else
        {
            ReportManualFail();
        }

        JudgeLog(
            $"[3-4 Judge] 결과 {(success ? "성공" : "실패")} " +
            $"(함정 터치={trapHit}, 오터치={wrongClick})");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // RhythmManager 판정은 사용하지 않음
    }

    public override void ExecutePracticeAction(
        int actionIndex,
        string actionType)
    {
        // 시범 모드는 Update에서 직접 처리
    }

    // 구버전 호환
    public void FinalJudge()
    {
        Finish();
    }
}
