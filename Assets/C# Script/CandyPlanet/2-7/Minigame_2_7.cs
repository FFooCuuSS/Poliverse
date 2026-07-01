using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_7 : MiniGameBase
{
    // 판정 범위 오버라이드
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "유지해라!";

    [Header("Stone")]
    [SerializeField] private Stone stone;

    [Header("입력 판정 창 (Stone 접촉 시점 기준 허용 오차, 초)")]
    [SerializeField] private float inputWindow = 0.2f;

    [Header("선입력 버퍼 (접촉 직전 이 시간 안의 클릭도 인정, 초)")]
    [SerializeField] private float preTouchBuffer = 0.15f;

    [Header("첫 접촉에만 추가로 주는 여유 시간 (초)")]
    [SerializeField] private float firstTouchExtraWindow = 0.3f;

    [Header("게임 시작 직후 클릭 무시 시간 (에디터 포커스 클릭 방지, 초)")]
    [SerializeField] private float inputIgnoreAfterStart = 0.3f;

    private float gameStartTime;

    private bool ended;
    public int missCount = 0;

    // CSV 차트 판정(OnJudgement) 대신, Jelly가 Stone에 닿는 횟수를 직접 세서
    protected override bool UseRhythmJudgementScore => false;

    private bool waitingForInput; // Stone 접촉 직후 ~ inputWindow 안에 입력 대기 중
    private float touchTimer;
    private float lastClickTime = -999f; // 선입력 버퍼용, 가장 최근 클릭 시각
    private bool isFirstTouch = true; // 게임 시작 후 첫 접촉인지 (여유시간 부여용)

    private void Start()
    {
        StartGame();
    }
    public override void StartGame()
    {
        base.StartGame();
        ended = false;

        missCount = 0;
        waitingForInput = false;
        touchTimer = 0f;
        isFirstTouch = true;
        gameStartTime = Time.time;

        if (stone != null)
        {
            // StartGame이 여러 경로(자체 Start(), 외부 RhythmManager 등)로 중복 호출돼도
            // 구독이 누적되지 않도록 먼저 해제 후 다시 구독
            stone.OnJellyTouch -= HandleJellyTouch;
            stone.OnJellyTouch += HandleJellyTouch;
        }
        else
        {
            Debug.LogWarning("[Minigame_2_7] stone 필드가 인스펙터에 연결되어 있지 않습니다. 입력 창이 절대 열리지 않습니다.");
        }

        // 추가 초기화
        // 예: instructionText.text = MinigameExplain;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (stone != null)
            stone.OnJellyTouch -= HandleJellyTouch;
    }

    private void Update()
    {
        if (ended) return;

        // 게임 시작 직후 일정 시간은 클릭 감지 자체를 스킵
        bool inputIgnored = Time.time - gameStartTime < inputIgnoreAfterStart;

        // 클릭은 waitingForInput 여부와 상관없이 항상 감지해서 시각 기록 (선입력 버퍼용)
        // 여기서는 연출을 재생하지 않는다 - 실제 접촉 시점(Judge 성공 시)에만 재생해서
        if (!inputIgnored && Input.GetMouseButtonDown(0))
        {
            lastClickTime = Time.time;

            if (waitingForInput)
            {
                float effectiveWindow = inputWindow + (isFirstTouch ? firstTouchExtraWindow : 0f);
                Judge(touchTimer <= effectiveWindow);
                return;
            }
        }

        if (!waitingForInput) return;

        touchTimer += Time.deltaTime;

        float timeoutWindow = inputWindow + (isFirstTouch ? firstTouchExtraWindow : 0f);
        if (touchTimer > timeoutWindow)
        {
            // 판정 창을 그냥 흘려보내면 실패로 집계
            Judge(false);
        }
    }

    private void HandleJellyTouch()
    {
        Debug.Log("Touch 발생");
        if (ended) return;

        // 접촉 직전 preTouchBuffer 안에 이미 클릭해뒀다면 그걸 인정
        if (Time.time - lastClickTime <= preTouchBuffer)
        {
            //Judge(true);
            return;
        }

        waitingForInput = true;
        touchTimer = 0f;
    }

    private void Judge(bool success)
    {
        Debug.Log($"Judge 호출 success={success}, Time={Time.time}");
        waitingForInput = false;
        isFirstTouch = false; // 첫 접촉 여유시간은 딱 한 번만 적용

        if (success)
        {
            stone.PlayBounceMotion(); // 실제 접촉 시점에 맞춰 튕기는 연출 
            ReportManualSuccess();
        }
        else
        {
            ReportManualFail();
            missCount++;
            Debug.Log($"현재 실수 횟수: {missCount}");
        }

        // 매 판정마다 실패 조건(미스 3회 이상)을 체크
        CheckGameResult();

        // 성공 여부는 JellySpawner가 모든 젤리를 다 처리했을 때 Succeed()를 직접 호출한다.
    }

    public void Succeed()
    {
        if (ended) return;
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
        if (action == "Slide")
        {
            // CSV 차트 기반 판정을 안 쓰기로 해서 현재는 미사용.
            // 필요 시 BGM 싱크 등 연출용으로만 활용
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
        // CSV 차트 기반 판정(RhythmManager가 쏘는 Perfect/Good/Miss)은 이 미니게임에서 더 이상 점수/실수 집계에 쓰지 않는다.
        // (Jelly-Stone 접촉 기반 매뉴얼 판정 - Judge() - 만 사용)
    }
    public void CheckGameResult()
    {
        if (IsInputLocked || ended) return;

        // 모두 Miss n번 이상 실패
        if (missCount >= 5)
        {
            Debug.Log("실패");
            Failure();
        }
    }
}