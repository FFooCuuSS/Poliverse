using UnityEngine;

/// <summary>
/// Minigame 3-4 Remake (판정 전용)
/// - StartGame: 초기화 + (선택) handMover.start = true
/// - SubmitInput: 클릭 순간 호출 -> OnPlayerInput("Input") 전달
/// - OnJudgement: RhythmManagerTest가 계산한 Good/Perfect/Miss 카운트만 누적
/// - FinalJudge: handStopX 도달 시 Success/Fail 결정
/// </summary>
public class Minigame_3_4_Remake : MiniGameBase
{
    [Header("Refs")]
    [SerializeField] private Transform handTransform;     // hand 트랜스폼(최종 판정 트리거용)
    [SerializeField] private HandMover handMover;         // 있으면 start on/off로 hand 제어

    [Header("Finish Condition")]
    [SerializeField] private float handStopX = 6f;        // hand x가 이 값 이상이면 최종판정

    [Header("Judge Rule")]
    [Tooltip("CSV에 Input이 총 몇 번 있는지(예: 5). 체크를 원치 않으면 0으로 두기.")]
    [SerializeField] private int expectedInputCount = 5;

    [Tooltip("성공으로 인정할 Good/Perfect 횟수. (예: 트랩 1번만 맞추는 게임이면 1)")]
    [SerializeField] private int requiredGoodOrPerfectCount = 1;

    // ===== 상태 =====
    private bool ended;

    // ===== 판정 카운트 =====
    private int perfectCnt;
    private int goodCnt;
    private int missCnt;

    // 클릭(입력) 시도 횟수(원하면 디버그/제한에 활용 가능)
    private int submittedInputCnt;

    private void Start()
    {
        //StartGame(); // 자동 시작 원하면 주석 해제
    }

    public override void StartGame()
    {
        Debug.Log("[3-4] StartGame called");
        base.StartGame();

        ended = false;

        perfectCnt = 0;
        goodCnt = 0;
        missCnt = 0;
        submittedInputCnt = 0;

        // hand 이동 시작(선택)
        if (handMover != null) handMover.start = true;
    }

    private void Update()
    {
        if (ended) return;

        // hand 위치가 목표 x 도달하면 최종 판정
        if (handTransform != null && handTransform.position.x >= handStopX)
        {
            FinalJudge();
        }
    }

    /// <summary>
    /// (다른 스크립트에 있는) GetMouseButtonDown에서 이거 호출하면 됨.
    /// 클릭 -> RhythmManagerTest에 "Input"을 전달 -> 판정 결과는 OnJudgement로 돌아옴.
    /// </summary>
    public void SubmitInput()
    {
        if (ended) return;
        if (IsInputLocked) return;     // 리듬매니저 입력 잠금(쿨다운 등)일 때 무시

        submittedInputCnt++;

        // 핵심 연결: 리듬매니저에게 입력이 들어왔다고 알림
        OnPlayerInput("Input");
    }

    /// <summary>
    /// RhythmManagerTest가 CSV 시간과 입력 시간을 ±윈도우로 비교해서
    /// Perfect/Good/Miss를 계산한 뒤 여기로 보내줌.
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

        //Debug.Log($"[3-4] Judge={judgement} (P={perfectCnt}, G={goodCnt}, M={missCnt}, Submit={submittedInputCnt})");
    }

    /// <summary>
    /// handStopX 도달 시 최종 판정.
    /// 기본 룰:
    /// 1) Miss가 1개라도 있으면 Fail
    /// 2) (Good+Perfect) == requiredGoodOrPerfectCount 이면 Success
    /// 3) expectedInputCount > 0 이면, 제출 횟수/판정 횟수 부족한 경우 Fail 처리(안전장치)
    /// </summary>
    public void FinalJudge()
    {
        if (ended) return;
        ended = true;

        if (handMover != null) handMover.start = false;

        int goodOrPerfect = goodCnt + perfectCnt;

        Debug.Log($"[3-4] FinalJudge: P={perfectCnt}, G={goodCnt}, M={missCnt}, GP={goodOrPerfect}, Submit={submittedInputCnt}");

        // 1) Miss 있으면 무조건 실패
        if (missCnt > 0)
        {
            Fail();
            return;
        }

        // (선택) CSV에 Input이 expectedInputCount번인데, 판정/제출이 너무 적으면 실패로 방지
        // - 리듬매니저가 자동 Miss를 쏴주는 구조면 submittedInputCnt 체크는 굳이 안 해도 됨
        if (expectedInputCount > 0)
        {
            int totalJudge = perfectCnt + goodCnt + missCnt;
            if (totalJudge < expectedInputCount)
            {
                // 아직 판정이 덜 들어왔는데 손이 끝나버린 케이스 방지
                Fail();
                return;
            }
        }

        // 2) Good/Perfect 합이 요구치면 성공
        if (goodOrPerfect == requiredGoodOrPerfectCount)
        {
            Success();
            return;
        }

        // 3) 그 외는 실패
        Fail();
    }
}