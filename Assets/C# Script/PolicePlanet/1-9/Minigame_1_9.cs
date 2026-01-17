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
    private SpriteRenderer bgRenderer;
    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "가동시켜라!";

    private Tween blinkTween;

    private void Awake()
    {
        if (brightBackground != null)
            bgRenderer = brightBackground.GetComponent<SpriteRenderer>();
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
        /*
        else
            Fail();
        */
    }

    private void PlayBlinkOnce()
    {
        if (bgRenderer == null) return;

        blinkTween?.Kill();

        // 켜고 알파 0으로 초기화
        brightBackground.SetActive(true);
        bgRenderer.color = new Color(
            bgRenderer.color.r,
            bgRenderer.color.g,
            bgRenderer.color.b,
            0f
        );

        blinkTween = DOTween.Sequence()
            .Append(bgRenderer.DOFade(1f, 0.2f)) // Fade In
            .AppendInterval(0.1f)               // 유지 (원하면 0 가능)
            .Append(bgRenderer.DOFade(0f, 0.2f)) // Fade Out
            .OnComplete(() =>
            {
                brightBackground.SetActive(false);
            });
    }



    private void StopBlink()
    {
        blinkTween?.Kill();
        blinkTween = null;

        if (bgRenderer != null)
        {
            bgRenderer.color = new Color(
                bgRenderer.color.r,
                bgRenderer.color.g,
                bgRenderer.color.b,
                0f
            );
            brightBackground.SetActive(false);
        }
    }
}
