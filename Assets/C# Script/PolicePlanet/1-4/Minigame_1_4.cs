using System.Collections.Generic;
using UnityEngine;


public class Minigame_1_4 : MiniGameBase
{
    // 판정 범위 오버라이드
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.4f;
    public override float hitWindowOverride => 0.8f;

    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "리듬에 맞춰 악세서리를 제거하세요.";

    private List<Accessory> orderedAccessories;
    private int missCount = 0;
    private int processedCount = 0;
    private bool ended;

    private void Start()
    {
        StartGame();
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");
        action = action.Trim();

        if (action == "Swipe")
        {
            // 판정 결과와 상관없이 현재 순서의 악세서리는 제거 (다음 단계를 위해)
            RemoveNextAccessory();
            //processedCount++;
        }
    }

    public void SetAccessoryOrder(List<Accessory> accessories)
    {
        orderedAccessories = accessories;

        foreach (var acc in orderedAccessories)
            acc.Init(this);
    }

    public override void StartGame()
    {
        base.StartGame();
        missCount = 0;
        processedCount = 0;
        ended = false;
    }

    public override void OnPlayerInput(string action = null)
    {
        // 입력 잠금 상태면 무시
        if (IsInputLocked) return;
        base.OnPlayerInput(action);
    }

    // RhythmManager로부터 판정 결과를 전달받음
    public override void OnJudgement(JudgementResult judgement)
    {
        if(IsInputLocked || IsSuccess || ended) return;

        base.OnJudgement(judgement);

        if (judgement == JudgementResult.Miss)
        {
            missCount++;
            Debug.Log($"현재 실수 횟수: {missCount}");
        }
        
        processedCount++;

        // 모든 악세서리(3개)가 처리되었는지 확인
        if (processedCount >= orderedAccessories.Count)
        {
            CheckGameResult();
        }
    }

    private void RemoveNextAccessory()
    {
        foreach (var acc in orderedAccessories)
        {
            if (!acc.IsRemoved)
            {
                acc.Remove();
                break;
            }
        }
    }
    private void CheckGameResult()
    {
        if (IsInputLocked || ended) return;
        ended = true;
        // 3개 모두 Miss인 경우 실패
        if (missCount >= 3)
        {
            Debug.Log("실패");
            Fail();
        }
        else
        {
            Debug.Log("성공");
            Success();
        }
    }
}
