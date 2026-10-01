using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_1 : MiniGameBase, IPracticeDemoInput
{
    // 판정 윈도우 오버라이드
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;

    protected override float TimerDuration => 5f;
    protected override string MinigameTitle => "케이크 뭉개기";
    protected override string MinigameExplain => "하늘 위의 케이그가 타밍에 맞춰 내려옵니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "알맞은 구멍을 찾아 화면 터치하세요."
    };

    private bool ended;
    private int missCount = 0;
    private int totalCount = 2;

    private DropCake dropCake;
    private PlayerSrChange srChange;

    // 점수 관련
    private int score;
    [SerializeField] private int missAmount;
    [SerializeField] private int goodAmount;
    [SerializeField] private int perfectAmount;

    [SerializeField] private float duration;

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[2-1] SetDemoMode = {isDemoMode}");
    }

    public override void StartGame()
    {
        base.StartGame();
        dropCake = GetComponent<DropCake>();
        srChange = GetComponentInChildren<PlayerSrChange>(true);
        ended = false;
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        action = action.Trim();
        if (action == "Input")
        {
            dropCake.MoveDownAndBack(duration);
        }
    }
    public override void OnPlayerInput(string action = null)
    {
        if (IsDemoMode) return;

        // 입력 잠금 상태 확인
        if (IsInputLocked) return;
        base.OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (IsInputLocked || ended) return;

        base.OnJudgement(judgement);

        if (judgement == JudgementResult.Miss)
        {
            srChange.ChangeSpriteTemporarily();
            missCount++;
        }
    }
    public void CheckGameResult()
    {
        if (IsInputLocked || ended) return;
        ended = true;
        
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

        Debug.Log($"[2-1 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}");

        // 1. 플레이어 홀드/숨기기 연출(아래로 내려갔다 올라오는 모션)을 강제로 실행
        PlayerHold playerHold = GetComponentInChildren<PlayerHold>(true);
        if (playerHold != null)
        {
            playerHold.TriggerDemoHold();
        }

        // 2. 리듬 시스템에 성공 판정 전달
        base.OnPlayerInput("Input");
    }
}