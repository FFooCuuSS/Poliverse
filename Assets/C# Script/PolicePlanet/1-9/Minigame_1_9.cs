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

    private bool canInput = false;
    private bool hasMissed = false;
    private SpriteRenderer bgRenderer;
    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "가동시켜라!";

    private Tween blinkTween;

    [Header("총 입력 수 (인스펙터에서 수정 가능)")]
    [SerializeField] private int maxInputCount = 3;

    private bool hasAnySuccess = false;
    private int totalInputCount = 0;
    private int handledInputCount = 0;
    private List<bool> inputResults = new List<bool>();

    private void Awake()
    {
        if (brightBackground != null)
            bgRenderer = brightBackground.GetComponent<SpriteRenderer>();
    }

    public override void StartGame()
    {
        canInput = false;
        hasAnySuccess = false;

        StopBlink();

        totalInputCount = 3;  // 여기서 총 입력 수를 미리 정함
        handledInputCount = 0;
        inputResults.Clear();
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
                if (totalInputCount >= maxInputCount)
                    return; // 최대 입력 수 초과 방지

                canInput = true;
                totalInputCount++;               // 등장한 입력 수 증가
                inputResults.Add(false);         // 아직 성공/실패 기록 없음
                Debug.Log($"Input 등장 ({totalInputCount}/{maxInputCount})");

                Invoke(nameof(AutoMissInput), 0.5f);
                break;
        }
    }

    public void OnScreenTouch()
    {
        // 항상 핸들 모션 실행
        handleMover.PlayStretch();

        if (rope != null)
            rope.PlayStretch(new Vector3(2f, 0, 0));

        if (!canInput) return;

        canInput = false;
        handledInputCount++;

        inputResults[handledInputCount - 1] = true;
        hasAnySuccess = true;

        Debug.Log($"인풋 처리됨 ({handledInputCount}/{maxInputCount}) → 성공 기록");

        CheckIfLastInputHandled();
    }

    // 판정 처리
    public override void OnJudgement(JudgementResult judgement)
    {
        //StopBlink();
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

        /*
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
        */
    }

    public void NotifySuccess()
    {
        hasAnySuccess = true;
    }

    private void AutoMissInput()
    {
        if (!canInput) return;

        canInput = false;
        handledInputCount++;

        Debug.Log($"입력 없음 → 자동 미스 ({handledInputCount}/{maxInputCount})");

        CheckIfLastInputHandled();
    }

    private void CheckIfLastInputHandled()
    {
        if (handledInputCount < maxInputCount)
            return; // 아직 처리 안 끝남

        Debug.Log("모든 Input 처리 완료 → 최종 판정");

        if (hasAnySuccess)
        {
            Debug.Log("최종 성공!");
            StartShaking();
            ActivateObject();
            Success();
        }
        else
        {
            Debug.Log("최종 실패");
            Fail();
        }
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

            SpriteRenderer sr = activateObject.GetComponent<SpriteRenderer>();
            sr.color = Color.white;
        }

        if (lightEffect != null)
            lightEffect.SetActive(true);
    }
}
