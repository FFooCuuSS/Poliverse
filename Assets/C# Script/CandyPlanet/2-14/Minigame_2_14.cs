using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_14 : MiniGameBase
{
    protected override float TimerDuration => 10f;
    protected override string MinigameTitle => "먹는 걸 막아라";
    protected override string MinigameExplain => "방패를 움직여 음식을 막으세요.";

    protected override bool UseRhythmJudgementScore => false;

    protected override int ManualTotalNodeCount => 5;

    public override void StartGame()
    {
        base.StartGame();
    }

    public override void OnRhythmEvent(string action)
    {
        if (string.IsNullOrEmpty(action)) return;

        action = action.Trim();

        if (action == "Show")
        {
            Debug.Log("Show → 음식 생성");

            FindObjectOfType<FoodSpawn_2_14>()
                ?.SpawnOneFood(1f);
        }
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        return;
    }

    public override void OnPlayerInput(string action = null)
    {
        return;
    }
}
