using UnityEngine;

/// <summary>
/// Minigame 3-4 Remake (판정 전용)
/// - StartGame: 초기화 + (선택) handMover.start = true
/// - SubmitInput: 클릭 순간 호출 -> OnPlayerInput("Input") 전달
/// - OnJudgement: RhythmManagerTest가 계산한 Good/Perfect/Miss 카운트만 누적
/// - FinalJudge: handStopX 도달 시 Success/Fail 결정 (Success/Fail 미사용)
/// </summary>
public class Minigame_3_4_Remake : MiniGameBase, IPracticeDemoInput
{
    [Header("Refs")]
    [SerializeField] private Transform handTransform;     // hand 트랜스폼(최종 판정 트리거용)
    [SerializeField] private HandMover handMover;          // 있으면 start on/off로 hand 제어

    [Header("Finish Condition")]
    [SerializeField] private float handStopX = 6f;         // hand x가 이 값 이상이면 최종판정

    [Header("Judge Rule")]
    [Tooltip("CSV에 Input이 총 몇 번 있는지(예: 5). 체크를 원치 않으면 0으로 두기.")]
    [SerializeField] private int expectedInputCount = 5;

    [Tooltip("성공으로 인정할 Good/Perfect 횟수. (예: 트랩 1번만 맞추는 게임이면 1)")]
    [SerializeField] private int requiredGoodOrPerfectCount = 1;


    protected override string MinigameTitle => "젤리 블록 철거!";
    protected override string MinigameExplain => "죄수가 순서대로 나타나 타이밍을 알려줍니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "배경이 어두워지면 타이밍에 맞게 죄수를 터치해주세요."
    };

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[3-4] SetDemoMode = {isDemoMode}");
    }


    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim();

        if (string.Equals(actionType, "Input", System.StringComparison.OrdinalIgnoreCase))
        {
            if (ended || IsInputLocked)
                return;

            Debug.Log($"[3-4 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}");
            SubmitInput();
        }
    }


    public GameObject arrow;
    // ===== 상태 =====
    private bool ended;

    // ===== 판정 카운트 =====
    private int perfectCnt;
    private int goodCnt;
    private int missCnt;

    // 클릭(입력) 시도 횟수
    private int submittedInputCnt;

    public override void StartGame()
    {
        Debug.Log("[3-4] StartGame called");
        if (arrow != null) arrow.SetActive(true);
        ended = false;

        perfectCnt = 0;
        goodCnt = 0;
        missCnt = 0;
        submittedInputCnt = 0;

        if (handMover != null) handMover.start = true;
    }

    private void Update()
    {
        if (ended) return;

        if (handTransform != null && handTransform.position.x >= handStopX)
        {
            FinalJudge();
        }
    }

    /// <summary>
    /// 클릭 순간 호출 -> RhythmManagerTest에 "Input" 전달
    /// </summary>
    public void SubmitInput()
    {
        if (ended) return;
        if (IsInputLocked) return;

        submittedInputCnt++;
        OnPlayerInput("Input");
    }

    /// <summary>
    /// 판정 결과 누적
    /// </summary>
    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended) return;

        switch (judgement)
        {
            case JudgementResult.Perfect:
                perfectCnt++;
                break;
            case JudgementResult.Good:
                goodCnt++;
                break;
            case JudgementResult.Miss:
                missCnt++;
                break;
        }
    }

    /// <summary>
    /// handStopX 도달 시 최종 판정
    /// </summary>
    public void FinalJudge()
    {
        if (ended) return;
        ended = true;

        if (handMover != null) handMover.start = false;

        int goodOrPerfect = goodCnt + perfectCnt;
        Debug.Log($"[3-4] FinalJudge: P={perfectCnt}, G={goodCnt}, M={missCnt}, GP={goodOrPerfect}, Submit={submittedInputCnt}");

        // Success와 Fail 함수를 호출하지 않고 로그로만 처리
        if (handMover != null && goodOrPerfect == requiredGoodOrPerfectCount && handMover.suspiciousClickCount == 1)
        {
            Debug.Log("[3-4] 조건 충족 완료 (Success 미사용)");
        }
        else
        {
            Debug.Log("[3-4] 조건 미달 (Fail 미사용)");
        }
    }
}