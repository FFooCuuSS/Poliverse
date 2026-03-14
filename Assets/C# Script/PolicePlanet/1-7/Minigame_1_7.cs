using System.Collections;
using UnityEngine;

public class Minigame_1_7 : MiniGameBase
{
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

    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.4f;
    public override float hitWindowOverride => 1f;

    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "금지야!";

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
        if (IsInputLocked) return;
        if (!roundActive) return;
        if (transitionRunning) return;
        if (gameEnded) return;
        if (prisoner == null) return;
        if (inputIndex >= inputPerRound) return;
        if (awaitingJudge) return;

        awaitingJudge = true;

        Debug.Log("Minigame_1_7 OnPlayerInput 호출됨");

        base.OnPlayerInput(action ?? "Input");
    }

    public void OnHoldButtonPressed()
    {
        OnPlayerInput("Input");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (!roundActive) return;
        if (prisoner == null) return;

        Debug.Log($"OnJudgement : {judgement}");

        awaitingJudge = false;

        holdJudge?.PlayJudgeFeedback(judgement);

        if (judgement == JudgementResult.Perfect || judgement == JudgementResult.Good)
            totalSuccess++;

        inputIndex++;

        if (inputIndex >= inputPerRound)
        {
            roundActive = false;
            pendingShow = false;
            Debug.Log("라운드 종료. 다음 Move에서 전환.");
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
            EndMinigameOnly();
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
            holdJudge?.ShowPreviewUI(inputIndex, prisoner.transform);
        }
    }

    private void EndMinigameOnly()
    {
        Debug.Log("Minigame 1_7 finished.");
    }
}