using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_11 : MiniGameBase
{
    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "쌓아보세요!";

    //public override float perfectWindowOverride => 0.1f;
    //public override float goodWindowOverride => 0.3f;
    //public override float hitWindowOverride => 0.5f;

    private bool ended;

    protected override bool UseRhythmJudgementScore => false;

    protected override int ManualTotalNodeCount => 5;

    [SerializeField] private MacaroonSpawn spawner;

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        action = action.Trim();

        switch (action)
        {
            case "Show":
                spawner.SpawnMacarons();
                break;
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        if (ended)
            return;

        Debug.Log("플레이어 클릭 입력");
    }

    public void MacaronSuccess()
    {
        ReportManualSuccess();

        Debug.Log("마카롱 쌓기 성공");
    }

    public void MacaronFail()
    {
        ReportManualFail();

        Debug.Log("마카롱 쌓기 실패");
    }

}
