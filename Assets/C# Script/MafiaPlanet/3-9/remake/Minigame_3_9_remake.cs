using UnityEngine;

public class Minigame_3_9_remake : MiniGameBase
{
    // ===== 미니게임 기본 정보 =====

    protected override float TimerDuration => 20f;

    protected override string MinigameExplain =>
        "빛난 순서를 기억해서 같은 순서로 클릭하세요.";


    // RhythmManager 판정 사용 안 함
    // ReportManualSuccess / Fail을 사용한다.
    protected override bool UseRhythmJudgementScore =>
        false;


    private void Start()
    {
        IsInputLocked = false;

        Debug.Log(
            "[3-9] Minigame 시작"
        );


        // 점수 집계 초기화
        base.StartGame();


        // 이번 게임은 패턴이 총 6개다.
        SetRuntimeTotalNodeCount(6);
    }


    /// <summary>
    /// Manager3_9에서 정답 입력 시 호출
    /// </summary>
    public void ReportPatternSuccess()
    {
        ReportManualSuccess();


        Debug.Log(
            "[3-9] Manual Success +1"
        );
    }


    /// <summary>
    /// Manager3_9에서 오답 입력 시 호출
    /// </summary>
    public void ReportPatternFail()
    {
        ReportManualFail();


        Debug.Log(
            "[3-9] Manual Fail +1"
        );
    }


    /// <summary>
    /// 6번의 입력이 전부 끝난 후 호출
    /// </summary>
    public void FinishGame()
    {
        // 최종 점수 계산
        ScoreResult result =
            FinalizeScoreSession();


        Debug.Log(
            "[3-9] 최종 결과" +
            " / Total = " +
            result.totalNode +
            " / Success = " +
            result.perfect +
            " / Fail = " +
            result.miss
        );


        // =========================
        // 6개 전부 맞아야 성공
        // =========================

        if (result.perfect == 6)
        {
            Debug.Log(
                "[3-9] 최종 성공"
            );

            Success();
        }
        else
        {
            Debug.Log(
                "[3-9] 최종 실패"
            );

            Fail();
        }
    }
}