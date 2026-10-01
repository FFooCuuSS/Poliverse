using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame2_2 : MiniGameBase, IPracticeDemoInput
{
    // 판정 범위 오버라이드
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;
    protected override float TimerDuration => 5f;

    protected override string MinigameTitle => "고드름을 피해라";
    protected override string MinigameExplain => "하늘에서 고드름이 떨어집니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "화면을 터치하여 우측으로 피하세요."
    };

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[2-2] SetDemoMode = {isDemoMode}");
    }

    private bool ended;
    public int missCount = 0;
    [SerializeField] SpawnIcicle spawnIcicle;
  
    public override void StartGame()
    {
        base.StartGame();
        ended = false;
        missCount = 0;
        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");
        action = action.Trim();
        if (action == "PatternStart")
        {
            spawnIcicle.SpawnNext();
        }
    }
    public override void OnPlayerInput(string action = null)
    {
        if (IsDemoMode) return;

        // 입력 잠금 상태면 무시
        if (IsInputLocked) return;
        base.OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended) return;
        base.OnJudgement(judgement);

        if (judgement == JudgementResult.Miss)
        {
            missCount++;
            CheckGameResult();
        }
    }


    // 외부에서 호출 가능하도록 실패/성공 로직 캡슐화
    public void ForceMiss() => OnJudgement(JudgementResult.Miss);

    public void CheckGameResult()
    {
        if (IsInputLocked || ended) return;
        ended = true;
        // 모두 Miss 3번 이상 실패
        if (missCount >= 3)
        {
            Debug.Log("실패");
           // Failure();
        }
    }

    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim();

        if (actionType != "Input" && actionType != "PatternStart")
            return;

        if (ended)
            return;

        Debug.Log($"[2-2 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}");

        // 플레이어의 이동 스크립트를 찾아 데모 강제 이동 실행 (고드름 타이밍에 맞춰 회피 동작 수행)
        PlayerMoveByClick playerMove = GetComponentInChildren<PlayerMoveByClick>(true);
        if (playerMove != null)
        {
            playerMove.ForceMoveForDemo();
        }
        else
        {
            // 플레이어를 찾지 못한 경우 기본 입력 처리
            base.OnPlayerInput("Input");
        }
    }
}
