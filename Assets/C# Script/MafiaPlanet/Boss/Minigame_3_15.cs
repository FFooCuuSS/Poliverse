using UnityEngine;

public class Minigame_3_15 : MiniGameBase
{
    [SerializeField] private manager_3_15 manager;
    [SerializeField] private bool logJudgement = true;

    protected override float TimerDuration => 70f;
    protected override string MinigameExplain => "무기를 파괴하세요!";

    // 타이밍 판정이 아니라 맞음/틀림 판정 → Manual Score
    protected override bool UseRhythmJudgementScore => false;
    protected override int ManualTotalNodeCount => -1;

    private int hitCount;
    private int missCount;

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

        if (manager != null) manager.BeginGame();
        else Debug.LogError("[Minigame_3_15] manager_3_15 참조 없음");
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