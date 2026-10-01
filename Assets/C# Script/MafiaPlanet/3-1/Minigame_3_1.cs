using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_1 : MiniGameBase
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

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[3-1] SetDemoMode = {isDemoMode}");
    }

    public override void StartGame()
    {
        // 추가 초기화
    }

    // --- IPracticeDemoInput 구현 ---
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim().ToLower();
        Debug.Log($"[3-1 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}");

        if (IsInputLocked || IsSuccess)
            return;

        // CSV의 input1, input2 단계에 따른 데모 시연 분기
        if (actionType.Contains("input1"))
        {
            // 첫 번째 입력: 총을 당길 수 있는 상태로 허용
            if (pistolDrag != null)
            {
                pistolDrag.canPull = true;
            }
        }
        else if (actionType.Contains("input2"))
        {
            // 두 번째 입력: 총이 위로 솟아오르는 로직(PistolUp) 트리거
            if (pistolUp != null)
            {
                pistolUp.goingUp = true;
            }
        }
    }
}  
