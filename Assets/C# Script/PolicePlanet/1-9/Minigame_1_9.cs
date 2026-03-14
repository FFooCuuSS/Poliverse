using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class Minigame_1_9 : MiniGameBase
{
    [Header("Input Count")]
    [SerializeField] private int totalInputs = 6;

    [Header("Success Condition")]
    [SerializeField] private int endingSuccessThreshold = 4;

    [Header("Visual")]
    [SerializeField] private Rope rope;

    [Header("배경 깜빡임")]
    [SerializeField] private GameObject brightBackground;
    [SerializeField] private HandleMover_1_9 handleMover;

    [Header("엔딩 연출")]
    [SerializeField] private GameObject movingObject;
    [SerializeField] private GameObject activateObject;
    [SerializeField] private GameObject lightEffect;

    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "가동시켜라!";

    private bool ended;
    private bool inputOpen;
    private bool awaitingJudge;

    private int judgedCount;
    private int successCount;

    private SpriteRenderer bgRenderer;
    private Tween blinkTween;
    private Tween shakeTween;

    private void Awake()
    {
        if (brightBackground != null)
            bgRenderer = brightBackground.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        //StartGame();
    }

    private void Update()
    {
        if (ended) return;

        if (Input.GetMouseButtonDown(0))
        {
            SubmitPlayerInput("Input");
        }
    }

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        inputOpen = false;
        awaitingJudge = false;

        judgedCount = 0;
        successCount = 0;

        StopBlink();
        StopShaking();

        if (activateObject != null)
        {
            activateObject.SetActive(false);
            RestoreAlpha(activateObject);
        }

        if (lightEffect != null)
            lightEffect.SetActive(false);

        handleMover?.ResetHandle();
        rope?.ResetRope();
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrWhiteSpace(action)) return;

        action = action.Trim();
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        switch (action)
        {
            case "Show":
                inputOpen = true;
                awaitingJudge = false;
                PlayBlinkOnce();
                break;

            case "Input":
                inputOpen = true;
                awaitingJudge = false;
                break;

            case "End":
                inputOpen = false;
                awaitingJudge = false;
                ended = true;

                Debug.Log($"최종 성공 수: {successCount} / {totalInputs}");

                if (successCount >= endingSuccessThreshold)
                {
                    PlayEndingEffect();
                }
                break;
        }
    }

    public void SubmitPlayerInput(string action = "Input")
    {
        if (ended) return;
        if (!inputOpen) return;
        if (awaitingJudge) return;

        handleMover?.PlayStretch();

        awaitingJudge = true;
        OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended) return;

        awaitingJudge = false;
        inputOpen = false;

        judgedCount++;

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                successCount++;
                Debug.Log($"판정 성공 / {judgedCount} / {totalInputs} / {judgement}");
                break;

            case JudgementResult.Miss:
                Debug.Log($"판정 미스 / {judgedCount} / {totalInputs} / {judgement}");
                break;
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
            .Append(bgRenderer.DOFade(0.7f, 0.1f))
            .AppendInterval(0.1f)
            .Append(bgRenderer.DOFade(0f, 0.1f))
            .OnComplete(() =>
            {
                brightBackground.SetActive(false);
            });
    }

    private void StopBlink()
    {
        blinkTween?.Kill();
        blinkTween = null;

        if (brightBackground != null)
            brightBackground.SetActive(false);
    }

    private void PlayEndingEffect()
    {
        ActivateObject();
        StartShaking();
    }

    private void StartShaking()
    {
        if (movingObject == null) return;

        shakeTween?.Kill();

        shakeTween = movingObject.transform
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

    private void StopShaking()
    {
        shakeTween?.Kill();
        shakeTween = null;
    }

    private void ActivateObject()
    {
        if (activateObject != null)
        {
            activateObject.SetActive(true);
            RestoreAlpha(activateObject);
        }

        if (lightEffect != null)
            lightEffect.SetActive(true);
    }

    private void RestoreAlpha(GameObject target)
    {
        if (target == null) return;

        SpriteRenderer[] spriteRenderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        CanvasGroup[] canvasGroups = target.GetComponentsInChildren<CanvasGroup>(true);
        foreach (var cg in canvasGroups)
        {
            cg.alpha = 1f;
        }
    }

    #endregion
}