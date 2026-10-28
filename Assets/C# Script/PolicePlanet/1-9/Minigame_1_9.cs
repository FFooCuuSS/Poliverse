using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class Minigame_1_9 : MiniGameBase, IPracticeDemoInput
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

    // 노트 간격이 1초라 hitWindow 1초면 힌트(Show) 구간 터치가 다음 Input 노트를 Miss로 먹어버린다.
    // 판정 범위를 노트 간격 절반 이하로 줄인다.
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.45f;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    private double SongT => (rhythmManager as RhythmManager)?.SongTime ?? -1;
    private int lastSubmitFrame = -1;

    protected override string MinigameTitle => "발전기 작동시키기";

    protected override string MinigameExplain => " 불이 2번씩 켜지며 타이밍을 알려줍니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "타이밍에 맞게 화면을 터치해주세요"
    };

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[1-9] SetDemoMode = {isDemoMode}");
    }

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
        if (IsDemoMode) return;
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

    // 플레이어 터치 (Update / BackgroundTouchHandler / DragDistanceJudge에서 호출)
    public void SubmitPlayerInput(string action = "Input")
    {
        // 시범 중에는 실제 터치를 막는다 (BackgroundTouchHandler는 데모 체크가 없었음)
        if (IsDemoMode) return;

        SubmitInput(action);
    }

    // 플레이어/시범 공통
    private void SubmitInput(string action)
    {
        if (ended) return;
        if (awaitingJudge) return;

        // 같은 프레임에 Update와 BackgroundTouchHandler가 둘 다 호출하는 중복 방지
        if (lastSubmitFrame == Time.frameCount) return;
        lastSubmitFrame = Time.frameCount;

        // 기존엔 판정 한 번 후 다음 리듬 이벤트까지 입력을 닫아서(inputOpen)
        // 박자보다 살짝 먼저 누르면 무시됐다. 이제 판정 범위는 RhythmManager가 결정한다.
        handleMover?.PlayStretch();

        if (judgeLog)
            Debug.Log($"[1-9 Judge] 터치 song={SongT:F3}");

        awaitingJudge = true;
        OnPlayerInput(action);

        // 범위 안에 노트가 없으면 판정이 안 온다 → 잠금 해제
        if (awaitingJudge)
        {
            awaitingJudge = false;

            if (judgeLog)
                Debug.Log($"[1-9 Judge] 노트 범위 밖 터치 → 무시 song={SongT:F3}");
        }
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended) return;

        base.OnJudgement(judgement);
        awaitingJudge = false;
        inputOpen = false;

        judgedCount++;

        if (judgeLog)
            Debug.Log($"[1-9 Judge] 판정 {judgement} song={SongT:F3}");

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

    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim();

        if (actionType != "Input")
            return;

        if (ended)
            return;

        Debug.Log($"[1-9 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}");

        // 시범: 플레이어가 누른 것과 동일하게 판정까지 보낸다
        SubmitInput("Input");
    }
}