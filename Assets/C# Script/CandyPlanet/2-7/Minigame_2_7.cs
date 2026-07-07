using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_7 : MiniGameBase
{
    public override float perfectWindowOverride => 0.15f; 
    public override float goodWindowOverride => 0.2f;     
    public override float hitWindowOverride => 1f;        

    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "유지해라!";

    [Header("Stone")]
    [SerializeField] private Stone stone;


    [Header("첫 접촉에만 추가로 주는 여유 시간 (초)")]
    [SerializeField] private float firstTouchExtraWindow = 0.3f;

    [Header("게임 시작 직후 클릭 무시 시간 (에디터 포커스 클릭 방지, 초)")]
    [SerializeField] private float inputIgnoreAfterStart = 0.3f;

    private float gameStartTime;

    private bool ended;
    public int missCount = 0;

    // CSV 차트 판정(OnJudgement) 대신, Jelly가 Stone에 닿는 횟수를 직접 세서 매뉴얼 판정
    protected override bool UseRhythmJudgementScore => false;

    private bool waitingForInput; // Stone 접촉 직후 ~ 판정 창 안에 입력 대기 중
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
            stone.OnJellyTouch -= HandleJellyTouch;
            stone.OnJellyTouch += HandleJellyTouch;
        }
        else
        {
            Debug.LogWarning("[Minigame_2_7] stone 필드가 인스펙터에 연결되어 있지 않습니다. 입력 창이 절대 열리지 않습니다.");
        }
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
        if (!inputIgnored && Input.GetMouseButtonDown(0))
        {
            lastClickTime = Time.time;

            if (waitingForInput)
            {
                // inputWindow 대신 goodWindowOverride 사용
                float effectiveWindow = goodWindowOverride + (isFirstTouch ? firstTouchExtraWindow : 0f);
                Judge(touchTimer <= effectiveWindow);
                return;
            }
        }

        if (!waitingForInput) return;

        touchTimer += Time.deltaTime;

        // inputWindow 대신 goodWindowOverride 사용
        float timeoutWindow = goodWindowOverride + (isFirstTouch ? firstTouchExtraWindow : 0f);
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

        if (Time.time - lastClickTime <= perfectWindowOverride)
        {
            Judge(true); 
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

        // 매 판정마다 실패 조건(미스 5회 이상)을 체크
        CheckGameResult();
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
    }

    public override void OnPlayerInput(string action = null)
    {
        // 입력 잠금 상태면 무시
        if (IsInputLocked) return;
        base.OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // 매뉴얼 판정(Jelly-Stone 접촉 기반)을 사용하므로 비워둠
    }

    public void CheckGameResult()
    {
        if (IsInputLocked || ended) return;

        // 미스 5번 이상 시 실패 조건 유지
        if (missCount >= 5)
        {
            Debug.Log("실패");
            Failure();
        }
    }
}