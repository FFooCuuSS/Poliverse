using UnityEngine;

// 3-2 거래 가방 전달
// 검은 가방이 4자리 중 랜덤으로 놓이고, 손은 노래 2.4초(horizontalMoveTime)에 그 가방 위에 온다.
// 판정은 미니게임이 직접 한다: 클릭 순간 손이 가방 위에서 몇 초 어긋났는지로 Perfect/Good/Miss.
// → CSV에 3-2 노트가 없어도 동작한다 (가방 위치가 랜덤이라 고정 노트로는 판정할 수 없음).
public class Minigame3_2remake : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 10f;

    protected override string MinigameTitle => "거래 가방 전달";

    protected override string MinigameExplain => "손이 오른쪽에서 왼쪽으로 이동합니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "타이밍에 맞춰 검은 가방을 잡으세요."
    };

    // 수동 판정: 대상 1개
    protected override bool UseRhythmJudgementScore => false;
    protected override int ManualTotalNodeCount => 1;

    [Header("Hand")]
    [SerializeField] private HandController handController;

    [Header("판정 범위 (초)")]
    [SerializeField] private float perfectWindow = 0.1f;
    [SerializeField] private float goodWindow = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    private bool clicked = false;
    private bool resolved = false;
    private bool isDemoMode = false;

    // 노래 시간 (RhythmManager가 돌고 있지 않으면 -1)
    public double SongTime
    {
        get
        {
            if (rhythmManager is RhythmManager rm && rm.IsRunning)
                return rm.SongTime;

            return -1;
        }
    }

    private void JudgeLog(string msg)
    {
        if (judgeLog) Debug.Log(msg);
    }

    public override void StartGame()
    {
        base.StartGame();
        IsInputLocked = false;

        clicked = false;
        resolved = false;
    }

    public override void SetDemoMode(bool isDemo)
    {
        base.SetDemoMode(isDemo);
        isDemoMode = isDemo;

        if (handController != null)
            handController.SetDemoMode(isDemo);

        Debug.Log("[3-2] 데모 모드: " + isDemoMode);
    }

    private void Update()
    {
        if (resolved) return;

        // 클릭 없이 손이 끝까지 가면 실패
        if (!clicked && handController != null && handController.IsHandPastEnd)
        {
            Resolve(false, "클릭 없음");
            return;
        }

        if (isDemoMode)
        {
            // 시범: 손이 가방 위에 오는 순간 자동 클릭
            if (!clicked && handController != null &&
                handController.GetTimingDiffSeconds() >= 0f)
            {
                DoGrab();
            }

            return;
        }

        if (Input.GetMouseButtonDown(0))
            SubmitInput();
    }

    public override void OnRhythmEvent(string action)
    {
        if (string.IsNullOrEmpty(action)) return;

        base.OnRhythmEvent(action.Trim());
    }

    public void SubmitInput()
    {
        if (isDemoMode) return;
        DoGrab();
    }

    // 플레이어/시범 공통: 한 번만
    private void DoGrab()
    {
        if (clicked || resolved) return;
        if (IsInputLocked) return;
        if (handController == null) return;

        clicked = true;

        float diff = handController.GetTimingDiffSeconds();
        float abs = Mathf.Abs(diff);

        string grade =
            abs <= perfectWindow ? "Perfect" :
            abs <= goodWindow ? "Good" : "Miss";

        JudgeLog($"[3-2 Judge] 클릭 song={SongTime:F3} diff={diff * 1000:F0}ms → {grade}");

        handController.StartGrabAction();
        Resolve(abs <= goodWindow, grade);
    }

    private void Resolve(bool success, string reason)
    {
        if (resolved) return;
        resolved = true;

        if (success) ReportManualSuccess();
        else ReportManualFail();

        if (handController != null)
            handController.SetJudgeResult(success);

        JudgeLog($"[3-2 Judge] 결과 {(success ? "성공" : "실패")} ({reason})");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // RhythmManager 판정은 쓰지 않는다 (수동 판정)
    }

    // 연습 시범 (CSV 노트가 있으면 그 박자, 없으면 Update에서 자동)
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (!isDemoMode) return;
        if (string.IsNullOrWhiteSpace(actionType)) return;
        if (actionType.Trim() != "Input") return;

        DoGrab();
    }

    // 구버전 호출 호환
    public void ReportHandTimeoutFail()
    {
    }
}