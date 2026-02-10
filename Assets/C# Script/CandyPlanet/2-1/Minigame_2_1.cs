using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_1 : MiniGameBase
{
    // 판정 범위 오버라이드
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;

    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "숨어라!";

    private bool ended;
    private int missCount = 0;
    private int totalCount = 2;

    private DropCake dropCake;
    private PlayerSrChange srChange;

    //스코어
    private int score;
    [SerializeField] private int missAmount;
    [SerializeField] private int goodAmount;
    [SerializeField] private int perfectAmount;

    [SerializeField] private float duration;

    private void Start()
    {
        dropCake = GetComponent<DropCake>();
        srChange = GetComponentInChildren<PlayerSrChange>();
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
        if (action == "Hold")
        {
            dropCake.MoveDownAndBack(duration);
        }
        if (action == "Show")
        {
            CheckGameResult();
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
            srChange.ChangeSpriteTemporarily();
            missCount++;
            Debug.Log($"현재 실수 횟수: {missCount}");
        }
    }
    public void CheckGameResult()
    {
        if (IsInputLocked || ended) return;
        ended = true;
        // 모두 Miss인 경우 실패
        if (missCount >= totalCount)
        {
            Debug.Log("실패");
            Failure();
        }
        else
        {
            Debug.Log("성공");
            Succeed();
        }
    }
}
