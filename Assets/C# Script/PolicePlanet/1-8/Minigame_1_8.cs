using UnityEngine;

public class Minigame_1_8 : MiniGameBase, IPracticeDemoInput
{
    [SerializeField] private Manager_1_8 manager;

    protected override float TimerDuration => 10f;

    protected override string MinigameTitle =>
        "범인 가두기";

    protected override string MinigameExplain =>
        "죄수가 감옥 아래로 지나가면 화면을 터치해 가둬주세요.";

    private bool isDemoMode;

    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        Debug.Log(
            $"[1-8] DemoMode 변경 : {isDemoMode}"
        );
    }

    public override void StartGame()
    {
        base.StartGame();

        if (manager != null)
        {
            manager.ResetRoundState();
        }
    }

    public override void OnRhythmEvent(string action)
    {
        Debug.Log(
            $"[1-8] Rhythm Event : {action}"
        );

        // 기존 Rhythm CSV의 Show
        if (action == "Show")
        {
            if (manager != null)
            {
                manager.SpawnNextPrisoner();
            }
        }
    }

    public override void OnJudgement(
        JudgementResult judgement)
    {
        //  리듬 판정 사용 안 함
    }
}