using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_1 : MiniGameBase, IPracticeDemoActionInput
{
    protected override float TimerDuration => 20f;

    protected override string MinigameTitle => "결투 신청";
    protected override string MinigameExplain => "죄수가 순서대로 나타나 타이밍을 알려줍니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "배경이 어두워지면 타이밍에 맞게 죄수를 터치해주세요."
    };

    [Header("데모 연출용 참조")]
    [SerializeField] private PistolDrag pistolDrag;
    [SerializeField] private PistolUp pistolUp;
    [SerializeField] private Transform pistolTransform;
    [SerializeField] private Button_3_1 button3_1;

    [Header("데모 연출 설정")]
    [SerializeField] private float demoPullDistance = 0.5f;
    [SerializeField] private float demoHandAngle = 45f;


    [Header("데모 속도 (초, 작을수록 빠름)")]
    [SerializeField] private float demoHandDuration = 0.2f;
    [SerializeField] private float demoPistolDuration = 0.2f;

    public static Minigame_3_1 Instance { get; private set; }

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        if (isDemo)
        {
            FindReferencesSafely();
            if (button3_1 != null) button3_1.ResetDemo();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public override void StartGame()
    {
        base.StartGame();
        FindReferencesSafely();
    }

    private void FindReferencesSafely()
    {
        if (pistolDrag == null)
            pistolDrag = FindObjectOfType<PistolDrag>();

        if (pistolUp == null && pistolDrag != null)
            pistolUp = pistolDrag.GetComponent<PistolUp>();
        if (pistolUp == null)
            pistolUp = FindObjectOfType<PistolUp>();

        if (button3_1 == null)
            button3_1 = FindObjectOfType<Button_3_1>();

        if (pistolTransform == null && pistolDrag != null)
        {
            pistolTransform = pistolDrag.transform;
        }
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(JudgementResult.Perfect);
        Debug.Log("[3-1] 강제 퍼펙트 판정 처리 완료");

        // 임시: 데모 매니저가 연출을 호출하지 않으므로 여기서 직접 실행
        if (!isDemoMode) return;
        PlayNextDemoAction();
    }

    private int demoStep = 0;
    private void PlayNextDemoAction()
    {
        // 로그상 순서: Swipe(당기기) → Tap(상승) 반복
        string type = (demoStep % 2 == 0) ? "Swipe" : "Tap";
        demoStep++;
        ExecutePracticeAction(demoStep, type);
    }

    public void SubmitManualInput(string actionName)
    {
        if (IsInputLocked) return;
        OnPlayerInput(actionName);
    }

    // --- IPracticeDemoInput 구현 ---
    public void ExecutePracticeAction(int actionIndex, string actionType)
    {
        Debug.Log($"[3-1 Demo] ExecutePracticeAction 진입 / isDemoMode={isDemoMode}, type={actionType}");

        if (!isDemoMode) return;

        FindReferencesSafely();

        if (pistolDrag != null)
            pistolDrag.canPull = false;

        Debug.Log($"[3-1 Demo] 액션 실행됨 -> actionType: {actionType}, index: {actionIndex}");

        // 1. 당기는 동작
        if (actionType == "Input1" || actionType == "Swipe" || actionType == "Input")
        {
            if (pistolTransform == null)
            {
                Debug.LogWarning("[3-1 Demo] pistolTransform이 없습니다.");
                return;
            }

            // PistolDrag와 동일한 방향 계산 (+90도)
            float baseAngle = pistolDrag != null ? pistolDrag.angleInDegrees : 0f;
            float angle = (baseAngle + 90f) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f).normalized;

            Vector3 targetPos = pistolTransform.position + direction * demoPullDistance;

            if (button3_1 != null)
            {
                float handRad = demoHandAngle * Mathf.Deg2Rad;
                Vector2 handDir = new Vector2(Mathf.Cos(handRad), Mathf.Sin(handRad));

                button3_1.PlayDemoPullAnimation(demoHandDuration, demoPullDistance, handDir, () =>
                {
                    MovePistol(targetPos);
                });
            }
            else
            {
                MovePistol(targetPos);
            }
        }
        // 2. 올라가는 동작
        else if (actionType == "Input2" || actionType == "Tap")
        {
            if (pistolUp != null)
            {
                pistolTransform.DOKill(); // 당기기 트윈 정리
                pistolUp.ForceRise();
            }
            else
            {
                Debug.LogWarning("[3-1 Demo] PistolUp 컴포넌트를 찾지 못했습니다.");
            }
        }
    }

    private void MovePistol(Vector3 targetPos)
    {
        if (pistolTransform == null) return;

        pistolTransform.DOKill();
        pistolTransform.DOMove(targetPos, demoPistolDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                Debug.Log("[3-1 Demo] 총 당기기 연출 완료");
            });
    }
}