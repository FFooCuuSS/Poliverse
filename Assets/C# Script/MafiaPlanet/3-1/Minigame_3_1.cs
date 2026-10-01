using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_1 : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 3f;

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

    // --- IPracticeDemoInput 구현 ---
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        Debug.Log($"[3-1 Demo] ExecutePracticeAction 호출됨! Index: {actionIndex}, Type: '{actionType}'");

        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim();

        // 1단계: 당기는 액션 (Swipe 또는 Input1)
        if (string.Equals(actionType, "Swipe", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(actionType, "Input1", StringComparison.OrdinalIgnoreCase))
        {
            if (pistolDrag != null)
            {
                pistolDrag.canPull = true;
            }

            if (pistolTransform != null)
            {
                float angleRad = (pistolDrag.angleInDegrees + 90f) * Mathf.Deg2Rad;
                Vector3 pullDir = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f).normalized;
                Vector3 targetPos = pistolTransform.position + pullDir * demoPullDistance;

                pistolTransform.DOMove(targetPos, demoPullDuration).SetEase(Ease.OutQuad);
                Debug.Log($"[3-1 Demo] {actionType} 처리 완료: 강제 당김 연출 실행");
            }
        }
        // 2단계: 뽑아 올리는 액션 (Tap 또는 Input2)
        else if (string.Equals(actionType, "Tap", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(actionType, "Input2", StringComparison.OrdinalIgnoreCase))
        {
            if (pistolUp != null)
            {
                pistolUp.goingUp = true;
                Debug.Log($"[3-1 Demo] {actionType} 처리 완료: goingUp = true");
            }
        }
        else
        {
            Debug.LogWarning($"[3-1 Demo] 처리되지 않은 actionType입니다: '{actionType}'");
        }
    }
}  
