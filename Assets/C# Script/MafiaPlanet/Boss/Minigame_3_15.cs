using UnityEngine;

public class Minigame_3_15 : MiniGameBase, IPracticeDemoInput
{
    [SerializeField] private manager_3_15 manager;
    [SerializeField] private bool logJudgement = true;

    protected override float TimerDuration => 70f;
    protected override string MinigameTitle => "마법중년 마피아";

    protected override string MinigameExplain => "마법중년 빔을 피해 움직이세요";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "플레이어를 잡은 채로 움직이세요. 거울은 빔을 반사해 막아줍니다."
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