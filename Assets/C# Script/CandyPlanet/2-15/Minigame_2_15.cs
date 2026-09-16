using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_15 : MiniGameBase
{
    public static Minigame_2_15 Instance;

    // 판정 범위 오버라이드 (더 이상 타이밍 판정에 쓰이진 않지만, 리듬매니저 바인딩 자체는 유지하는 경우를 대비해 남겨둠)
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "피해라!";

    private bool ended;
    public int missCount = 0;

    // BiteZoneController가 판정 직후 참고할 수 있도록 마지막 결과를 저장 (필요 시 UI 등에서 활용)
    public JudgementResult LastJudgement { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public override void StartGame()
    {
        base.StartGame();
        ended = false;
        missCount = 0;
        // 추가 초기화
    }

    public void Succeed()
    {
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
        if (action == "Input")
        {
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        // BiteZoneController가 이제 OnJudgement를 직접 호출하므로 이 경로는 사용하지 않음.
        if (IsInputLocked) return;
        base.OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended || IsInputLocked) return;
        base.OnJudgement(judgement);

        LastJudgement = judgement;

        if (judgement == JudgementResult.Miss)
        {
            missCount++;
            //CheckGameResult();
        }
    }

    public void CheckGameResult()
    {
        if (IsInputLocked || ended) return;
        ended = true;
        // 모두 Miss 3번 이상 실패
        if (missCount >= 3)
        {
            Debug.Log("실패");
            Failure();
        }
    }
}