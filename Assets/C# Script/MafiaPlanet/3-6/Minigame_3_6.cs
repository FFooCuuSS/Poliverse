using UnityEngine;

public class Minigame_3_6 : MiniGameBase, IPracticeDemoInput
{
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;
    protected override float TimerDuration => 10f;
    protected override string MinigameTitle => "도청기 설치";

    protected override string MinigameExplain => "바움쿠헨이 계속 굴러갑니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "장애물을 피해 좌우로 움직이세요."
    };

    // 이 미니게임은 RhythmManager의 입력 타이밍 판정을 쓰지 않는다.
    // CSV의 Input 노드 순간에 버튼이 눌려 있는지만 직접 검사한다.
    protected override bool UseRhythmJudgementScore => false;

    [Header("3-6 References")]
    [SerializeField] private LightController_3_6 lightController;

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.25f;
    public override float hitWindowOverride => 0.35f;

    private bool isRecording;
    private bool ended;

    private bool isDemoMode;

    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        // 데모에서는 플레이어가 녹음 중인 상태를 조작하지 못함
        isRecording = false;

        Debug.Log("[3-6] 데모 모드: " + isDemoMode);
    }

    public override void StartGame()
    {
        base.StartGame();

        isRecording = false;
        ended = false;

        if (lightController != null)
            lightController.TurnOffAll();
    }

    // 버튼 PointerDown에서 호출
    public void StartRecording()
    {
        if (isDemoMode || ended) return;
        if (clickSound != null)
        {
            if (GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            {
                GameRoot.Instance.Audio.PlaySfx(clickSound);
            }
        }

        isRecording = true;
        Debug.Log("[3-6] Recording Start");
    }

    // 버튼 PointerUp에서 호출
    public void StopRecording()
    {
        if (isDemoMode) return;
        isRecording = false;
        Debug.Log("[3-6] Recording Stop");
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        action = action.Trim();

        switch (action)
        {
            case "ShowGreen":
                lightController?.ShowGreen();
                break;

            case "ShowYellow":
                lightController?.ShowYellow();
                break;

            case "ShowRed":
                lightController?.ShowRed();
                break;

            case "RecordCheck":
                JudgeRecordingNode();
                break;

            case "Off":
                lightController?.TurnOffAll();
                break;

            case "End":
                ended = true;
                isRecording = false;
                lightController?.TurnOffAll();
                break;
        }
    }

    private void JudgeRecordingNode()
    {
        if (isDemoMode)
        {
            ReportManualSuccess();
            Debug.Log("[3-6] 데모 자동 성공 판정");
            return;
        }

        if (isRecording)
        {
            ReportManualSuccess();
           // Debug.Log("[3-6] Manual Success");
        }
        else
        {
            ReportManualFail();
           // Debug.Log("[3-6] Manual Fail");
        }
    }

    private void OnDisable()
    {
        isRecording = false;
    }
}