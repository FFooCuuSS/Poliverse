using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameRemake_1_10 : MiniGameBase
{
    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "분류해라!";

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;

    [Header("Refs")]
    [SerializeField] private Manager_1_10 manager;   // 같은 프리팹/자식에서 드래그 or Find로 세팅

    private bool ended;
    private bool inputOpen;          // Input 구간인지
    private bool awaitingJudge;      // 입력 후 판정 대기중(중복 입력 방지용)

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        inputOpen = false;
        awaitingJudge = false;

        if (manager != null)
            manager.OnMinigameStart(this);
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        action = action.Trim();

        if (action == "Show")
        {
            inputOpen = false;
            awaitingJudge = false;

            if (manager != null)
                manager.SpawnPersonForShow();
        }
        else if (action == "Input")
        {
            inputOpen = true;
            awaitingJudge = false;

            if (manager != null)
                manager.OnInputWindowOpened();
        }
        else if (action == "Move")
        {
            if (manager != null)
                manager.MoveBothPlatforms();
        }
    }

    /// <summary>
    /// 외부(Manager_1_10)에서 스와이프 등 입력이 들어오면 호출.
    /// 실제 판정은 RhythmManager가 하고, 결과는 OnJudgement로 돌아온다.
    /// </summary>
    public void SubmitPlayerInput(string action = "Input")
    {
        if (ended) return;
        if (!inputOpen) return;
        if (awaitingJudge) return;

        awaitingJudge = true;
        OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended) return;
        base.OnJudgement(judgement);

        // 판정 대기 해제
        awaitingJudge = false;

        // 판정별로 Manager의 public 함수 호출
        switch (judgement)
        {
            case JudgementResult.Miss:
                manager.OnMiss();
                break;

            case JudgementResult.Good:
                manager.OnAccepted(judgement);
                break;
            case JudgementResult.Perfect:
                manager.OnAccepted(judgement);
                break;
        }
    }


    public void Succeed()
    {
        if (ended) return;
        ended = true;
        Success();
    }

    public void Failure()
    {
        if (ended) return;
        ended = true;
        Fail();
    }
}