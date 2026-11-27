using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Minigame_4_2 : MiniGameBase
{
    [Header("Success Effect")]
    public SpriteRenderer successEffect;
    [Tooltip("효과가 최종적으로 커지는 배율 (1이면 크기 변화 없음)")]
    public float effectEndScale = 1.3f;
    public float effectDuration = 0.4f;

    private Vector3 effectBaseScale;
    private bool effectReady;

    private DotMove_4_2 targetDot;

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
        HideEffect();
        targetDot = null;
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

        if (judgement != JudgementResult.Miss) PlaySuccessEffect();

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
            {
                inputOpen = true;
                targetDot = d;
            }
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

    private void PlaySuccessEffect()
    {
        if (successEffect == null) return;

        if (!effectReady) { effectBaseScale = successEffect.transform.localScale; effectReady = true; }

        Transform t = successEffect.transform;
        t.DOKill();
        successEffect.DOKill();

        // 처음 상태로 맞추고 보이게 한 뒤, 커지면서 투명해진다
        t.localScale = effectBaseScale;
        SetEffectAlpha(1f);
        if (targetDot != null)
        {
            Vector3 pos = targetDot.transform.position;
            pos.z = t.position.z;
            t.position = pos;
        }
        successEffect.gameObject.SetActive(true);

        t.DOScale(effectBaseScale * effectEndScale, effectDuration).SetEase(Ease.OutQuad).SetLink(successEffect.gameObject);
        successEffect.DOFade(0f, effectDuration).SetEase(Ease.InQuad).SetLink(successEffect.gameObject).OnComplete(HideEffect);
    }

    private void HideEffect()
    {
        if (successEffect == null) return;

        successEffect.DOKill();
        successEffect.transform.DOKill();

        if (effectReady) successEffect.transform.localScale = effectBaseScale;
        successEffect.gameObject.SetActive(false);
    }

    private void SetEffectAlpha(float a)
    {
        Color c = successEffect.color;
        c.a = a;
        successEffect.color = c;
    }
}