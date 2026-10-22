using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_1 : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 20f;

    protected override string MinigameTitle => "젤리 블록 철거!";
    protected override string MinigameExplain => "죄수가 순서대로 나타나 타이밍을 알려줍니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "배경이 어두워지면 타이밍에 맞게 죄수를 터치해주세요."
    };

    [Header("데모 연출용 참조")]
    [SerializeField] private PistolDrag pistolDrag;
    [SerializeField] private PistolUp pistolUp;
    [SerializeField] private Transform pistolTransform;

    [Header("데모 연출 설정")]
    [SerializeField] private float demoPullDistance = 0.5f;
    [SerializeField] private float demoPullDuration = 0.2f;

    public static Minigame_3_1 Instance { get; private set; }

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[3-1] SetDemoMode 설정됨: {isDemoMode}");
    }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public override void StartGame()
    {
        base.StartGame();

        if (pistolDrag == null) pistolDrag = FindObjectOfType<PistolDrag>();
        if (pistolUp == null) pistolUp = FindObjectOfType<PistolUp>();

        if (pistolTransform == null && pistolDrag != null)
        {
            pistolTransform = pistolDrag.transform;
        }
    }

    // --- 리듬 판정 오버라이드: 어떤 입력이든 무조건 Perfect 처리 ---
    public override void OnJudgement(JudgementResult judgement)
    {
        // 강제로 Perfect 판정으로 고정 혹은 가로채기
        base.OnJudgement(JudgementResult.Perfect);
        Debug.Log("[3-1] 강제 퍼펙트 판정 처리 완료");
    }

    // --- 플레이어 수동 입력 시 처리 (필요시 호출) ---
    public void SubmitManualInput(string actionName)
    {
        if (IsInputLocked) return;
        OnPlayerInput(actionName);
    }

    // --- IPracticeDemoInput 구현 ---
    public void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (!isDemoMode)
            return;

        if (pistolDrag == null)
            pistolDrag = FindObjectOfType<PistolDrag>();

        if (pistolUp == null)
            pistolUp = FindObjectOfType<PistolUp>();

        if (pistolDrag == null || pistolUp == null)
        {
            Debug.LogWarning("[Minigame_3_1] PistolDrag 또는 PistolUp을 찾을 수 없습니다.");
            return;
        }

        if (pistolTransform == null)
            pistolTransform = pistolDrag.transform;

        // 총을 당기는 동작
        if (actionType == "Swipe" || actionType == "Input1")
        {
            pistolDrag.canPull = false;

            float angle = pistolDrag.angleInDegrees * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(
                Mathf.Cos(angle),
                Mathf.Sin(angle),
                0f
            );

            Vector3 targetPos =
                pistolTransform.position + direction * demoPullDistance;

            pistolTransform.DOKill();

            pistolTransform.DOMove(targetPos, demoPullDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    if (isDemoMode && pistolUp != null)
                    {
                        pistolUp.goingUp = true;
                    }
                });
        }

        // 총을 올리는 입력도 데모에서는 자동으로 실행
        else if (actionType == "Tap" || actionType == "Input2")
        {
            pistolUp.goingUp = true;
        }
    }
}