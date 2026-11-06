using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_4_2 : MiniGameBase
{
    [Header("Dot Prefabs")]
    public GameObject yellowDotPrefab;
    public GameObject redDotPrefab;
    public Transform startPoint;

    [Header("Player")]
    public SpriteRenderer playerRenderer;
    public Sprite idleSprite;
    public Sprite attackSprite;
    public Sprite damageSprite;
    public float returnToIdleDelay = 0.4f;

    [Header("Input Window")]
    [Tooltip("점이 판정구역에 도착하는 번째 이동 (CSV의 Move 횟수)")]
    public int arriveStep = 6;
    private bool inputOpen;

    // ===== 미니게임 기본 정보 =====
    protected override float TimerDuration => 20f;

    protected override string MinigameTitle => "적 요격";

    protected override string MinigameExplain => "우측에서 음파가 칸 단위로 다가옵니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "음파가 판정구역에 도달했을 때 정확히 클릭하면 점수를 획득합니다."
    };

    private readonly List<DotMove_4_2> yellowDots = new List<DotMove_4_2>();
    private readonly List<DotMove_4_2> redDots = new List<DotMove_4_2>();

    public override void StartGame()
    {
        base.StartGame();
        inputOpen = false;
    }

    public override void OnRhythmEvent(string action)
    {
        switch (action)
        {
            case "Show1":
                yellowDots.Add(Spawn(yellowDotPrefab));
                break;

            case "Show2":
                redDots.Add(Spawn(redDotPrefab));
                break;

            case "Move1":
                MoveAll(yellowDots);
                break;

            case "Move2":
                MoveAll(redDots);
                break;

            case "Input":
                FadeAll(yellowDots);
                FadeAll(redDots);
                break;
        }
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);   // 점수 집계라서 반드시 호출
        inputOpen = false;

        if (playerRenderer == null) return;

        Sprite s = judgement == JudgementResult.Miss ? damageSprite : attackSprite;
        if (s != null) playerRenderer.sprite = s;

        CancelInvoke(nameof(ReturnToIdle));
        Invoke(nameof(ReturnToIdle), returnToIdleDelay);
    }

    private void ReturnToIdle()
    {
        if (playerRenderer != null && idleSprite != null)
            playerRenderer.sprite = idleSprite;
    }

    private DotMove_4_2 Spawn(GameObject prefab)
    {
        var go = Instantiate(prefab, startPoint.position, Quaternion.identity);
        return go.GetComponent<DotMove_4_2>();
    }

    private void MoveAll(List<DotMove_4_2> dots)
    {
        dots.RemoveAll(d => d == null);
        foreach (var d in dots)
        {
            d.Move();

            // 도착 한 칸 전부터 입력을 연다
            if (d.Step == arriveStep - 1)
                inputOpen = true;
        }
    }

    private void FadeAll(List<DotMove_4_2> dots)
    {
        dots.RemoveAll(d => d == null);
        foreach (var d in dots)
            d.FadeOutAndDestroy(d.lingerTime);
        dots.Clear();
    }
    private void Update()
    {
        if (IsInputLocked || !inputOpen) return;

        if (Input.GetMouseButtonDown(0))
        {
            OnPlayerInput();
            inputOpen = false;
        }
    }
}