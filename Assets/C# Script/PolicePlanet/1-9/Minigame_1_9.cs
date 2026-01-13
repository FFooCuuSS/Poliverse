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

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            handleMover.PlayStretch();
        }
    }

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
        if (!canInput) return;

        canInput = false;

        handleMover.PlayStretch();

        OnPlayerInput();
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
