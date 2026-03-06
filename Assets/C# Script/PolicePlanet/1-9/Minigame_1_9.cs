using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class Minigame_1_9 : MiniGameBase
{
    [Header("Visual")]
    [SerializeField] private Rope rope;

    [Header("배경 깜빡임")]
    [SerializeField] private GameObject brightBackground;
    [SerializeField] private HandleMover_1_9 handleMover;

    [Header("성공 연출")]
    [SerializeField] private GameObject movingObject;
    [SerializeField] private GameObject activateObject;
    [SerializeField] private GameObject lightEffect;

    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "가동시켜라!";

    private bool ended;
    private bool inputOpen;
    private bool awaitingJudge;
    private bool hasAnySuccess;

    private SpriteRenderer bgRenderer;
    private Tween blinkTween;

    private void Awake()
    {
        if (brightBackground != null)
            bgRenderer = brightBackground.GetComponent<SpriteRenderer>();
    }

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        inputOpen = false;
        awaitingJudge = false;
        hasAnySuccess = false;

        StopBlink();
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        action = action.Trim();
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        switch (action)
        {
            case "Show":
                inputOpen = false;
                awaitingJudge = false;
                PlayBlinkOnce();
                break;

            case "Input":
                inputOpen = true;
                awaitingJudge = false;
                break;

            case "InputEnd":
                if (inputOpen && !awaitingJudge)
                {
                    // 입력 안 했으므로 강제 Miss
                    Debug.Log("입력 없음 → 강제 Miss");
                    OnJudgement(JudgementResult.Miss);
                }

                inputOpen = false;
                break;
        }
    }

    /// <summary>
    /// 화면 터치 시 Manager나 UI에서 호출
    /// </summary>
    public void SubmitPlayerInput(string action = "Input")
    {
        // 연출은 항상 실행
        handleMover?.PlayStretch();
        rope?.PlayStretch(new Vector3(2f, 0, 0));

        if (ended) return;
        if (!inputOpen) return;
        if (awaitingJudge) return;

        awaitingJudge = true;
        OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended) return;

        awaitingJudge = false;
        inputOpen = false;

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                Debug.Log("판정 성공");
                hasAnySuccess = true;
                break;

            case JudgementResult.Miss:
                Debug.Log("판정 미스");
                break;
        }
    }

    /// <summary>
    /// 타이머 종료 or 차트 종료 시 외부에서 호출
    /// </summary>
    public void FinalJudge()
    {
        if (ended) return;
        ended = true;

        if (hasAnySuccess)
        {
            Success();
        }
        else
        {
            Fail();
        }
    }

    #region 연출

    private void PlayBlinkOnce()
    {
        if (bgRenderer == null) return;

        blinkTween?.Kill();

        brightBackground.SetActive(true);
        bgRenderer.color = new Color(bgRenderer.color.r, bgRenderer.color.g, bgRenderer.color.b, 0f);

        blinkTween = DOTween.Sequence()
            .Append(bgRenderer.DOFade(1f, 0.2f))
            .AppendInterval(0.1f)
            .Append(bgRenderer.DOFade(0f, 0.2f))
            .OnComplete(() =>
            {
                brightBackground.SetActive(false);
            });
    }

    private void StopBlink()
    {
        blinkTween?.Kill();
        blinkTween = null;
    }

    private void StartShaking()
    {
        if (movingObject == null) return;

        movingObject.transform
            .DOShakePosition(
                duration: 1f,
                strength: 0.1f,
                vibrato: 20,
                randomness: 90,
                snapping: false,
                fadeOut: false
            )
            .SetLoops(-1);
    }

    private void ActivateObject()
    {
        if (activateObject != null)
        {
            activateObject.SetActive(true);
            activateObject.GetComponent<SpriteRenderer>().color = Color.white;
        }

        if (lightEffect != null)
            lightEffect.SetActive(true);
    }

    #endregion
}
