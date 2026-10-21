using UnityEngine;

public class Minigame_3_15 : MiniGameBase, IPracticeDemoInput
{
    [SerializeField] private manager_3_15 manager;
    [SerializeField] private bool logJudgement = true;

    protected override float TimerDuration => 70f;
    protected override string MinigameTitle => "마법중년 마피아";

    protected override string MinigameExplain => "바움쿠헨이 계속 굴러갑니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "장애물을 피해 좌우로 움직이세요."
    };

    // 타이밍 판정이 아니라 맞음/틀림 판정 → Manual Score
    protected override bool UseRhythmJudgementScore => false;
    protected override int ManualTotalNodeCount => -1;

    private int hitCount;
    private int missCount;

    private bool isDemoMode = false;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        if (manager != null)
            manager.SetDemoMode(isDemoMode);
    }

    protected override void Awake()
    {
        base.Awake();
        if (manager == null)
            manager = GetComponentInChildren<manager_3_15>(true);
    }

    public override void StartGame()
    {
        base.StartGame();
        hitCount = 0;
        missCount = 0;

        if (manager != null)
        {
            manager.SetDemoMode(isDemoMode);
            manager.BeginGame();
        }
        else
        {
            Debug.LogError("[Minigame_3_15] manager_3_15 참조 없음");
        }
    }

    public void ReportHit(string reason = "")
    {
        ReportManualSuccess();
        hitCount++;
        if (logJudgement)
            Debug.Log($"<color=#6cf>[3-15 HIT]</color> {reason}  (Hit {hitCount} / Miss {missCount})");
    }

    public void ReportMiss(string reason = "")
    {
        ReportManualFail();
        missCount++;
        if (logJudgement)
            Debug.Log($"<color=#f66>[3-15 MISS]</color> {reason}  (Hit {hitCount} / Miss {missCount})");
    }
}