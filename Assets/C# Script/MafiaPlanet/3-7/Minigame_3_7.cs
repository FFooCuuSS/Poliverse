using UnityEngine;
using DG.Tweening;

// 3-7 서명 위조하기
// 라운드 (2번):  Show(왼쪽 서명) → Show(서명 종이 내려옴) → Input x5(유지 체크) → End
//  - 종이가 '딱 내려앉는 순간'(두 번째 Show + showDuration)이 누르는 박자. 그때 펜도 출발한다.
//  - 펜이 서명을 다 그릴 때까지 누르고 있으면 된다. 누르고 있는 동안에만 펜 자국이 남는다.
//
// 판정 (Input 노트마다 1점, 총 10개):
//  - 라운드 첫 Input : 내려앉는 박자 ±goodWindow 안에 눌렀는지 (누르는 타이밍)
//  - 펜이 그리는 중 Input : 그 시점에 계속 누르고 있는지
//  - 펜이 다 그린 뒤 Input : 펜이 끝날 때까지 누르고 있었는지 (다 그린 뒤엔 떼도 됨)
// 박자 전부터 누르고 있던 건 무효.
public class Minigame_3_7 : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 16f;
    protected override string MinigameTitle => "서명 위조하기";
    protected override string MinigameExplain => "빈 종이가 내려옵니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "타이밍에 맞춰 화면을 터치하고 있으세요."
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

    [Header("판정 범위 (초) - 종이 내려오는 박자 기준")]
    [SerializeField] private float perfectWindow = 0.1f;
    [SerializeField] private float goodWindow = 0.3f;

    [Tooltip("펜이 끝나기 이 시간 전에 뗀 건 봐줌 (초)")]
    [SerializeField] private float releaseTolerance = 0.15f;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    private bool ended;
    private int showCount;

    private Transform leftSignTransform;

    private Vector3 leftSignTargetPos;
    private Vector2 drawAreaTargetPos;

    private Sequence hideSequence;

    private bool isDemoMode = false;
    public new bool IsDemoMode => isDemoMode;

    // 이번 라운드
    private float refTime = -1f;      // 종이 내려앉는 박자 (두 번째 Show + showDuration)
    private float pressTime = -1f;    // 마지막으로 누르기 시작한 노래 시각
    private float releaseTime = -1f;  // 마지막으로 뗀 노래 시각
    private bool penStarted;
    private float penEndTime = -1f;
    private bool firstCheckDone;
    private int pendingChecks;

    private float SongT
    {
        get
        {
            if (rhythmManager is RhythmManager rm && rm.IsRunning)
                return (float)rm.SongTime;

            return Time.time;
        }
    }

    private bool IsHolding => holdInput != null ? holdInput.IsHolding : Input.GetMouseButton(0);

    // 이번 라운드에 맞게 눌러서 유지 중인지 (펜 자국 표시용)
    public bool InkAllowed
    {
        get
        {
            if (ended) return false;
            if (isDemoMode) return refTime >= 0f;

            return IsHolding && refTime >= 0f && pressTime >= refTime - goodWindow;
        }
    }

    private void JudgeLog(string msg)
    {
        if (judgeLog) Debug.Log(msg);
    }

    public override void SetDemoMode(bool isDemo)
    {
        base.SetDemoMode(isDemo);
        isDemoMode = isDemo;
        Debug.Log($"[3-7] SetDemoMode = {isDemoMode}");
    }

    // 시범도 OnRhythmEvent(Input)에서 자동 성공 처리된다.
    // 여기서 또 처리하면 Input마다 두 번 판정되므로 아무것도 하지 않는다.
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
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
        pressTime = -1f;
        ResetRound();

        if (holdInput != null)
            holdInput.ResetInput();

        if (autoBrush != null)
            autoBrush.ResetBrush();

        MoveObjectsToStartPosition();
    }

    private void ResetRound()
    {
        refTime = -1f;
        penStarted = false;
        penEndTime = -1f;
        firstCheckDone = false;
        pendingChecks = 0;
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

    private void Update()
    {
        if (!ended && !isDemoMode && Input.GetMouseButtonDown(0))
        {
            pressTime = SongT;

            if (refTime >= 0f)
                JudgeLog($"[3-7 Judge] 누름 song={pressTime:F3} (박자 대비 {(pressTime - refTime) * 1000:F0}ms)");
            else
                JudgeLog($"[3-7 Judge] 누름 song={pressTime:F3} (종이 내려오기 전)");
        }

        if (!ended && !isDemoMode && Input.GetMouseButtonUp(0))
        {
            releaseTime = SongT;
            JudgeLog($"[3-7 Judge] 뗌 song={releaseTime:F3}" + (penEndTime > 0f ? $" (펜 끝 {penEndTime:F3})" : ""));
        }

        // 종이가 내려앉는 순간 펜 출발
        if (!ended && refTime >= 0f && !penStarted && SongT >= refTime)
        {
            penStarted = true;

            if (autoBrush != null)
            {
                autoBrush.StartDrawingPath();
                penEndTime = refTime + autoBrush.DrawDuration;
            }

            JudgeLog($"[3-7 Judge] 펜 출발 song={SongT:F3} → 펜 끝 {penEndTime:F3}");
        }

        if (pendingChecks > 0)
            TryFlushPending();
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        action = action.Trim();

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
        // 0, 2번째 Show = 왼쪽 서명 / 1, 3번째 Show = 서명 종이 (누르는 박자)
        if (showCount % 2 == 0)
        {
            FlushPending();
            ResetRound();
            ShowLeftSign();
        }
        else
        {
            refTime = SongT + showDuration;
            JudgeLog($"[3-7 Judge] 종이 내려옴 song={SongT:F3} → 누르는 박자 {refTime:F3}");
            ShowDrawArea();
        }

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
        pendingChecks++;
        TryFlushPending();
    }

    // 종이 박자 + goodWindow가 지나야 '누르는 타이밍'까지 확정되므로 그때 처리
    private void TryFlushPending()
    {
        if (pendingChecks <= 0) return;

        if (!isDemoMode && refTime >= 0f && SongT < refTime + goodWindow)
            return;

        FlushPending();
    }

    private void FlushPending()
    {
        while (pendingChecks > 0)
        {
            pendingChecks--;
            JudgeHoldCheck();
        }
    }

    private void JudgeHoldCheck()
    {
        if (isDemoMode)
        {
            PlayClick();
            ReportManualSuccess();
            JudgeLog("[3-7 Judge] 시범 자동 성공");
            return;
        }

        bool holding = IsHolding;
        bool pressedForThisRound = holding && refTime >= 0f && pressTime >= refTime - goodWindow;

        bool success;
        string reason;

        if (!firstCheckDone)
        {
            firstCheckDone = true;

            float diff = pressTime - refTime;
            success = pressedForThisRound && Mathf.Abs(diff) <= goodWindow;

            string grade = !success ? "Miss" : Mathf.Abs(diff) <= perfectWindow ? "Perfect" : "Good";
            reason = holding
                ? $"누른 타이밍 {diff * 1000:F0}ms → {grade}"
                : "안 누름 → Miss";
        }
        else if (penEndTime > 0f && SongT >= penEndTime)
        {
            // 펜이 다 그린 뒤: 펜 끝까지 버텼으면 성공 (이후엔 떼도 됨)
            bool validPress = refTime >= 0f && pressTime >= refTime - goodWindow;
            bool heldToEnd = holding || (releaseTime > pressTime && releaseTime >= penEndTime - releaseTolerance);

            success = validPress && heldToEnd;
            reason = success ? "펜 끝까지 유지" : (!validPress ? "박자 전부터 누름/안 누름" : $"펜 끝 전에 뗌 ({releaseTime:F3})");
        }
        else
        {
            success = pressedForThisRound;
            reason = success ? "유지 중" : (holding ? "박자 전부터 누름 → 무효" : "안 누르고 있음");
        }

        if (success)
        {
            PlayClick();
            ReportManualSuccess();
        }
        else
        {
            ReportManualFail();
        }

        JudgeLog($"[3-7 Judge] 체크 {(success ? "성공" : "실패")} ({reason}) song={SongT:F3}");
    }

    private void PlayClick()
    {
        if (clickSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            GameRoot.Instance.Audio.PlaySfx(clickSound);
    }

    private void OnEndSignal()
    {
        FlushPending();

        if (autoBrush != null)
            autoBrush.StopDrawing();

        // 라운드가 끝나면 손을 떼고 다음 라운드에 다시 눌러야 함
        if (holdInput != null)
            holdInput.ResetInput();

        refTime = -1f;
        penStarted = true;   // 다음 Show 전까지 펜 재출발 방지

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