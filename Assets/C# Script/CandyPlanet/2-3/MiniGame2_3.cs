using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_3 : MiniGameBase
{
    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 1f;

    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "가동시켜라!";

    [Header("총 Input 수")]
    [SerializeField] private int maxInputCount = 3;

    [SerializeField] private MovingIconController2_3 movingIcon;

    private int handledInputCount = 0;
    private bool hasAnySuccess = false;
    private List<bool> inputResults = new List<bool>();
    private bool inputOpen = false;
    private bool awaitingJudge = false;

    public bool IsInputTiming { get; private set; }

    public override void StartGame()
    {
        base.StartGame();
        handledInputCount = 0;
        hasAnySuccess = false;
        inputResults.Clear();
        inputOpen = false;
        awaitingJudge = false;
    }

    public override void OnRhythmEvent(string action)
    {
        Debug.Log($"OnRhythmEvent 호출됨, action: {action}");
        base.OnRhythmEvent(action);

        if (action == "Input")
        {
            inputOpen = true;
            awaitingJudge = false;
            IsInputTiming = true;

            // Input 등장 시 기록 초기화
            if (handledInputCount < maxInputCount)
            {
                inputResults.Add(false); // 아직 성공/실패 기록 없음
                Debug.Log($"Input 등장 ({handledInputCount + 1}/{maxInputCount})");
            }

            // 자동 Miss 처리 (예: 0.5초 안에 입력 없으면 실패)
            Invoke(nameof(AutoMissInput), 0.5f);
        }
    }

    public void SubmitPlayerInput(string action = "Input")
    {
        if (!inputOpen || awaitingJudge || handledInputCount >= maxInputCount)
            return;

        awaitingJudge = true;

        bool success = movingIcon.IsInCorrectZone;

        // 이번 Input 결과 저장
        inputResults[handledInputCount] = success;

        if (success)
            hasAnySuccess = true;

        Debug.Log($"Input {handledInputCount + 1} : {(success ? "성공" : "실패")}");

        inputOpen = false;
        IsInputTiming = false;

        handledInputCount++;
        CheckIfAllInputsHandled();
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (!inputOpen || handledInputCount >= maxInputCount)
            return;

        awaitingJudge = false;
        inputOpen = false;
        IsInputTiming = false;

        // 성공 판정이면
        if (judgement == JudgementResult.Perfect || judgement == JudgementResult.Good)
        {
            inputResults[handledInputCount] = true;
            hasAnySuccess = true;
            Debug.Log($"Input {handledInputCount + 1} 성공!");
        }
        else
        {
            Debug.Log($"Input {handledInputCount + 1} 실패!");
        }

        handledInputCount++;
        CheckIfAllInputsHandled();
    }

    private void AutoMissInput()
    {
        if (!inputOpen) return;

        inputOpen = false;
        IsInputTiming = false;

        inputResults[handledInputCount] = false;

        Debug.Log($"Input {handledInputCount + 1} 자동 실패");

        handledInputCount++;
        CheckIfAllInputsHandled();
    }

    private void CheckIfAllInputsHandled()
    {
        if (handledInputCount < maxInputCount)
            return;

        Debug.Log("모든 Input 처리 완료 → 최종 판정");

        if (hasAnySuccess)
        {
            Debug.Log("최종 성공!");
            Success();
        }
        else
        {
            Debug.Log("최종 실패!");
            Fail();
        }
    }
}
