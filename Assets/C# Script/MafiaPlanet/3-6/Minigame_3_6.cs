using System.Collections.Generic;
using UnityEngine;

// 3-6 통화 내용 감청
// 초록 → 노랑 → 빨강 순서로 불이 켜지고, 빨간불 박자에 마이크를 눌러서 그대로 누르고 있는다.
//
// 판정 (RecordCheck 노트마다 1점, 총 8개):
//  - 라운드의 첫 RecordCheck : 빨간불 시각 ±goodWindow 안에 눌렀는지 (누르는 타이밍)
//  - 나머지 RecordCheck      : 그 시점에 계속 누르고 있는지 (빨간불 전부터 누르고 있던 건 무효)
// 첫 체크 노트가 빨간불 직후(0.1초)에 있어도, 판정은 빨간불 + goodWindow 까지 기다렸다가 한다.
public class Minigame_3_6 : MiniGameBase, IPracticeDemoInput
{
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;
    protected override float TimerDuration => 10f;
    protected override string MinigameTitle => "통화 내용 감청";

    protected override string MinigameExplain => "초록 노랑 빨강 순서로 불이 켜집니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "앞선 박자에 맞춰 빨간불에 마이크를 터치하세요."
    };

    // RhythmManager의 입력 타이밍 판정은 쓰지 않는다 (RecordCheck마다 직접 판정)
    protected override bool UseRhythmJudgementScore => false;

    [Header("3-6 References")]
    [SerializeField] private LightController_3_6 lightController;

    [Header("판정 범위 (초) - 빨간불 기준")]
    [SerializeField] private float perfectWindow = 0.1f;
    [SerializeField] private float goodWindow = 0.3f;

    [Header("누름 연출 (플레이 + 시범)")]
    [Tooltip("누르는 동안 켜둘 오브젝트 (프리팹 안에 미리 배치, 평소엔 꺼둠)")]
    [SerializeField] private GameObject pressEffect;

    [Header("마이크 판정 영역")]
    [Tooltip("화면에 보이는 마이크 (Recorder)")]
    [SerializeField] private Transform micButton;
    [Tooltip("켜면 마이크 버튼 위를 눌렀을 때만 인정, 끄면 화면 어디든 인정")]
    [SerializeField] private bool micOnlyTouch = false;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.25f;
    public override float hitWindowOverride => 0.35f;

    private bool isRecording;
    private bool ended;
    private bool redOn;          // 빨간불 켜져 있는 동안 true
    private bool effectShown;

    private bool isDemoMode;
    public new bool IsDemoMode => isDemoMode;

    // 이번 라운드
    private float redTime = -1f;        // 빨간불 켜진 노래 시각
    private float pressTime = -1f;      // 현재 누르기 시작한 노래 시각
    private bool firstCheckDone;

    // 빨간불 + goodWindow 전에 온 체크는 잠시 보류
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

    private void JudgeLog(string msg)
    {
        if (judgeLog) Debug.Log(msg);
    }

    public override void SetDemoMode(bool isDemo)
    {
        base.SetDemoMode(isDemo);
        isDemoMode = isDemo;

        // 데모에서는 플레이어가 녹음 중인 상태를 조작하지 못함
        isRecording = false;

        Debug.Log("[3-6] 데모 모드: " + isDemoMode);
    }

    public override void StartGame()
    {
        base.StartGame();

        isRecording = false;
        ended = false;
        redOn = false;
        ResetRound();
        HidePressFx();

        if (lightController != null)
            lightController.TurnOffAll();
    }

    private void ResetRound()
    {
        redTime = -1f;
        firstCheckDone = false;
        pendingChecks = 0;
    }

    // 버튼 PointerDown에서 호출
    public void StartRecording()
    {
        if (isDemoMode || ended) return;
        if (isRecording) return;

        if (clickSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            GameRoot.Instance.Audio.PlaySfx(clickSound);

        isRecording = true;
        pressTime = SongT;

        if (redTime >= 0f)
            JudgeLog($"[3-6 Judge] 누름 song={pressTime:F3} (빨간불 대비 {(pressTime - redTime) * 1000:F0}ms)");
        else
            JudgeLog($"[3-6 Judge] 누름 song={pressTime:F3} (빨간불 전)");
    }

    // 버튼 PointerUp에서 호출
    public void StopRecording()
    {
        if (isDemoMode) return;
        if (!isRecording) return;

        isRecording = false;
        JudgeLog($"[3-6 Judge] 뗌 song={SongT:F3}");
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        action = action.Trim();

        switch (action)
        {
            case "ShowGreen":
                redOn = false;
                // 이전 라운드에 남은 보류 체크 정리
                FlushPending();
                ResetRound();
                lightController?.ShowGreen();
                break;

            case "ShowYellow":
                redOn = false;
                lightController?.ShowYellow();
                break;

            case "ShowRed":
                redOn = true;
                redTime = SongT;
                lightController?.ShowRed();
                JudgeLog($"[3-6 Judge] 빨간불 song={redTime:F3}");
                DemoPress();
                break;

            case "RecordCheck":
                pendingChecks++;
                TryFlushPending();
                break;

            case "Off":
                redOn = false;
                lightController?.TurnOffAll();
                break;

            case "End":
                redOn = false;
                FlushPending();
                HidePressFx();
                ended = true;
                isRecording = false;
                lightController?.TurnOffAll();
                break;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        HidePressFx();   // 로드 직후 깜빡임 방지
    }

    private void Update()
    {
        PollPointer();
        RefreshPressFx();

        if (pendingChecks > 0)
            TryFlushPending();
    }

    // 버튼 이벤트 연결이 빠져 있어도 동작하도록 직접 누름/뗌을 읽는다
    // (StartRecording/StopRecording은 중복 호출돼도 안전)
    private void PollPointer()
    {
        if (isDemoMode || ended) return;

        if (Input.GetMouseButtonDown(0))
        {
            bool onMic = IsPointerOnMic();
            JudgeLog($"[3-6 Judge] 클릭 감지 song={SongT:F3} 마이크위={onMic} (micButton={(micButton != null ? micButton.name : "없음")})");

            if (onMic)
                StartRecording();
        }

        if (isRecording && !Input.GetMouseButton(0))
            StopRecording();
    }

    private bool IsPointerOnMic()
    {
        // 기본: 화면 어디를 눌러도 인정 (모바일에서 버튼만 노리기 어려움)
        if (!micOnlyTouch) return true;

        // 마이크 버튼이 지정 안 돼 있으면 화면 어디든 인정
        if (micButton == null) return true;

        Vector2 screen = Input.mousePosition;

        if (micButton is RectTransform rt)
        {
            Canvas canvas = rt.GetComponentInParent<Canvas>();
            // Screen Space Camera인데 카메라가 비어 있으면 Overlay처럼 동작하므로 null
            Camera uiCam = (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                ? null
                : canvas.worldCamera;

            return RectTransformUtility.RectangleContainsScreenPoint(rt, screen, uiCam);
        }

        Camera cam = Camera.main;
        if (cam == null) return true;

        Vector3 w = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(cam.transform.position.z)));

        var col = micButton.GetComponentInChildren<Collider2D>();
        if (col != null) return col.OverlapPoint(w);

        var sr = micButton.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Bounds b = sr.bounds;
            w.z = b.center.z;
            return b.Contains(w);
        }

        return true;
    }

    // 빨간불 + goodWindow가 지나야 '누르는 타이밍'까지 확정되므로 그때 처리
    private void TryFlushPending()
    {
        if (pendingChecks <= 0) return;

        if (!isDemoMode && redTime >= 0f && SongT < redTime + goodWindow)
            return;

        FlushPending();
    }

    private void FlushPending()
    {
        while (pendingChecks > 0)
        {
            pendingChecks--;
            JudgeRecordingNode();
        }
    }

    private void JudgeRecordingNode()
    {
        if (isDemoMode)
        {
            ReportManualSuccess();
            Debug.Log("[3-6] 데모 자동 성공 판정");
            return;
        }

        bool success;
        string reason;

        // 빨간불 전부터 누르고 있던 건 무효 (계속 누르고 버티기 방지)
        bool pressedForThisRed =
            isRecording &&
            redTime >= 0f &&
            pressTime >= redTime - goodWindow;

        if (!firstCheckDone)
        {
            firstCheckDone = true;

            float diff = pressTime - redTime;
            success = pressedForThisRed && Mathf.Abs(diff) <= goodWindow;

            string grade = !success ? "Miss" : Mathf.Abs(diff) <= perfectWindow ? "Perfect" : "Good";
            reason = isRecording
                ? $"누른 타이밍 {diff * 1000:F0}ms → {grade}"
                : "안 누름 → Miss";
        }
        else
        {
            success = pressedForThisRed;
            reason = success ? "유지 중" : (isRecording ? "빨간불 전부터 누름 → 무효" : "안 누르고 있음");
        }

        if (success) ReportManualSuccess();
        else ReportManualFail();

        JudgeLog($"[3-6 Judge] 체크 {(success ? "성공" : "실패")} ({reason}) song={SongT:F3}");
    }

    // 시범: 빨간불에 마이크를 누르고, 다음 라운드(초록불)나 끝날 때 뗀다
    private void DemoPress()
    {
        if (!isDemoMode) return;

        if (clickSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            GameRoot.Instance.Audio.PlaySfx(clickSound);

    }

    // 누르는 동안 마이크 버튼 위에 프레임 유지
    // 말풍선은 '빨간불 켜져 있음 + 이번 빨간불에 맞게(±goodWindow) 눌러서 유지 중'일 때만
    // 시범은 빨간불 동안 항상
    private void RefreshPressFx()
    {
        bool show;

        if (ended || !redOn) show = false;
        else if (isDemoMode) show = true;
        else show = isRecording && redTime >= 0f && Mathf.Abs(pressTime - redTime) <= goodWindow;

        if (show == effectShown) return;

        effectShown = show;
        if (pressEffect != null) pressEffect.SetActive(show);
    }

    private void HidePressFx()
    {
        effectShown = false;
        if (pressEffect != null) pressEffect.SetActive(false);
    }

    private void OnDisable()
    {
        isRecording = false;
        HidePressFx();
    }
}