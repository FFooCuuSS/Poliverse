using UnityEngine;
using DG.Tweening;

public class Minigame_3_7 : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 16f;
    protected override string MinigameTitle => "젤리 블록 철거!";
    protected override string MinigameExplain => "죄수가 순서대로 나타나 타이밍을 알려줍니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "배경이 어두워지면 타이밍에 맞게 죄수를 터치해주세요."
    };

    protected override bool UseRhythmJudgementScore => false;

    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;

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

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[3-7] SetDemoMode = {isDemoMode}");
    }

    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim();
        Debug.Log($"[3-7 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}");

        if (ended)
            return;

        // CSV 신호에 따른 데모 동작 분기
        if (string.Equals(actionType, "Show", System.StringComparison.OrdinalIgnoreCase))
        {
            OnShowSignal();
        }
        else if (string.Equals(actionType, "Input", System.StringComparison.OrdinalIgnoreCase))
        {
            // 데모 시연 시 홀드가 안 되어 있어도 자동 성공 처리를 위해 holdInput 강제 모사 혹은 Input 시그널 연동
            if (holdInput != null)
            {
                // 데모 모드에서는 홀드 상태를 강제로 true로 만들어 정상적으로 그려지게 함
                // (필요시 리플렉션이나 홀드 속성 확장 가능, 여기서는 편의상 로직 태우기)
            }
            OnInputSignal();
        }
        else if (string.Equals(actionType, "End", System.StringComparison.OrdinalIgnoreCase))
        {
            OnEndSignal();
        }
    }

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
        bool isHolding = isDemoMode || (holdInput != null && holdInput.IsHolding);

        if (isHolding)
        {
            if (clickSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            {
                GameRoot.Instance.Audio.PlaySfx(clickSound);
            }

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