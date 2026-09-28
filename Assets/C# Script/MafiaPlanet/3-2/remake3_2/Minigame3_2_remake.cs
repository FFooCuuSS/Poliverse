using UnityEngine;

public class Minigame3_2remake : MiniGameBase
{
    // ===== 미니게임 기본 정보 =====
    protected override float TimerDuration => 10f;

    protected override string MinigameExplain =>
        "미니게임 3-2 설명을 여기에 넣기";

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;


    [Header("Hand")]
    [SerializeField] private HandController handController;


    private bool finished = false;


    private void Start()
    {
        IsInputLocked = false;

        Debug.Log("[3-2] Minigame3_2remake 시작");

        base.StartGame();
    }


    private void Update()
    {
        if (finished)
        {
            return;
        }

        // 좌클릭
        if (Input.GetMouseButtonDown(0))
        {
            SubmitInput();
        }
    }


    public override void OnRhythmEvent(string action)
    {
        if (finished)
        {
            return;
        }

        switch (action)
        {
            case "Input":
                Debug.Log("[3-2] Input 이벤트 도착");
                break;
        }
    }


    public void SubmitInput()
    {
        if (finished)
        {
            return;
        }

        Debug.Log("[3-2] 좌클릭");


        // 클릭하면 Hand 동작 시작
        if (handController != null)
        {
            handController.StartGrabAction();
        }


        // 리듬매니저에도 입력 전달
        OnPlayerInput("Input");
    }


    // ===== 리듬매니저 판정 결과 =====

    public override void OnJudgement(
        JudgementResult judgement)
    {
        // 판정 결과는 로그만 출력
        Debug.Log("[3-2] 판정 결과 : " + judgement);
    }


    /// <summary>
    /// Hand가 클릭 없이 x = -12까지 도착했을 때 호출
    /// </summary>
    public void ReportHandTimeoutFail()
    {
        if (finished)
        {
            return;
        }

        Debug.Log(
            "[3-2] Hand가 -12까지 도착 - Manual Fail"
        );

        base.ReportManualFail();
    }
}