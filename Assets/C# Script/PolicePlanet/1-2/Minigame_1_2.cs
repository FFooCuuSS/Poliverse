using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_2 : MiniGameBase
{
    // 판정 범위 오버라이드
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;

    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "체포해라!";

    private bool ended;

    private void Start()
    {
        StartGame();
    }
    public override void StartGame()
    {
        base.StartGame();
        ended = false;
        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
    }

    public void Succeed()
    {
        ended = true;
        Success();
    }
    public void Failure()
    {
        ended = true;
        Fail();
    }
    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");
        action = action.Trim();

        if (action == "Input")
        {

        }
    }

    public override void OnPlayerInput(string action = null)
    {
        // 입력 잠금 상태면 무시
        if (IsInputLocked) return;
        base.OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (IsInputLocked || ended) return;

        base.OnJudgement(judgement);

        if (judgement == JudgementResult.Miss)
        {
            Failure();
        }
        else if (judgement == JudgementResult.Good || judgement == JudgementResult.Perfect)
        {
            Succeed();
        }
     }
}
