using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_10 : MiniGameBase
{
    // 판정 윈도우 오버라이드
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;
    // 라운드(패턴) 1개당 6초(시스템 3초 + 플레이어 3초). 등록된 라운드 수만큼 곱해서 전체 길이를 알려준다.
    protected override float TimerDuration =>
        6f * Mathf.Max(1, temperatureController != null && temperatureController.Pattern != null
            ? temperatureController.Pattern.PatternCount
            : 1);
    protected override string MinigameExplain => "초콜릿 젓기!";

    [Header("참조")]
    [SerializeField] private TemperatureController temperatureController;
    [SerializeField] private ScoopDrag scoopDrag;

    private void Start()
    {
        StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();

        // 이 미니게임은 HeatPattern(인스펙터에서 조정하는 라운드별 패턴)을 기준으로
        // 총 노드 수(=전체 라운드에서 플레이어가 맞춰야 할 입력 개수 합)를 런타임에 직접 지정한다.
        if (temperatureController != null && temperatureController.Pattern != null)
            SetRuntimeTotalNodeCount(temperatureController.Pattern.TotalDropCount);

        if (temperatureController != null)
        {
            // TemperatureController의 판정 윈도우를 이 미니게임의 오버라이드 값과 동기화
            temperatureController.SetJudgementWindows(perfectWindowOverride, goodWindowOverride, hitWindowOverride);

            temperatureController.OnSystemDrop -= HandleSystemDrop;
            temperatureController.OnSystemDrop += HandleSystemDrop;

            temperatureController.OnPlayerPhaseStarted -= HandlePlayerPhaseStarted;
            temperatureController.OnPlayerPhaseStarted += HandlePlayerPhaseStarted;

            temperatureController.OnInputJudged -= HandleInputJudged;
            temperatureController.OnInputJudged += HandleInputJudged;

            temperatureController.OnRoundFinished -= HandleRoundFinished;
            temperatureController.OnRoundFinished += HandleRoundFinished;

            temperatureController.OnAllPatternsFinished -= HandleAllPatternsFinished;
            temperatureController.OnAllPatternsFinished += HandleAllPatternsFinished;

            temperatureController.BeginPattern();
        }

        if (scoopDrag != null)
        {
            scoopDrag.OnSwipeDetected -= HandleSwipeDetected;
            scoopDrag.OnSwipeDetected += HandleSwipeDetected;
        }
    }

    // 시스템 콜(0~3초) 구간에서 온도계가 한 박 내려갈 때마다 호출됨
    private void HandleSystemDrop(int index)
    {
        OnRhythmEvent("Show");
    }

    private void HandlePlayerPhaseStarted()
    {
        // 필요 시 플레이어 응답 구간 시작 연출/사운드를 여기에 추가
    }

    // ScoopDrag에서 좌<->우 스와이프가 감지될 때 호출됨
    private void HandleSwipeDetected()
    {
        OnPlayerInput("Swipe");
    }

    // TemperatureController가 스와이프(또는 자동 미스)를 판정할 때마다 호출됨
    private void HandleInputJudged(JudgementResult judgement, int index)
    {
        OnJudgement(judgement);
    }

    // 라운드(패턴) 하나가 끝날 때마다 호출됨 (다음 라운드가 있으면 TemperatureController가 자동으로 이어서 재생)
    private void HandleRoundFinished(int patternIndex)
    {
        Debug.Log($"{gameObject.name} 라운드 종료: {patternIndex + 1}/{temperatureController.TotalPatternCount}");
    }

    // 등록된 모든 라운드가 순서대로 전부 끝났을 때 한 번 호출됨
    private void HandleAllPatternsFinished()
    {
        Debug.Log($"{gameObject.name} 전체 패턴 종료");
        // 점수 집계/클리어 판정은 별도 시스템에서 처리 (여기서는 다루지 않음)
    }

    public override void OnRhythmEvent(string action)
    {
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");
        action = action.Trim();
        if (action == "Show")
        {
        }
        if (action == "Drop")
        {
        }
        if (action == "Input")
        {
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        // 입력 잠금 상태면 무시
        if (IsInputLocked) return;

        // 주의: base.OnPlayerInput(action)을 호출하지 않는다.
        // base.OnPlayerInput은 (씬에 다른 용도로 바인딩되어 있을 수 있는) rhythmManager.ReceivePlayerInput()을
        // 함께 호출하는데, 이 미니게임은 판정을 전적으로 temperatureController(HeatPattern 기반 로컬 타이밍)로만 한다.
        // 둘 다 호출하면 스와이프 1회당 OnJudgement가 두 번(외부 rhythmManager 판정 + 로컬 판정) 불려서
        // 실제 입력 횟수와 판정 개수가 어긋난다.
        temperatureController?.OnSwipe();
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (IsInputLocked) return;

        base.OnJudgement(judgement);
    }
}