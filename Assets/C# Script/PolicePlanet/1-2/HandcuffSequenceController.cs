using System.Collections;
using UnityEngine;

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

    public enum State { Idle, LeftMoving, RightMoving, PlayerDrag }
    public State curState { get; private set; } = State.Idle;

    private Coroutine seqJob;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SpawnRound()
    {
        if (seqJob != null)
        {
            StopCoroutine(seqJob);
            seqJob = null;
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
                c.gameObject.SetActive(true);
            }
        }
    }

    public void StartRoundSequence()
    {
        if (seqJob != null) StopCoroutine(seqJob);
        seqJob = StartCoroutine(SequenceCo());
    }

    private IEnumerator SequenceCo()
    {
        curState = State.LeftMoving;

        if (leftHand != null)
            leftHand.StartMove();

        float attachDelay = 0f;
        if (leftHand != null)
            attachDelay = Mathf.Max(0f, leftHand.totalTravelTime - 0.1f);

        yield return new WaitForSeconds(attachDelay);

        if (cuff2 != null)
            cuff2.transform.position = cuff2LeftHandArrivedWorldPos;

        if (chainGenerator != null)
            chainGenerator.isLeftCuffLocked = true;

        while (leftHand != null && !leftHand.hasArrived)
            yield return null;

        if (leftHandCollider != null)
            leftHandCollider.enabled = false;

        yield return new WaitForSeconds(delayBetweenHands);

        curState = State.RightMoving;

        if (rightHand != null)
            rightHand.StartMove();

        while (rightHand != null && !rightHand.hasArrived)
            yield return null;

        if (rightHandCollider != null)
            rightHandCollider.enabled = true;

        curState = State.PlayerDrag;
        seqJob = null;
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