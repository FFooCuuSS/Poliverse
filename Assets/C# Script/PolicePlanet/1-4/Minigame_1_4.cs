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
    public int totalRound = 3;
    public int accessoryPerRound = 3;

    private int currentRound;
    private int accessoryIndex;
    private int totalSuccess;

    private bool inputOpen;
    private bool awaitingJudge;
    private bool ended;

    private List<Accessory> orderedAccessories;
    [SerializeField] private SpawnManager spawnManager;

    void Start()
    {
        StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();

        currentRound = 0;
        accessoryIndex = 0;
        totalSuccess = 0;

        inputOpen = false;
        awaitingJudge = false;
        ended = false;

        
    }

    public void SetAccessoryOrder(List<Accessory> accessories)
    {
        orderedAccessories = accessories;

        foreach (var acc in orderedAccessories)
            acc.Init(this);

        StartRound();
    }

    void StartRound()
    {
        Debug.Log("StartRound : " + (currentRound + 1));

        accessoryIndex = 0;
        inputOpen = false;
        awaitingJudge = false;

        foreach (var acc in orderedAccessories)
            acc.ResetAccessory();
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;

        action = action.Trim();
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        if (action == "Swipe")
        {
            inputOpen = true;
            awaitingJudge = false;
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        if (IsInputLocked) return;
        if (!inputOpen || awaitingJudge) return;

        awaitingJudge = true;

        base.OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (!inputOpen) return;

        inputOpen = false;
        awaitingJudge = false;

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:

                totalSuccess++;
                RemoveNextAccessory();
                break;

            case JudgementResult.Miss:

                RemoveNextAccessory();
                break;
        }

        accessoryIndex++;

        CheckRoundEnd();
    }

    void RemoveNextAccessory()
    {
        foreach (var acc in orderedAccessories)
        {
            if (!acc.IsRemoved)
            {
                acc.Remove();
                break;
            }
        }
    }

    void CheckRoundEnd()
    {
        if (accessoryIndex < accessoryPerRound)
            return;

        Debug.Log("Round End");

        currentRound++;

        if (currentRound >= totalRound)
        {
            EndGame();
        }
        else
        {
            spawnManager.SpawnNewRound();
            StartRound();
        }
    }


    void EndGame()
    {
        if (ended) return;

        ended = true;

        Debug.Log("Game End success = " + totalSuccess);

        if (totalSuccess >= 3)
            Success();
        else
            Fail();
    }
}