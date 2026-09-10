using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame2_4 : MiniGameBase
{
    protected override float TimerDuration => 5f;
    protected override string MinigameExplain => "같은 색끼리 옮겨담아라!";

    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.45f;
    public override float hitWindowOverride => 0.6f;

    private bool ended;
    private bool inputOpen;
    public bool IsInputOpen => inputOpen;

    [SerializeField] private BottleSpawner2_4 spawner;
    [SerializeField] private Kettle2_4 kettle;

    [SerializeField] private GameObject liquidPrefab;
    [SerializeField] private Sprite[] fillingSprites;

    private Bottle2_4 currentBottle;

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        inputOpen = false;
        currentBottle = null;
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        action = action.Trim();

        switch (action)
        {
            case "Show":
                currentBottle = spawner.SpawnBottle();
                break;
            case "Input":
                inputOpen = true;
                break;
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        if (ended) return;
        if (!inputOpen) return;

        inputOpen = false;

        kettle.Pour();

        rhythmManager?.ReceivePlayerInput("Input");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:

                if (currentBottle == null)
                {
                    Debug.LogWarning("현재 채울 보틀이 없습니다.");
                    return;
                }

                CreateRandomFilling();

                break;

            case JudgementResult.Miss:

                break;
        }
    }

    private void CreateRandomFilling()
    {
        if (liquidPrefab == null || fillingSprites == null || fillingSprites.Length == 0)
        {
            Debug.LogError("Liquid Prefab 또는 Filling Sprite가 비어있습니다.");
            return;
        }

        int randomIndex = Random.Range(0, fillingSprites.Length);
        Sprite selectedSprite = fillingSprites[randomIndex];

        GameObject liquid = Instantiate(
            liquidPrefab,
            kettle.PourPoint.position,
            Quaternion.identity
        );

        SpriteRenderer liquidRenderer = liquid.GetComponent<SpriteRenderer>();

        if (liquidRenderer != null)
        {
            liquidRenderer.sprite = selectedSprite;
            currentBottle.FillBottle(liquid);
        }
        else
        {
            Destroy(liquid);
        }
    }
}
