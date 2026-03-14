using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_4 : MiniGameBase
{
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.4f;
    public override float hitWindowOverride => 0.8f;

    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "리듬에 맞춰 악세서리를 제거하세요.";

    [Header("Round Setting")]
    [SerializeField] private int totalRound = 2;
    [SerializeField] private int accessoryPerRound = 3;

    [Header("Refs")]
    [SerializeField] private SpawnManager spawnManager;

    private int currentRound;
    private int swipeCountInRound;
    private int totalSuccess;

    private bool inputOpen;
    private bool awaitingJudge;
    private bool roundEnding;
    private bool ended;

    private readonly List<Accessory> orderedAccessories = new List<Accessory>();

    // 입력은 들어왔지만 아직 판정 전인 악세사리
    private Accessory pendingAccessory;

    private readonly Accessory.AccessoryType[] requiredOrder =
    {
        Accessory.AccessoryType.Hat,
        Accessory.AccessoryType.Glasses,
        Accessory.AccessoryType.Mustache
    };

    private void Start()
    {
        //StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();

        currentRound = 0;
        swipeCountInRound = 0;
        totalSuccess = 0;

        inputOpen = false;
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

        StartRound();
    }

    private void StartRound()
    {
        Debug.Log($"[1-4] StartRound : {currentRound + 1}");

        swipeCountInRound = 0;
        inputOpen = false;
        awaitingJudge = false;
        roundEnding = false;
        pendingAccessory = null;

        LockAllAccessories();
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended || roundEnding) return;

        action = action?.Trim();
        if (action != "Swipe") return;

        if (awaitingJudge) return;
        if (swipeCountInRound >= accessoryPerRound) return;

        inputOpen = true;
        EnableOnlyCurrentAccessory();

        Debug.Log($"[1-4] Swipe window OPEN / target = {GetCurrentRequiredType()}");
    }

    public void TryAccessoryInput(Accessory clickedAccessory)
    {
        if (clickedAccessory == null) return;
        if (IsInputLocked) return;
        if (ended || roundEnding) return;
        if (!inputOpen || awaitingJudge) return;

        var requiredType = GetCurrentRequiredType();
        if (clickedAccessory.Type != requiredType)
        {
            Debug.Log($"[1-4] Wrong accessory clicked: {clickedAccessory.Type}, required: {requiredType}");
            return;
        }

        awaitingJudge = true;
        inputOpen = false;
        pendingAccessory = clickedAccessory;

        LockAllAccessories();

        // 제거는 여기서 하지 말고, 판정 성공 후에 한다.
        base.OnPlayerInput("Swipe");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended || roundEnding) return;
        if (!awaitingJudge && judgement != JudgementResult.Miss) return;

        Debug.Log(judgement);
        awaitingJudge = false;
        inputOpen = false;
        LockAllAccessories();

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                totalSuccess++;

                if (pendingAccessory != null)
                    pendingAccessory.RemoveWithDelay(0f); // 성공 시 즉시 페이드 제거
                break;

            case JudgementResult.Miss:
                // Miss면 제거하지 않음
                break;
        }

        pendingAccessory = null;

        // 순서는 성공 여부와 무관하게 라운드 진행 기준으로 넘긴다.
        swipeCountInRound++;
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
        inputOpen = false;
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

            bool canInput = acc.Type == requiredType;
            acc.SetInteractableNow(canInput);
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
}