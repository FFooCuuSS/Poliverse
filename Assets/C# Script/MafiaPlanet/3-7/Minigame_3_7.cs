using UnityEngine;
using DG.Tweening;

public class Minigame_3_7 : MiniGameBase
{
    protected override float TimerDuration => 16f;
    protected override string MinigameExplain => "사인을 베끼세요!";
    protected override bool UseRhythmJudgementScore => false;

    [Header("3-7 References")]
    [SerializeField] private SignatureHoldInput_3_7 holdInput;
    [SerializeField] private SignatureAutoBrush autoBrush;

    [Header("Show / Hide Objects")]
    [SerializeField] private GameObject leftSignObject;   // 월드 오브젝트
    [SerializeField] private RectTransform drawArea;      // 캔버스 UI

    [Header("Tween Settings")]
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float hideDuration = 0.2f;

    [SerializeField] private float leftSignTopY = 6f;
    [SerializeField] private float leftSignBottomY = -6f;

    [SerializeField] private float drawAreaTopY = 900f;
    [SerializeField] private float drawAreaBottomY = -900f;

    private bool ended;
    private int showCount;

    private Transform leftSignTransform;

    private Vector3 leftSignTargetPos;
    private Vector2 drawAreaTargetPos;

    private Sequence hideSequence;

    protected override void Awake()
    {
        base.Awake();

        if (leftSignObject != null)
        {
            leftSignTransform = leftSignObject.transform;
            leftSignTargetPos = leftSignTransform.position;
        }

        if (drawArea != null)
            drawAreaTargetPos = drawArea.anchoredPosition;
    }

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        showCount = 0;

        if (holdInput != null)
            holdInput.ResetInput();

        if (autoBrush != null)
            autoBrush.ResetBrush();

        MoveObjectsToStartPosition();
    }

    private void MoveObjectsToStartPosition()
    {
        if (leftSignTransform != null)
        {
            DOTween.Kill(leftSignTransform);
            leftSignObject.SetActive(true);

            Vector3 pos = leftSignTargetPos;
            pos.y = leftSignTopY;
            leftSignTransform.position = pos;
        }

        if (drawArea != null)
        {
            DOTween.Kill(drawArea);

            Vector2 pos = drawAreaTargetPos;
            pos.y = drawAreaTopY;
            drawArea.anchoredPosition = pos;
        }
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        action = action.Trim();

        Debug.Log($"[3-7] Rhythm Event: {action}");

        switch (action)
        {
            case "Show":
                OnShowSignal();
                break;

            case "Input":
                OnInputSignal();
                break;

            case "End":
                OnEndSignal();
                break;
        }
    }

    private void OnShowSignal()
    {
        // CSV에서 Show가 번갈아 나오므로
        // 0번째 Show = LeftSign
        // 1번째 Show = DrawArea
        // 2번째 Show = LeftSign
        // 3번째 Show = DrawArea
        if (showCount % 2 == 0)
            ShowLeftSign();
        else
            ShowDrawArea();

        showCount++;
    }

    private void ShowLeftSign()
    {
        if (leftSignTransform == null) return;

        DOTween.Kill(leftSignTransform);
        leftSignObject.SetActive(true);

        Vector3 start = leftSignTargetPos;
        start.y = leftSignTopY;
        leftSignTransform.position = start;

        leftSignTransform
            .DOMove(leftSignTargetPos, showDuration)
            .SetEase(Ease.OutCubic);
    }

    private void ShowDrawArea()
    {
        if (drawArea == null) return;

        DOTween.Kill(drawArea);

        // 플레이어 쪽 사인 시작 전 초기화
        if (autoBrush != null)
            autoBrush.ResetBrush();

        Vector2 start = drawAreaTargetPos;
        start.y = drawAreaTopY;
        drawArea.anchoredPosition = start;

        drawArea
            .DOAnchorPos(drawAreaTargetPos, showDuration)
            .SetEase(Ease.OutCubic);
    }

    private void OnInputSignal()
    {
        if (autoBrush != null)
            autoBrush.StartDrawingPath();

        JudgeHoldInput();
    }

    private void JudgeHoldInput()
    {
        bool isHolding = holdInput != null && holdInput.IsHolding;

        if (isHolding)
        {
            ReportManualSuccess();
            Debug.Log("[3-7] Manual Perfect - Holding");
        }
        else
        {
            ReportManualFail();
            Debug.Log("[3-7] Manual Miss - Not Holding");
        }
    }

    private void OnEndSignal()
    {
        if (autoBrush != null)
            autoBrush.StopDrawing();

        if (holdInput != null)
            holdInput.ResetInput();

        HideObjectsThenClearBrush();
    }

    private void HideObjectsThenClearBrush()
    {
        hideSequence?.Kill();
        hideSequence = DOTween.Sequence();

        if (leftSignTransform != null)
        {
            DOTween.Kill(leftSignTransform);

            Vector3 target = leftSignTargetPos;
            target.y = leftSignBottomY;

            hideSequence.Join(
                leftSignTransform
                    .DOMove(target, hideDuration)
                    .SetEase(Ease.InCubic)
            );
        }

        if (drawArea != null)
        {
            DOTween.Kill(drawArea);

            Vector2 target = drawAreaTargetPos;
            target.y = drawAreaBottomY;

            hideSequence.Join(
                drawArea
                    .DOAnchorPos(target, hideDuration)
                    .SetEase(Ease.InCubic)
            );
        }

        hideSequence.OnComplete(() =>
        {
            if (autoBrush != null)
                autoBrush.ResetBrush();
        });
    }

    private void OnDisable()
    {
        ended = true;

        hideSequence?.Kill();

        if (leftSignTransform != null)
            DOTween.Kill(leftSignTransform);

        if (drawArea != null)
            DOTween.Kill(drawArea);

        if (holdInput != null)
            holdInput.ResetInput();

        if (autoBrush != null)
        {
            autoBrush.StopDrawing();
            autoBrush.ResetBrush();
        }
    }
}