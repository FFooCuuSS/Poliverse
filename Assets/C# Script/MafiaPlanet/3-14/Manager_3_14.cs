using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static MiniGameBase;

public class Manager_3_14 : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerController_3_14 player;
    [SerializeField] private Transform roundParent;

    [Header("Cell Root")]
    [Tooltip("비워둬도 됨. 비워두면 빈 GameObject에 WindowCell_3_14를 붙여 생성함.")]
    [SerializeField] private GameObject cellRootPrefab;

    [Header("Window Resources")]
    [SerializeField] private GameObject offWindowPrefab;
    [SerializeField] private GameObject onWindowPrefab;

    [Header("Round Data")]
    [Tooltip("각 라운드에서 성공 입력으로 전진해야 하는 횟수. 예: 2,2,4면 생성 칸은 3,3,5개.")]
    [SerializeField] private int[] roundStepCounts = { 2, 2, 4 };

    [Header("Cell Pieces")]
    [Tooltip("한 논리 칸을 몇 개의 오브젝트 조각으로 보여줄지.")]
    [SerializeField] private int piecesPerCell = 2;

    [Tooltip("한 칸 안에서 조각끼리 벌어지는 거리.")]
    [SerializeField] private float pieceSpacingX = 0.35f;

    [Header("Layout")]
    [SerializeField] private float roundOriginX = -3f;
    [SerializeField] private float roundOriginY = 0f;
    [SerializeField] private float wallSpacing = 3f;

    [Header("Input")]
    [Tooltip("Input 신호 후 아무 입력이 없으면 Miss 처리할 시간. hitWindowOverride보다 살짝 크게.")]
    [SerializeField] private float inputAutoCloseDelay = 0.55f;

    [Header("Player Move")]
    [SerializeField] private float playerStepDuration = 0.18f;
    [SerializeField] private float playerResetDuration = 0.15f;

    [Header("Round Transition")]
    [SerializeField] private float transitionDuration = 0.25f;
    [SerializeField] private float roundEnterOffsetX = 9f;
    [SerializeField] private float roundExitOffsetX = 9f;
    [SerializeField] private float finalSuccessDelay = 0.25f;

    [Header("Final")]
    [SerializeField] private bool finishWhenLastInputResolved = true;

    private Minigame_3_14 minigame;

    private Transform currentRoundRoot;
    private readonly List<WindowCell_3_14> currentCells = new List<WindowCell_3_14>();

    private int currentRoundIndex;
    private int showIndexInRound;
    private int inputIndexInRound;
    private int activeInputStepIndex = -1;

    private bool inputOpen;
    private bool awaitingJudge;
    private bool transitioning;
    private bool ended;

    private int perfectCount;
    private int goodCount;
    private int missCount;
    private int successMoveCount;

    private Coroutine inputAutoCloseCoroutine;

    private int CurrentStepCount
    {
        get
        {
            if (roundStepCounts == null || roundStepCounts.Length == 0)
                return 0;

            int index = Mathf.Clamp(currentRoundIndex, 0, roundStepCounts.Length - 1);
            return Mathf.Max(0, roundStepCounts[index]);
        }
    }

    private bool IsLastRound => currentRoundIndex >= roundStepCounts.Length - 1;

    public void OnMinigameStart(Minigame_3_14 mg)
    {
        minigame = mg;

        currentRoundIndex = 0;
        showIndexInRound = 0;
        inputIndexInRound = 0;
        activeInputStepIndex = -1;

        inputOpen = false;
        awaitingJudge = false;
        transitioning = false;
        ended = false;

        perfectCount = 0;
        goodCount = 0;
        missCount = 0;
        successMoveCount = 0;

        StopInputAutoClose();
        ClearAllRounds();

        currentRoundRoot = BuildRound(currentRoundIndex, Vector3.zero);

        if (player != null)
        {
            player.OnMinigameStart(this);
            player.ResetToRoundStart(0f);
        }
    }

    private Transform BuildRound(int roundIndex, Vector3 localPosition)
    {
        currentCells.Clear();

        GameObject rootObj = new GameObject($"Round_{roundIndex + 1}");
        Transform root = rootObj.transform;

        if (roundParent != null)
            root.SetParent(roundParent, false);

        root.localPosition = localPosition;

        int stepCount = CurrentStepCount;
        int cellCount = stepCount + 1;

        for (int i = 0; i < cellCount; i++)
        {
            GameObject cellObj;

            if (cellRootPrefab != null)
                cellObj = Instantiate(cellRootPrefab, root);
            else
            {
                cellObj = new GameObject($"Cell_{i}");
                cellObj.transform.SetParent(root, false);
            }

            cellObj.name = $"Cell_{i}";
            cellObj.transform.localPosition = new Vector3(
                roundOriginX + i * wallSpacing,
                roundOriginY,
                0f
            );
            cellObj.transform.localRotation = Quaternion.identity;

            WindowCell_3_14 cell = cellObj.GetComponent<WindowCell_3_14>();

            if (cell == null)
                cell = cellObj.GetComponentInChildren<WindowCell_3_14>();

            if (cell == null)
                cell = cellObj.AddComponent<WindowCell_3_14>();

            cell.BuildVisuals(
                offWindowPrefab,
                onWindowPrefab,
                piecesPerCell,
                pieceSpacingX
            );

            if (i == 0)
                cell.SetOffImmediate();
            else
                cell.SetOnImmediate();

            currentCells.Add(cell);
        }

        return root;
    }

    private void ClearAllRounds()
    {
        currentCells.Clear();

        if (roundParent == null) return;

        for (int i = roundParent.childCount - 1; i >= 0; i--)
        {
            Destroy(roundParent.GetChild(i).gameObject);
        }

        currentRoundRoot = null;
    }

    // =========================
    // CSV Show
    // =========================
    public void ShowNextCall()
    {
        if (ended) return;
        if (transitioning) return;

        int targetCellIndex = showIndexInRound + 1;

        if (targetCellIndex >= currentCells.Count)
            return;

        WindowCell_3_14 cell = currentCells[targetCellIndex];

        if (cell != null)
            cell.SetOff();

        showIndexInRound++;
    }

    // =========================
    // CSV Input
    // =========================
    public void OnInputWindowOpened()
    {
        if (ended) return;
        if (transitioning) return;

        if (inputIndexInRound >= CurrentStepCount)
            return;

        activeInputStepIndex = inputIndexInRound;
        inputIndexInRound++;

        inputOpen = true;
        awaitingJudge = false;

        int targetCellIndex = activeInputStepIndex + 1;

        if (IsValidCellIndex(targetCellIndex))
            currentCells[targetCellIndex]?.SetInputReady();
    }

    public void RequestMoveInput()
    {
        if (ended) return;
        if (transitioning) return;
        if (!inputOpen) return;
        if (awaitingJudge) return;
        if (activeInputStepIndex < 0) return;
        if (minigame == null) return;

        awaitingJudge = true;
        minigame.SubmitPlayerInput("Input");
    }

    // =========================
    // Judgement Callback
    // =========================
    public void OnAccepted(JudgementResult judgement)
    {
        if (ended) return;
        if (activeInputStepIndex < 0) return;

        StopInputAutoClose();

        int judgedStep = activeInputStepIndex;
        int targetCellIndex = judgedStep + 1;

        inputOpen = false;
        awaitingJudge = false;
        activeInputStepIndex = -1;

        if (judgement == JudgementResult.Perfect)
            perfectCount++;
        else if (judgement == JudgementResult.Good)
            goodCount++;

        successMoveCount++;

        if (IsValidCellIndex(targetCellIndex))
            currentCells[targetCellIndex]?.MarkSuccess();

        if (player != null)
            player.MoveRightOneStep(playerStepDuration);

        TryFinishIfFinalRoundResolved();
    }

    public void OnMiss()
    {
        if (ended) return;

        int missedStep = activeInputStepIndex;

        if (missedStep < 0)
            missedStep = Mathf.Clamp(inputIndexInRound - 1, 0, CurrentStepCount - 1);

        RegisterMiss(missedStep);
    }

    private void RegisterMiss(int missedStep)
    {
        StopInputAutoClose();

        int targetCellIndex = missedStep + 1;

        inputOpen = false;
        awaitingJudge = false;
        activeInputStepIndex = -1;

        missCount++;

        if (IsValidCellIndex(targetCellIndex))
            currentCells[targetCellIndex]?.MarkMiss();

        TryFinishIfFinalRoundResolved();
    }

    // =========================
    // CSV Move
    // =========================
    public void OnMoveSignal()
    {
        if (ended) return;
        if (transitioning) return;

        ClosePreviousInputAsMissIfNeeded();

        if (IsLastRound)
        {
            StartCoroutine(FinishSuccessAfterDelay(finalSuccessDelay));
            return;
        }

        StartCoroutine(TransitionToNextRound());
    }

    public void OnEndSignal()
    {
        if (ended) return;

        ClosePreviousInputAsMissIfNeeded();
        StartCoroutine(FinishSuccessAfterDelay(finalSuccessDelay));
    }

    private IEnumerator TransitionToNextRound()
    {
        transitioning = true;

        StopInputAutoClose();

        inputOpen = false;
        awaitingJudge = false;
        activeInputStepIndex = -1;

        Transform oldRoot = currentRoundRoot;

        if (oldRoot != null)
        {
            WindowCell_3_14[] oldCells = oldRoot.GetComponentsInChildren<WindowCell_3_14>();

            for (int i = 0; i < oldCells.Length; i++)
                oldCells[i].SetOff();
        }

        currentRoundIndex++;
        showIndexInRound = 0;
        inputIndexInRound = 0;

        Transform newRoot = BuildRound(currentRoundIndex, new Vector3(roundEnterOffsetX, 0f, 0f));
        currentRoundRoot = newRoot;

        if (player != null)
            player.ResetToRoundStart(playerResetDuration);

        if (oldRoot != null)
        {
            oldRoot.DOLocalMoveX(-roundExitOffsetX, transitionDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    if (oldRoot != null)
                        Destroy(oldRoot.gameObject);
                });
        }

        if (newRoot != null)
        {
            newRoot.DOLocalMoveX(0f, transitionDuration)
                .SetEase(Ease.InOutQuad);
        }

        yield return new WaitForSeconds(transitionDuration);

        transitioning = false;
    }

    private void TryFinishIfFinalRoundResolved()
    {
        if (!finishWhenLastInputResolved) return;
        if (!IsLastRound) return;
        if (inputIndexInRound < CurrentStepCount) return;
        if (ended) return;

        StartCoroutine(FinishSuccessAfterDelay(finalSuccessDelay));
    }

    private IEnumerator FinishSuccessAfterDelay(float delay)
    {
        if (ended) yield break;

        ended = true;

        StopInputAutoClose();

        inputOpen = false;
        awaitingJudge = false;
        activeInputStepIndex = -1;

        yield return new WaitForSeconds(delay);

        Debug.Log($"3-14 결과 / Perfect:{perfectCount}, Good:{goodCount}, Miss:{missCount}, Move:{successMoveCount}");

        minigame?.Succeed();
    }

    private void ClosePreviousInputAsMissIfNeeded()
    {
        if (!inputOpen) return;
        if (awaitingJudge) return;
        if (activeInputStepIndex < 0) return;

        RegisterMiss(activeInputStepIndex);
    }

    private void ForceCloseInputWithoutJudgement()
    {
        StopInputAutoClose();

        inputOpen = false;
        awaitingJudge = false;
        activeInputStepIndex = -1;
    }

    private bool IsValidCellIndex(int index)
    {
        return index >= 0 && index < currentCells.Count;
    }

    private void StartInputAutoClose(int stepIndex)
    {
        StopInputAutoClose();

        if (inputAutoCloseDelay <= 0f) return;

        inputAutoCloseCoroutine = StartCoroutine(AutoCloseInputAfterDelay(stepIndex));
    }

    private void StopInputAutoClose()
    {
        if (inputAutoCloseCoroutine != null)
        {
            StopCoroutine(inputAutoCloseCoroutine);
            inputAutoCloseCoroutine = null;
        }
    }

    private IEnumerator AutoCloseInputAfterDelay(int stepIndex)
    {
        yield return new WaitForSeconds(inputAutoCloseDelay);

        inputAutoCloseCoroutine = null;

        if (ended) yield break;
        if (transitioning) yield break;
        if (!inputOpen) yield break;
        if (awaitingJudge) yield break;
        if (activeInputStepIndex != stepIndex) yield break;

        RegisterMiss(stepIndex);
    }

    public void ButtonMovePlayer()
    {
        RequestMoveInput();
    }
}