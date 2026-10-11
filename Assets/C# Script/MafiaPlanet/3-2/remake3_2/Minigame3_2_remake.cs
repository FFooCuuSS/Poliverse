using UnityEngine;

public class Minigame3_2remake : MiniGameBase
{
    // ===== 미니게임 기본 정보 =====
    protected override float TimerDuration => 10f;

    protected override string MinigameTitle => "거래 가방 전달";

    protected override string MinigameExplain => "바움쿠헨이 계속 굴러갑니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "장애물을 피해 좌우로 움직이세요."
    };

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;


    [Header("Hand")]
    [SerializeField] private HandController handController;


    private bool finished = false;
    private bool isDemoMode;

    private void Start()
    {
        StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();
        IsInputLocked = false;

        Debug.Log("[3-2] Minigame3_2remake 시작");
        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
    }

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        Debug.Log(
            $"[3-2] Demo Mode = {isDemoMode}"
        );

        if (handController != null)
        {
            handController.SetDemoMode(isDemo);
        }
        else
        {
            Debug.LogWarning(
                "[3-2] HandController가 연결되지 않았습니다."
            );
        }
    }

    private void Update()
    {
        if (finished)
        {
            return;
        }
        if (isDemoMode) return;

        // 좌클릭
        if (Input.GetMouseButtonDown(0))
        {
            SubmitInput();
        }
    }


    public override void OnRhythmEvent(string action)
    {
        if (finished || string.IsNullOrEmpty(action))
            return;

        action = action.Trim();

        switch (action)
        {
            case "Input":
                Debug.Log("[3-2] Input 이벤트 도착");
                break;
        }
    }


    public void SubmitInput()
    {
        if (finished || isDemoMode) return;

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