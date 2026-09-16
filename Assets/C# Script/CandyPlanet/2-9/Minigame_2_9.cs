using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_9 : MiniGameBase
{
    // 판정 범위 오버라이드
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "쌓아라!";

    private bool ended;
    private int totalCount = 5;

    [SerializeField] private CloudSpawner cloudSpawner;

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
        Success();
    }
    public void Failure()
    {
        Fail();
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");
        action = action.Trim();

        if (action == "Show")
        {
            // 이동만
            cloudSpawner.OnBeatEvent();
        }

        if (action == "Spawn")
        {
            // 생성만 — CSV에서 지정한 시점에 구름 등장
            cloudSpawner.SpawnCloudManual();
        }

        if (action == "Input")
        {
            // 기존과 동일: 판정은 Hand.cs → RhythmManager → OnJudgement 경로
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

        switch (judgement)
        {
            case JudgementResult.Miss:
                // 필요 시: PlaySFX("Miss");
                break;

            case JudgementResult.Good:
            case JudgementResult.Perfect:
                // 필요 시: PlaySFX("Hit");
                break;
        }
    }
}