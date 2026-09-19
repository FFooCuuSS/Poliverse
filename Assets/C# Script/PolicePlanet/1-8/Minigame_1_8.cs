using UnityEngine;

public class Minigame_1_8 : MiniGameBase
{
    [SerializeField] private Manager_1_8 manager;

    protected override float TimerDuration => 10f;

    protected override string MinigameTitle => "모두 가둬라!";

    protected override string MinigameExplain => "죄수가 감옥 아래로 지나가면 화면을 터치해 가둬주세요.";

    private void Start()
    {
        //StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();
        if (manager != null)
            manager.ResetRoundState();
    }

    public override void OnRhythmEvent(string action)
    {
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        if (action == "Show")
        {
            manager.SpawnNextPrisoner();
        }
    }

    // 이 게임은 리듬 판정 사용 안 함
    public override void OnJudgement(JudgementResult judgement)
    {
        //
    }
}