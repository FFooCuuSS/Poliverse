using System.Reflection;
using UnityEngine;

public class Minigame3_8remake : MiniGameBase
{
    // ===== 미니게임 기본 정보 =====
    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "미니게임 3-8 설명을 여기에 넣기";
    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;

    [Header("Input Settings")]
    public KeyCode inputKey = KeyCode.Mouse0;   // 좌클릭 입력

    private bool finished = false;

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (finished) return;

        // 좌클릭 입력
        if (Input.GetKeyDown(inputKey))
        {
            SubmitInput();
        }
    }

    public override void OnRhythmEvent(string action)
    {
        if (finished) return;

        switch (action)
        {
            

            case "Input":
                Debug.Log("[3-8] Input 이벤트 도착");
                break;
        }
    }

    private void OnShowEvent()
    {
        Debug.Log("[3-8] Show 이벤트 실행");

        // TODO: bush 생성 같은 로직
    }

    public void SubmitInput()
    {
        // 리듬매니저 입력 잠금이면 무시
        if (IsInputLocked) return;

        // 입력을 리듬매니저로 전달
        OnPlayerInput("Input");
    }

    // ===== 리듬매니저 판정 결과 로그 =====


    public void OnJudgePerfect()
    {
        Debug.Log("[3-8] PERFECT");
    }

    public void OnJudgeGood()
    {
        Debug.Log("[3-8] GOOD");
    }

    public void OnJudgeMiss()
    {
        Debug.Log("[3-8] MISS");
    }
}