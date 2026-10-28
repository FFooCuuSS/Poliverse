using System.Collections;
using UnityEngine;

public class Minigame_1_7 : MiniGameBase, IPracticeDemoInput
{
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;

    [Header("Refs")]
    [SerializeField] private PrisonerSpawner1_7 prisonerSpawner;
    [SerializeField] private HoldCheck1_7 holdJudge;
    [SerializeField] private Transform basket;

    [Header("Round Settings")]
    [SerializeField] private int totalRound = 3;
    [SerializeField] private int inputPerRound = 4;

    [Header("Flow Timing")]
    [SerializeField] private float throwToExitDelay = 0.15f;
    [SerializeField] private float prisonerExitDuration = 0.45f;

    private PrisonerController1_7 prisoner;

    private int currentRound = 0;
    private int inputIndex = 0;
    private int totalSuccess = 0;

    private bool awaitingJudge = false;
    private bool roundActive = false;
    private bool gameEnded = false;
    private bool transitionRunning = false;
    private bool pendingShow = false;

    // 이번 라운드에서 판정 UI를 띄운 횟수 (Show 노트 수)
    private int shownCount = 0;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    private double SongT => (rhythmManager as RhythmManager)?.SongTime ?? -1;

    private void JudgeLog(string msg)
    {
        if (judgeLog) Debug.Log(msg);
    }

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[1-7] SetDemoMode = {isDemoMode}");
    }

    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.4f;
    public override float hitWindowOverride => 1f;

    protected override float TimerDuration => 10f;
    protected override string MinigameTitle => "금지물품 압수";

    protected override string MinigameExplain => "화면에서 조사 아이콘이 뜨면 사라지기 전 정확한 타이미에 터치해주세요.";

    private void Start()
    {
        //StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();

        currentRound = 0;
        inputIndex = 0;
        totalSuccess = 0;

        awaitingJudge = false;
        roundActive = false;
        gameEnded = false;
        transitionRunning = false;
        pendingShow = false;
        shownCount = 0;

        if (holdJudge != null)
        {
            holdJudge.SetMinigame(this);
            holdJudge.ResetAll();
        }

        SpawnAndEnterNextPrisoner();
    }

    public override void OnRhythmEvent(string action)
    {
        if (gameEnded) return;
        if (string.IsNullOrWhiteSpace(action)) return;

        action = action.Trim();

        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        switch (action)
        {
            case "Move":
                HandleMoveEvent();
                break;

            case "Show":
                HandleShowEvent();
                break;

            case "Input":
                HandleInputEvent();
                break;
        }
    }

    private void HandleMoveEvent()
    {
        if (transitionRunning || gameEnded) return;

        pendingShow = false;

        if (prisoner != null)
            StartCoroutine(Co_MoveTransition());
        else
            SpawnAndEnterNextPrisoner();
    }

    private void HandleShowEvent()
    {
        if (prisoner == null) return;
        if (inputIndex >= inputPerRound) return;

        if (!roundActive)
        {
            pendingShow = true;
            return;
        }

        pendingShow = false;
        awaitingJudge = false;

        shownCount++;
        JudgeLog($"[1-7 Judge] SHOW #{shownCount} song={SongT:F3}");

        holdJudge?.ShowPreviewUI(inputIndex, prisoner.transform);
    }

    private void HandleInputEvent()
    {
        // 더 이상 입력 허용 타이밍으로 쓰지 않음.
        // 리듬 기준점은 RhythmManager가 이미 알고 있으므로 여기선 아무것도 안 함.
        Debug.Log("Input 이벤트 도착");
    }

    public override void OnPlayerInput(string action = null)
    {
        // 실제 플레이어 입력은 시범 중에 막는다
        if (IsDemoMode) return;

        SubmitInput(action);
    }

    // 플레이어/시범 공통 입력 처리
    // (기존엔 시범의 ExecutePracticeAction도 OnPlayerInput을 거쳐서 IsDemoMode에 막혔음)
    private void SubmitInput(string action)
    {
        if (IsInputLocked) return;
        if (!roundActive) return;
        if (transitionRunning) return;
        if (gameEnded) return;
        if (prisoner == null) return;
        if (inputIndex >= inputPerRound) return;
        if (awaitingJudge) return;

        awaitingJudge = true;
        if (clickSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
        {
            GameRoot.Instance.Audio.PlaySfx(clickSound);
        }


        JudgeLog($"[1-7 Judge] 터치 song={SongT:F3}");

        base.OnPlayerInput(action ?? "Input");

        // 판정 범위 안에 노트가 없으면 판정이 오지 않는다 → 잠금 풀기
        if (awaitingJudge)
        {
            awaitingJudge = false;
            JudgeLog($"[1-7 Judge] 노트 범위 밖 터치 → 무시 song={SongT:F3}");
        }
    }

    public void OnHoldButtonPressed()
    {
        if (IsDemoMode) return;

        OnPlayerInput("Input");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // 점수 집계(Perfect/Good 카운트)는 Base가 한다. 이게 빠져 있어서 점수가 항상 0이었음.
        base.OnJudgement(judgement);

        if (!roundActive) return;
        if (prisoner == null) return;

        bool fromPlayer = awaitingJudge;
        awaitingJudge = false;

        // 시간초과 Miss는 노트 시각 + hitWindow 뒤에 늦게 도착한다.
        // 그 사이 다음 Show로 새 UI가 떠 있으면, 그 새 UI를 지우면 안 된다.
        bool newerUiShown = shownCount > inputIndex + 1;

        JudgeLog($"[1-7 Judge] 판정 {judgement} ({(fromPlayer ? "터치" : "시간초과")}) 노트#{inputIndex + 1} song={SongT:F3}" +
                 (!fromPlayer && newerUiShown ? " → 다음 UI 유지" : ""));

        if (fromPlayer || !newerUiShown)
            holdJudge?.PlayJudgeFeedback(judgement);

        if (judgement == JudgementResult.Perfect || judgement == JudgementResult.Good)
            totalSuccess++;

        inputIndex++;

        if (inputIndex >= inputPerRound)
        {
            roundActive = false;
            pendingShow = false;
        }
    }

    private IEnumerator Co_MoveTransition()
    {
        transitionRunning = true;

        roundActive = false;
        awaitingJudge = false;
        pendingShow = false;

        if (prisoner != null)
        {
            var item = prisoner.GetProhibitedItem();

            if (item != null)
                prisoner.DropToBasket(basket);

            yield return new WaitForSeconds(throwToExitDelay);

            yield return prisoner.ExitToLeftAndDestroy(prisonerExitDuration);

            prisoner = null;
        }

        holdJudge?.ResetAll();

        currentRound++;

        if (currentRound >= totalRound)
        {
            gameEnded = true;
            Debug.Log($"Game End. totalSuccess = {totalSuccess}");
        }
        else
        {
            inputIndex = 0;
            SpawnAndEnterNextPrisoner();
        }

        transitionRunning = false;
    }

    private void SpawnAndEnterNextPrisoner()
    {
        if (prisonerSpawner == null)
        {
            Debug.LogError("PrisonerSpawner1_7 미할당");
            return;
        }

        GameObject newPrisonerObj = prisonerSpawner.SpawnRandomPrisoner(transform);

        if (newPrisonerObj == null)
        {
            Debug.LogError("새 죄수 생성 실패");
            return;
        }

        prisoner = newPrisonerObj.GetComponent<PrisonerController1_7>();

        if (prisoner == null)
        {
            Debug.LogError("PrisonerController1_7 없음");
            return;
        }

        prisoner.OnArrived -= OnPrisonerArrived;
        prisoner.OnArrived += OnPrisonerArrived;

        roundActive = false;
        awaitingJudge = false;
        inputIndex = 0;
        pendingShow = false;
        shownCount = 0;

        holdJudge?.ResetAll();

        prisoner.EnterFromRight();
    }

    private void OnPrisonerArrived()
    {
        if (prisoner == null) return;

        roundActive = true;
        awaitingJudge = false;

        holdJudge?.PrepareRound(prisoner.transform, inputPerRound);

        if (pendingShow && inputIndex < inputPerRound)
        {
            pendingShow = false;
            shownCount++;
            JudgeLog($"[1-7 Judge] SHOW #{shownCount} (도착 후 지연 표시) song={SongT:F3}");
            holdJudge?.ShowPreviewUI(inputIndex, prisoner.transform);
        }
    }

    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim();

        if (actionType != "Input")
            return;

        if (gameEnded || !roundActive || prisoner == null)
            return;

        if (inputIndex >= inputPerRound)
            return;

        Debug.Log($"[1-7 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}, inputIndex={inputIndex}");

        // 시범: 실제 유저가 누른 것처럼 판정까지 보낸다
        SubmitInput("Input");
    }
}