using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameRemake_1_10 : MiniGameBase
{
    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "분류해라!";

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

        if (manager == null)
            manager = GetComponentInChildren<Manager_1_10>(true);

        if (manager != null)
            manager.OnMinigameStart(this);
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        action = action.Trim();

        // CSV가 Show/Input이므로 action도 Show/Input로 들어온다고 가정 (action=type 구조)
        if (action == "Show")
        {
            inputOpen = false;          // Show 중 입력 닫기(원하면 유지 가능)
            awaitingJudge = false;

            if (manager != null)
                manager.SpawnPersonForShow();
        }
        else if (action == "Input")
        {
            inputOpen = true;
            awaitingJudge = false;

            if (manager != null)
                manager.OnInputWindowOpened(); // 연속 Show 오프셋 리셋 등에 사용
        }
    }

    /// <summary>
    /// 외부(Manager_1_10)에서 스와이프 등 입력이 들어오면 호출.
    /// 실제 판정은 RhythmManager가 하고, 결과는 OnJudgement로 돌아온다.
    /// </summary>
    public void SubmitPlayerInput(string action = null)
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

        // Miss면 입력 실패 처리(일단 Move를 막고 다시 입력 기다리게)
        if (judgement == JudgementResult.Miss)
        {
            awaitingJudge = false; // 다시 입력 가능
            if (manager != null)
                manager.OnRhythmMiss();
            return;
        }

        // Good/Perfect면 “타이밍 통과”로 보고 실제 분류 이동 실행
        awaitingJudge = false;
        if (manager != null)
            manager.OnRhythmAccepted(judgement);
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
