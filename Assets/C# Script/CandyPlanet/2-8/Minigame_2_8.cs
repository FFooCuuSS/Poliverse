using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_8 : MiniGameBase
{
    // 판정 범위 오버라이드
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "조심해라!";

    private bool ended;
    private bool inputOpen;
    private bool awaitingJudge;

    public int missCount = 0;
    private int totalCount = 5;

    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private PlayerRotate playerRotate;
    [SerializeField] private PlayerSr playerSr;

    private void Start()
    {
        StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        inputOpen = false;
        awaitingJudge = false;

        missCount = 0;

        obstacleSpawner.Init();

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
        if (string.IsNullOrWhiteSpace(action)) return;

        action = action.Trim();
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        switch (action)
        {
            case "Show":
                // 새 판정 사이클 시작 - 입력 창 닫고, 장애물 낙하 + 판 기울임(예고)
                inputOpen = false;
                awaitingJudge = false;
                obstacleSpawner.SpawnObstacle(playerRotate);
                break;

            case "Input":
                // 입력 창 오픈
                inputOpen = true;
                awaitingJudge = false;
                break;
        }
    }

    // PlayerRotate가 클릭을 감지하면 방향과 함께 여기로 제출한다.
    // 게이트(입력 가능 여부/중복 제출 방지)와 실제 회전 적용을 여기서 전담한다.
    public void SubmitPlayerInput(bool isLeftClick)
    {
        if (ended) return;
        if (!inputOpen) return;
        if (awaitingJudge) return;

        awaitingJudge = true;

        playerRotate.ApplyClickRotation(isLeftClick ? 1f : -1f);

        OnPlayerInput("Input");
    }

    public override void OnPlayerInput(string action = null)
    {
        // 입력 잠금 상태면 무시
        if (IsInputLocked) return;
        base.OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended) return;

        base.OnJudgement(judgement);

        // 판정이 오는 즉시 입력 창을 닫는다 (다음 Input 이벤트 전까지는 클릭 무시)
        awaitingJudge = false;
        inputOpen = false;

        switch (judgement)
        {
            case JudgementResult.Miss:
                missCount++;
                Debug.Log($"현재 실수 횟수: {missCount}");

                // 현재 봉이 기울어진 방향을 기준으로 넘어지는 스프라이트 표시
                bool isLeft = playerRotate.CurrentAngle > 0f;
                playerSr.ShowFall(isLeft);
                break;

            case JudgementResult.Good:
            case JudgementResult.Perfect:

                break;
        }
    }
}