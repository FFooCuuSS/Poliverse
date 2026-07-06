using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_2_5 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "½×¾Æ¶ó!";

     public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.45f;
    public override float hitWindowOverride => 0.6f;

    private bool ended;
    private bool inputOpen;
   
    public bool IsInputOpen => inputOpen;

    private IceCreamSpawner2_5 spawner;
    private IceCream2_5 currentIceCream;
    private Queue<IceCream2_5> iceCreamQueue = new Queue<IceCream2_5>();
    private IceCreamFloor floor;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnPlayerInput();
        }
    }

    public override void StartGame()
    {
        base.StartGame();

        spawner = FindAnyObjectByType<IceCreamSpawner2_5>();
        floor = FindAnyObjectByType<IceCreamFloor>();

        ended = false;
        inputOpen = false;

        //iceCreamQueue.Clear();
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        Debug.Log($"{gameObject.name} ¸®µë¸Þ¼¼Áö: {action}");

        action = action.Trim();

        switch (action)
        {
            case "Show":
                IceCream2_5 iceCream = spawner.SpawnIceCream();
                iceCreamQueue.Enqueue(iceCream);
                break;
            case "Input":
                Debug.Log("Input");
                inputOpen = true;
                break;
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        if (ended) return;
        if (!inputOpen) return;

        inputOpen = false;

        rhythmManager?.ReceivePlayerInput("Input");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);

        if (iceCreamQueue.Count == 0)
            return;

        IceCream2_5 iceCream = iceCreamQueue.Dequeue();

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                iceCream.Drop();
                break;

            case JudgementResult.Miss:
                Destroy(iceCream.gameObject);
                break;
        }
    }
}
