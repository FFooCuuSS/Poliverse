using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandcuffSequenceController : MonoBehaviour
{
    public static HandcuffSequenceController Instance;

    public HandAutoMove leftHand;
    public HandAutoMove rightHand;

    public CircleCollider2D leftHandCollider;
    public CircleCollider2D rightHandCollider;

    [Header("Cuffs (FitChecker들)")]
    [SerializeField] private HandcuffFitChecker[] cuffs;

    [Header("Direct References")]
    [SerializeField] private HandcuffFitChecker cuff1;   // 체인 달린 쪽
    [SerializeField] private HandcuffFitChecker cuff2;   // 왼손 도착 시 붙일 쪽
    [SerializeField] private ChainGenerator chainGenerator;

    [Header("Attach Position")]
    [SerializeField] private Vector3 cuff2LeftHandArrivedWorldPos = new Vector3(-6f, 0f, 0f);

    [Header("Delay")]
    [SerializeField] private float delayBetweenHands = 0.2f;

    [Header("Snap Fade")]
    [SerializeField] private float snapFadeStartDelay = 0f;
    [SerializeField] private float snapFadeDuration = 0.1f;

    public enum State { Idle, LeftMoving, RightMoving, PlayerDrag }
    public State curState { get; private set; } = State.Idle;

    private Coroutine seqJob;
    private Tween snapFadeTween;
    private SpriteRenderer[] cachedRenderers;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        CacheRenderers();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void CacheRenderers()
    {
        var list = new List<SpriteRenderer>();

        if (leftHand != null)
            list.AddRange(leftHand.GetComponentsInChildren<SpriteRenderer>(true));

        if (rightHand != null)
            list.AddRange(rightHand.GetComponentsInChildren<SpriteRenderer>(true));

        if (cuffs != null)
        {
            foreach (var c in cuffs)
            {
                if (c == null) continue;
                list.AddRange(c.GetComponentsInChildren<SpriteRenderer>(true));
            }
        }

        if (chainGenerator != null)
            list.AddRange(chainGenerator.GetComponentsInChildren<SpriteRenderer>(true));

        cachedRenderers = list.ToArray();
    }

    private void SetWholeAlpha(float alpha)
    {
        if (cachedRenderers == null) return;

        foreach (var sr in cachedRenderers)
        {
            if (sr == null) continue;

            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    public void BeginSnapFadeAll()
    {
        if (snapFadeTween != null && snapFadeTween.IsActive())
        {
            snapFadeTween.Kill();
            snapFadeTween = null;
        }

        CacheRenderers();

        if (cachedRenderers == null || cachedRenderers.Length == 0)
            return;

        Sequence seq = DOTween.Sequence();

        if (snapFadeStartDelay > 0f)
            seq.AppendInterval(snapFadeStartDelay);

        foreach (var sr in cachedRenderers)
        {
            if (sr == null) continue;
            seq.Join(sr.DOFade(0f, snapFadeDuration));
        }

        snapFadeTween = seq;
    }

    public void SpawnRound()
    {
        CacheRenderers();

        if (seqJob != null)
        {
            StopCoroutine(seqJob);
            seqJob = null;
        }

        if (snapFadeTween != null && snapFadeTween.IsActive())
        {
            snapFadeTween.Kill();
            snapFadeTween = null;
        }

        curState = State.Idle;

        if (leftHand != null) leftHand.ResetToStart(true);
        if (rightHand != null) rightHand.ResetToStart(true);

        if (leftHandCollider != null) leftHandCollider.enabled = true;
        if (rightHandCollider != null) rightHandCollider.enabled = false;

        if (chainGenerator != null)
            chainGenerator.isLeftCuffLocked = false;

        if (cuffs != null)
        {
            foreach (var c in cuffs)
            {
                if (c == null) continue;
                c.ResetForRound();
            }
        }

        SetWholeAlpha(1f);
    }

    public void StartRoundSequence()
    {
        if (seqJob != null) StopCoroutine(seqJob);
        seqJob = StartCoroutine(SequenceCo());
    }

    private IEnumerator SequenceCo()
    {
        if (leftHand == null || rightHand == null)
        {
            curState = State.Idle;
            seqJob = null;
            yield break;
        }

        int leftSteps = Mathf.Max(1, leftHand.steps);
        float leftStepInterval = leftHand.GetStepInterval();

        int rightSteps = Mathf.Max(1, rightHand.steps);
        float rightStepInterval = rightHand.GetStepInterval();

        curState = State.LeftMoving;

        for (int i = 0; i < leftSteps; i++)
        {
            leftHand.ForceStep(i);

            // 마지막 스텝 직전에 붙여도 되고, 마지막 스텝 시작 시 붙여도 됨
            if (i == leftSteps - 1)
            {
                Invoke("delayedCuffMove", 0.2f);

                if (chainGenerator != null)
                    chainGenerator.isLeftCuffLocked = true;
            }

            yield return new WaitForSeconds(leftStepInterval);
        }

        leftHand.ForceFinish();

        if (leftHandCollider != null)
            leftHandCollider.enabled = false;

        if (delayBetweenHands > 0f)
            yield return new WaitForSeconds(delayBetweenHands);

        curState = State.RightMoving;

        for (int i = 0; i < rightSteps; i++)
        {
            rightHand.ForceStep(i);
            yield return new WaitForSeconds(rightStepInterval);
        }

        rightHand.ForceFinish();

        if (rightHandCollider != null)
            rightHandCollider.enabled = true;

        curState = State.PlayerDrag;
        seqJob = null;
    }

    private void delayedCuffMove()
    {
        if (cuff2 != null)
            cuff2.transform.position = cuff2LeftHandArrivedWorldPos;
    }

    public void DespawnRound(float fadeSeconds = 0.05f)
    {
        if (seqJob != null)
        {
            StopCoroutine(seqJob);
            seqJob = null;
        }

        curState = State.Idle;

        if (leftHand != null) leftHand.Despawn(fadeSeconds);
        if (rightHand != null) rightHand.Despawn(fadeSeconds);

        if (cuffs != null)
        {
            foreach (var c in cuffs)
            {
                if (c == null) continue;
                c.Despawn(fadeSeconds);
            }
        }
    }
}