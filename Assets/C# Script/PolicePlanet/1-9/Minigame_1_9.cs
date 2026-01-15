using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Minigame_1_9 : MiniGameBase
{
    [Header("Visual")]
    [SerializeField] private Rope rope;

    [Header("배경 깜빡임")]
    [SerializeField] private GameObject brightBackground;

    [SerializeField] private HandleMover_1_9 handleMover;

    private bool canInput = false;
    private bool hasMissed = false;

    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "가동시켜라!";

    private Tween blinkTween;

    public override void StartGame()
    {
        canInput = false;
        StopBlink();

        hasMissed = false;
    }

    public void Succeed()
    {
        Success();
    }
    public void Failure()
    {
        Fail();
    }

    public override void OnRhythmEvent(string action)
    {
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        // 이건 나중에 개별 미니게임에서 override하는 형태로
        switch (action)
        {
            case "Show":
                PlayBlinkOnce();
                break;

            case "Input":
                canInput = true;
                break;
        }
    }

    public void OnScreenTouch()
    {
        // 항상 핸들 모션 실행
        handleMover.PlayStretch();

        // 줄 늘어났다 줄어드는 효과
        if (rope != null)
            rope.PlayStretch(new Vector3(2f, 0, 0), 0.3f);

        if (canInput)
        {
            canInput = false;

            // 디버그 메시지 출력
            Debug.Log("성공!");

            // 실제 성공 처리 호출
            Success(); // 또는 OnPlayerInput() 호출 계속해도 됨
        }
    }



    // 판정 처리
    public override void OnJudgement(JudgementResult judgement)
    {
        StopBlink();

        if (judgement == JudgementResult.Perfect || judgement == JudgementResult.Good)
            Success();
        else
            Fail();
    }

    private void PlayBlinkOnce()
    {
        if (brightBackground == null) return;

        // 각 Show마다 독립적인 Sequence 생성
        Sequence blinkSequence = DOTween.Sequence();
        blinkSequence.AppendCallback(() => brightBackground.SetActive(true));
        blinkSequence.AppendInterval(0.5f); // 켜진 시간
        blinkSequence.AppendCallback(() => brightBackground.SetActive(false));

        // 자동 제거
        blinkSequence.SetAutoKill(true);
    }


    private void StopBlink()
    {
        blinkTween?.Kill();

        if (brightBackground != null)
            brightBackground.SetActive(false);
    }
}
