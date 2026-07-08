using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MiniGameBase;

public class Minigame_3_11_remake : MiniGameBase
{
    // ===== 미니게임 기본 정보 =====
    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "미니게임 3-2 설명을 여기에 넣기";
    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;


    private bool finished = false;

    private void Start()
    {
        IsInputLocked = false;
        Debug.Log("[3-2] Minigame3_2remake 시작");
        base.StartGame();
    }


    private void Update()
    {
        if (finished) return;

        // 좌클릭 입력
        if (Input.GetMouseButtonDown(0))
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
                Debug.Log("[3-2] Input 이벤트 도착");
                break;
        }
    }

    public void SubmitInput()
    {
        // 리듬매니저 입력 잠금이면 무시
        //if (IsInputLocked) return;
        Debug.Log("[3-2] SubmitInput 호출 - 입력 전달 준비");

        // 입력을 리듬매니저로 전달
        OnPlayerInput("Input");
    }

    // ===== 리듬매니저 판정 결과 로그 =====


    public override void OnJudgement(JudgementResult judgement)
    {
        Debug.Log($"{judgement}");
    }
}
