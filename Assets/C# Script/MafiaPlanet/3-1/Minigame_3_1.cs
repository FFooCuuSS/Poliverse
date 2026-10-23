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

        if (!IsInputLocked)
        {
            OnPlayerInput(actionType);
        }

        Button_3_1[] buttons = FindObjectsOfType<Button_3_1>();
        PistolDrag[] drags = FindObjectsOfType<PistolDrag>();

        Button_3_1 targetButton = null;
        PistolDrag targetDrag = null;

        if (buttons.Length > 0)
        {
            Array.Sort(buttons, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            Array.Sort(drags, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            int targetIdx = (actionIndex / 2) % buttons.Length;
            targetButton = buttons[targetIdx];
            targetDrag = drags[targetIdx];
        }

        if (targetDrag == null) targetDrag = pistolDrag;
        if (targetDrag != null) targetDrag.canPull = false;

        PistolUp targetUp = targetDrag != null ? targetDrag.GetComponent<PistolUp>() : pistolUp;

        // 1. Input1 (또는 Swipe): 버튼(선)과 총이 함께 당겨지는 연출
        if (actionType == "Input1" || actionType == "Swipe")
        {
            if (targetButton != null)
            {
                targetButton.PlayDemoPullAnimation(demoPullDuration, demoPullDistance, null);
            }

            if (targetDrag != null)
            {
                Transform pTransform = targetDrag.transform;
                float angle = targetDrag.angleInDegrees * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

                // 왼쪽(X < 0)에 있는 총은 X 방향 대칭 반전
                if (pTransform.position.x < 0f)
                {
                    direction.x = -direction.x;
                }

                Vector3 targetPos = pTransform.position + direction * demoPullDistance;

                pTransform.DOKill();
                pTransform.DOMove(targetPos, demoPullDuration)
                    .SetEase(Ease.OutQuad);
            }
        }
        // 2. Input2 (또는 Tap): 총이 위로 솟아오름
        else if (actionType == "Input2" || actionType == "Tap")
        {
            if (targetUp != null)
            {
                targetUp.goingUp = true;
            }
        }
    }
}