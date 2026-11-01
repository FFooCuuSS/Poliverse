using DG.Tweening;
using UnityEngine;

// 3-1 결투 신청 (Call & Response)
// 첫째 박: 상대가 단추를 떼거나(CallButton) 총을 뽑는다(CallDraw)
// 둘째 박: 플레이어가 같은 행동
//   - 단추 떼기 = 오른쪽으로 슬라이드 (CallButton 이후에만 받음)
//   - 총 뽑기   = 위로 슬라이드     (CallDraw 이후에만 받음 → 상대보다 먼저 못 뽑음)
//
// CSV 3-1:
//   3-1_time: 1.8, 2.4, 3.6, 4.2
//   3-1_type: CallButton, Swipe, CallDraw, Swipe
public class Minigame_3_1 : MiniGameBase, IPracticeDemoActionInput
{
    protected override float TimerDuration => 20f;

    protected override string MinigameTitle => "결투 신청";
    protected override string MinigameExplain => "첫째 박에 상대가 단추를 떼거나 총을 뽑으며 박자를 알려줍니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "둘째 박에 오른쪽으로 슬라이드해 단추를 떼고, 위로 슬라이드해 총을 뽑으세요."
    };

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;

    [Header("Refs - Player")]
    [SerializeField] private PistolDrag pistolDrag;
    [SerializeField] private PistolUp pistolUp;
    [SerializeField] private Button_3_1 button3_1;

    [Header("Refs - Enemy")]
    [SerializeField] private EnemyMove_3_1 enemy;

    [Header("Player 연출")]
    [Tooltip("단추 끈 끝이 이동할 오프셋 (상대 쪽과 좌우 대칭)")]
    [SerializeField] private Vector2 unbuttonOffset = new Vector2(3.18f, 0.95f);
    [SerializeField] private float unbuttonDuration = 0.2f;

    [Header("Input")]
    [Tooltip("이 화면 비율(x)보다 오른쪽에서 시작한 슬라이드만 인정 (0.5 = 오른쪽 절반)")]
    [SerializeField, Range(0f, 1f)] private float inputAreaMinX = 0.5f;
    [Tooltip("슬라이드로 인정할 거리 (화면 높이 비율)")]
    [SerializeField, Range(0.01f, 0.5f)] private float swipeThreshold = 0.06f;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    public static Minigame_3_1 Instance { get; private set; }

    private bool isDemoMode = false;
    public new bool IsDemoMode => isDemoMode;

    private bool playerUnbuttoned;
    private bool playerDrawn;

    // 0 = 대기, 1 = 단추 콜 이후(단추만 가능), 2 = 총 콜 이후(총만 가능)
    private int phase;

    private bool pressing;
    private bool swipeSent;
    private Vector2 pressPos;

    private double SongT => (rhythmManager as RhythmManager)?.SongTime ?? -1;

    private void JudgeLog(string msg)
    {
        if (judgeLog) Debug.Log(msg);
    }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public override void SetDemoMode(bool isDemo)
    {
        base.SetDemoMode(isDemo);
        isDemoMode = isDemo;

        pressing = false;

        if (isDemo)
        {
            FindReferencesSafely();
            if (button3_1 != null) button3_1.ResetDemo();
        }
    }

    public override void StartGame()
    {
        base.StartGame();
        FindReferencesSafely();

        playerUnbuttoned = false;
        playerDrawn = false;
        pressing = false;
        phase = 0;

        // 기존 드래그 조작(단추 끈 드래그, 총 드래그)은 전부 끈다 → 슬라이드 판정으로 대체
        foreach (var drag in GetComponentsInChildren<DragAndDrop>(true))
        {
            drag.banDragging = true;
            drag.isDragging = false;
        }
    }

    private void FindReferencesSafely()
    {
        if (pistolDrag == null)
            pistolDrag = GetComponentInChildren<PistolDrag>(true);

        if (pistolUp == null && pistolDrag != null)
            pistolUp = pistolDrag.GetComponent<PistolUp>();

        if (button3_1 == null)
            button3_1 = GetComponentInChildren<Button_3_1>(true);

        if (enemy == null)
            enemy = GetComponentInChildren<EnemyMove_3_1>(true);
    }

    // ───────── 리듬 이벤트 (Call) ─────────
    public override void OnRhythmEvent(string action)
    {
        if (string.IsNullOrWhiteSpace(action)) return;

        action = action.Trim();
        base.OnRhythmEvent(action);

        switch (action)
        {
            case "CallButton":
                phase = 1;
                JudgeLog($"[3-1 Judge] CALL 단추 song={SongT:F3}");
                if (enemy != null) enemy.PlayUnbutton();
                break;

            case "CallDraw":
                phase = 2;
                JudgeLog($"[3-1 Judge] CALL 총 song={SongT:F3}");
                if (enemy != null) enemy.PlayDraw();

                // 단추를 놓쳤어도 총 뽑기는 할 수 있게 연출만 풀어준다 (점수는 그대로 Miss)
                if (!playerUnbuttoned)
                    DoUnbuttonVisual();
                break;
        }
    }

    // ───────── 플레이어 입력 (Response) ─────────
    private void Update()
    {
        if (isDemoMode) return;
        if (IsInputLocked) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 p = Input.mousePosition;

            if (p.x >= Screen.width * inputAreaMinX)
            {
                pressing = true;
                swipeSent = false;
                pressPos = p;
            }
        }

        if (pressing && Input.GetMouseButton(0) && !swipeSent)
        {
            Vector2 d = ((Vector2)Input.mousePosition - pressPos) / Mathf.Max(1, Screen.height);

            if (d.magnitude >= swipeThreshold)
            {
                // 한 번 누를 때 슬라이드 하나만
                swipeSent = true;

                bool horizontal = Mathf.Abs(d.x) >= Mathf.Abs(d.y);

                if (horizontal && d.x > 0f)
                    TryUnbutton();
                else if (!horizontal && d.y > 0f)
                    TryDraw();
                else
                    JudgeLog($"[3-1 Judge] 방향 무시 d={d} song={SongT:F3}");
            }
        }

        if (Input.GetMouseButtonUp(0))
            pressing = false;
    }

    // 오른쪽 슬라이드: 상대가 단추를 뗀 뒤(phase 1)에만
    private void TryUnbutton()
    {
        if (playerUnbuttoned) return;

        if (phase != 1)
        {
            JudgeLog($"[3-1 Judge] 단추 슬라이드 무시 (지금 단계 아님) song={SongT:F3}");
            return;
        }

        DoUnbuttonVisual();
        SendInput("단추");
    }

    // 위 슬라이드: 상대가 총을 뽑은 뒤(phase 2)에만
    private void TryDraw()
    {
        if (playerDrawn) return;

        if (phase != 2)
        {
            JudgeLog($"[3-1 Judge] 총 슬라이드 무시 (상대가 아직 안 뽑음) song={SongT:F3}");
            return;
        }

        DoDrawVisual();
        SendInput("총");
    }

    private void SendInput(string label)
    {
        JudgeLog($"[3-1 Judge] 입력 {label} song={SongT:F3}");

        // 두 행동 모두 차트상 Swipe 노트. 단계(phase)로 이미 구분했으므로 그대로 보낸다.
        OnPlayerInput("Swipe");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);
        JudgeLog($"[3-1 Judge] 판정 {judgement} song={SongT:F3}");
    }

    private void DoUnbuttonVisual()
    {
        if (playerUnbuttoned) return;
        playerUnbuttoned = true;

        if (button3_1 != null)
        {
            button3_1.PlayDemoPullAnimation(
                unbuttonDuration,
                unbuttonOffset.magnitude,
                unbuttonOffset.normalized,
                null
            );
        }
    }

    private void DoDrawVisual()
    {
        if (playerDrawn) return;
        playerDrawn = true;

        if (!playerUnbuttoned)
            DoUnbuttonVisual();

        if (pistolUp != null)
        {
            pistolUp.transform.DOKill();
            pistolUp.ForceRise();
        }
    }

    // ───────── 시범 ─────────
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (!isDemoMode) return;
        if (string.IsNullOrWhiteSpace(actionType)) return;

        actionType = actionType.Trim();

        Debug.Log($"[3-1 Demo] Action #{actionIndex} {actionType} phase={phase}");

        if (actionType != "Swipe") return;

        // 시범도 플레이어와 같은 규칙: 지금 단계에 맞는 행동만
        if (phase == 1 && !playerUnbuttoned)
            DoUnbuttonVisual();
        else if (phase == 2 && !playerDrawn)
            DoDrawVisual();
        else
            return;

        // 시범도 실제 판정까지 보낸다
        OnPlayerInput("Swipe");
    }

    // 기존 외부 호출 호환용
    public void SubmitManualInput(string actionName)
    {
        if (IsInputLocked) return;
        OnPlayerInput(actionName);
    }
}