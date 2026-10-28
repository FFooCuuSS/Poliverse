using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_4 : MiniGameBase, IPracticeDemoInput
{
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.4f;
    public override float hitWindowOverride => 0.8f;

    protected override float TimerDuration => 10f;

    protected override string MinigameTitle => "몽타주";

    protected override string MinigameExplain => "죄수의 변장이 순서대로 깜빡이며 타이밍을 알려줍니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "잠시 후 변장을 깜빡인 순서대로 터치해주세요."
    };

    [Header("Round Setting")]
    [SerializeField] private int totalRound = 2;
    [SerializeField] private int accessoryPerRound = 3;

    [Header("Refs")]
    [SerializeField] private SpawnManager spawnManager;
    [Tooltip("비우면 자식에서 자동 탐색")]
    [SerializeField] private AccessoryBlinkManager blinkManager;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    private int currentRound;
    private int swipeCountInRound;
    private int showCountInRound;
    private int totalSuccess;

    private bool awaitingJudge;
    private bool roundEnding;
    private bool ended;

    private readonly List<Accessory> orderedAccessories = new List<Accessory>();

    private bool isDemoMode;
    public bool IsDemoMode => isDemoMode;

    // 입력은 들어왔지만 아직 판정 전인 악세사리
    private Accessory pendingAccessory;

    private readonly Accessory.AccessoryType[] requiredOrder =
    {
        Accessory.AccessoryType.Hat,
        Accessory.AccessoryType.Glasses,
        Accessory.AccessoryType.Mustache
    };

    private double SongT => (rhythmManager as RhythmManager)?.SongTime ?? -1;

    private void JudgeLog(string msg)
    {
        if (judgeLog) Debug.Log(msg);
    }

    protected override void Awake()
    {
        base.Awake();

        if (blinkManager == null)
            blinkManager = GetComponentInChildren<AccessoryBlinkManager>(true);
    }

    public void SetDemoMode(bool value)
    {
        isDemoMode = value;

        foreach (var acc in orderedAccessories)
        {
            if (acc != null)
                acc.SetDemoInputBlocked(value);
        }

        Debug.Log($"[1-4 Demo] 데모 모드: {isDemoMode}");
    }

    public override void StartGame()
    {
        base.StartGame();

        currentRound = 0;
        swipeCountInRound = 0;
        showCountInRound = 0;
        totalSuccess = 0;

        awaitingJudge = false;
        roundEnding = false;
        ended = false;
        pendingAccessory = null;
    }

    public void SetAccessoryOrder(List<Accessory> accessories)
    {
        orderedAccessories.Clear();

        if (accessories != null)
        {
            foreach (var acc in accessories)
            {
                if (acc == null) continue;
                orderedAccessories.Add(acc);
                acc.Init(this);
            }
        }

        if (isDemoMode)
        {
            foreach (var acc in orderedAccessories)
            {
                if (acc != null)
                    acc.SetDemoInputBlocked(true);
            }
        }

        StartRound();
    }

    private void StartRound()
    {
        Debug.Log($"[1-4] StartRound : {currentRound + 1}");

        swipeCountInRound = 0;
        showCountInRound = 0;
        awaitingJudge = false;
        roundEnding = false;
        pendingAccessory = null;

        // 노트 전에도 미리 잡을 수 있게 현재 순서 악세사리는 바로 열어둔다.
        // 판정 범위 밖에서 놓으면 RhythmManager가 무시하므로 손해 없음.
        if (isDemoMode)
            LockAllAccessories();
        else
            EnableOnlyCurrentAccessory();
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;

        action = action?.Trim();

        if (action == "Show")
        {
            if (blinkManager != null && blinkManager.UseRhythmShow)
            {
                JudgeLog($"[1-4 Judge] SHOW 힌트 {showCountInRound} song={SongT:F3}");
                blinkManager.BlinkAt(showCountInRound);
            }

            showCountInRound++;
            return;
        }

        if (action == "Swipe")
            JudgeLog($"[1-4 Judge] SWIPE 노트 song={SongT:F3} 현재 순서={GetCurrentRequiredType()}");
    }

    public void TryAccessoryInput(Accessory clickedAccessory)
    {
        if (isDemoMode) return;
        if (clickedAccessory == null) return;
        if (IsInputLocked) return;
        if (ended || roundEnding) return;
        if (awaitingJudge) return;

        var requiredType = GetCurrentRequiredType();
        if (clickedAccessory.Type != requiredType)
        {
            JudgeLog($"[1-4 Judge] 순서 틀림 {clickedAccessory.Type} (필요: {requiredType}) song={SongT:F3}");
            return;
        }

        JudgeLog($"[1-4 Judge] 입력 {clickedAccessory.Type} song={SongT:F3}");

        awaitingJudge = true;
        pendingAccessory = clickedAccessory;

        LockAllAccessories();

        // ReceivePlayerInput는 판정 시 OnJudgement를 동기로 호출한다.
        base.OnPlayerInput("Swipe");

        // 판정 범위(hitWindow) 안에 노트가 없으면 아무 판정도 안 온다 → 입력 무효, 다시 열어줌
        if (awaitingJudge)
        {
            JudgeLog($"[1-4 Judge] 노트 범위 밖 입력 → 무시 song={SongT:F3}");

            awaitingJudge = false;
            pendingAccessory = null;

            if (!ended && !roundEnding)
                EnableOnlyCurrentAccessory();
        }
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended || roundEnding) return;

        base.OnJudgement(judgement);

        // 내 입력 판정이 아니면서 Miss도 아니면 무시
        if (!awaitingJudge && judgement != JudgementResult.Miss) return;

        bool fromPlayer = awaitingJudge;

        awaitingJudge = false;
        LockAllAccessories();

        JudgeLog($"[1-4 Judge] 판정 {judgement} ({(fromPlayer ? "입력" : "시간초과")}) 순서={GetCurrentRequiredType()} song={SongT:F3}");

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                totalSuccess++;

                if (pendingAccessory != null)
                    pendingAccessory.RemoveWithDelay(0f);
                break;

            case JudgementResult.Miss:
                break;
        }

        pendingAccessory = null;

        // 순서는 성공 여부와 무관하게 라운드 진행 기준으로 넘긴다.
        swipeCountInRound++;

        // 시범 모드에서는 악세사리를 열지 않는다 (자동 입력만)
        if (!isDemoMode && swipeCountInRound < accessoryPerRound)
            EnableOnlyCurrentAccessory();

        CheckRoundEnd();
    }

    private void CheckRoundEnd()
    {
        if (swipeCountInRound < accessoryPerRound)
            return;

        StartCoroutine(CoEndRound());
    }

    private IEnumerator CoEndRound()
    {
        if (roundEnding) yield break;
        roundEnding = true;

        yield return new WaitForSeconds(0.5f);

        yield return spawnManager.DespawnCurrentRoundObjectsWithFade(0.2f);

        currentRound++;

        if (currentRound >= totalRound)
        {
            EndGame();
            yield break;
        }

        yield return spawnManager.SpawnNewRoundWithFade(0.2f);
    }

    private void EndGame()
    {
        if (ended) return;

        ended = true;
        awaitingJudge = false;
        roundEnding = true;
        pendingAccessory = null;

        LockAllAccessories();

        Debug.Log($"[1-4] Game End success = {totalSuccess}");
    }

    private Accessory.AccessoryType GetCurrentRequiredType()
    {
        int safeIndex = Mathf.Clamp(swipeCountInRound, 0, requiredOrder.Length - 1);
        return requiredOrder[safeIndex];
    }

    private void EnableOnlyCurrentAccessory()
    {
        var requiredType = GetCurrentRequiredType();

        foreach (var acc in orderedAccessories)
        {
            if (acc == null || acc.IsRemoved)
            {
                if (acc != null) acc.SetInteractableNow(false);
                continue;
            }

            acc.SetInteractableNow(acc.Type == requiredType);
        }
    }

    private void LockAllAccessories()
    {
        foreach (var acc in orderedAccessories)
        {
            if (acc == null) continue;
            acc.SetInteractableNow(false);
        }
    }

    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (!string.Equals(actionType, "Action", System.StringComparison.OrdinalIgnoreCase))
            return;

        Debug.Log($"[1-4 Demo] Action Index: {actionIndex}");

        int targetRound = actionIndex / accessoryPerRound;
        int roundLocalIndex = actionIndex % accessoryPerRound;

        if (currentRound != targetRound)
        {
            currentRound = targetRound;
            Debug.Log($"[1-4 Demo] 데모 라운드 전환: {currentRound + 1}");
        }

        swipeCountInRound = roundLocalIndex;

        switch (roundLocalIndex)
        {
            case 0:
                RemoveAccessoryByType(Accessory.AccessoryType.Hat);
                break;

            case 1:
                RemoveAccessoryByType(Accessory.AccessoryType.Glasses);
                break;

            case 2:
                RemoveAccessoryByType(Accessory.AccessoryType.Mustache);
                break;

            default:
                Debug.LogWarning($"[1-4 Demo] 알 수 없는 Round Local Index: {roundLocalIndex}");
                break;
        }

        swipeCountInRound++;
        CheckRoundEnd();
    }

    private void RemoveAccessoryByType(Accessory.AccessoryType type)
    {
        var targetAcc = orderedAccessories.Find(acc => acc != null && acc.Type == type && !acc.IsRemoved);
        if (targetAcc != null)
        {
            targetAcc.SetInteractableNow(true);
            targetAcc.RemoveWithDelay(0f);
            targetAcc.SetInteractableNow(false);
            Debug.Log($"[1-4 Demo] 악세서리 자동 제거: {type}");
        }
    }
}