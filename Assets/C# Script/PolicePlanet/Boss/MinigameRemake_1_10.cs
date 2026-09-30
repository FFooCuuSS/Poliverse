using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameRemake_1_10 : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 15f;
    protected override string MinigameTitle => "분류해라!";

    protected override string MinigameExplain => " 가운데서 경찰이나 죄수가 나옵니다. 1초 후 죄수면 왼쪽 경찰이면 오른쪽을 터치해주세요.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "2번씩 나와도 순서에 맞게 왼쪽 또는 오른쪽을 터치해주세요.",
        "곡이 빨라지면 누르는 타이밍도 빨라집니다 조심하세요"
    };

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;

    [Header("Refs")]
    [SerializeField] private Manager_1_10 manager;

    private bool ended;
    private bool inputOpen;
    private bool awaitingJudge;

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[1-10] SetDemoMode = {isDemoMode}");
    }

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

        action = action.Trim();
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        switch (action)
        {
            case "Show":
                inputOpen = false;
                awaitingJudge = false;
                manager?.SpawnPersonForShow();
                break;

            case "Input":
                inputOpen = true;
                awaitingJudge = false;
                manager?.OnInputWindowOpened();
                break;

            case "Move":
                inputOpen = false;
                awaitingJudge = false;
                manager?.MoveBothPlatforms();
                break;
        }
    }

    public void SubmitPlayerInput(string action = "Input")
    {
        if (IsDemoMode) return;

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
        awaitingJudge = false;

        switch (judgement)
        {
            case JudgementResult.Miss:
                manager?.OnMiss();
                break;

            case JudgementResult.Good:
            case JudgementResult.Perfect:
                manager?.OnAccepted(judgement);
                break;
        }
    }

    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim();

        if (actionType != "Input")
            return;

        if (ended)
            return;

        Debug.Log($"[1-10 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}");

        if (manager != null)
        {
            // 매니저를 통해 현재 대상의 정답(경찰/죄수)을 판별하여 즉시 올바른 방향으로 처리
            manager.ExecuteDemoAction();
        }
    }
}