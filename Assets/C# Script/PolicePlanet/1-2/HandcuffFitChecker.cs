using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandcuffFitChecker : MonoBehaviour
{
    [Header("Refs")]
    public Minigame_1_2 minigame;

    [SerializeField] private List<CircleCollider2D> handColliders;

    [Header("Hide")]
    [SerializeField] private Vector3 hiddenWorldPos = new Vector3(9999f, 9999f, 0f);

    private CircleCollider2D cuffCollider;
    private Vector3 startPos;

    private bool isSnapped;
    private CircleCollider2D snappedHand;

    private SpriteRenderer[] spriteRenderers;
    private Coroutine despawnJob;

    private void Awake()
    {
        cuffCollider = GetComponent<CircleCollider2D>();
        startPos = transform.position;
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    public bool IsSnapped => isSnapped;
    public CircleCollider2D SnappedHand => snappedHand;

    public void ResetForRound()
    {
        if (despawnJob != null)
        {
            StopCoroutine(despawnJob);
            despawnJob = null;
        }

        DOTween.Kill(this);
        DOTween.Kill(transform);

        isSnapped = false;
        snappedHand = null;

        transform.position = startPos;

        if (cuffCollider != null)
            cuffCollider.enabled = true;

        SetAlpha(1f);
    }

    public void Despawn(float delay = 0.05f)
    {
        if (despawnJob != null)
        {
            StopCoroutine(despawnJob);
            despawnJob = null;
        }

        despawnJob = StartCoroutine(DespawnCo(delay));
    }

    private IEnumerator DespawnCo(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (cuffCollider != null)
            cuffCollider.enabled = false;

        MoveOffscreen();
        despawnJob = null;
    }

    private void Update()
    {
        if (minigame == null) return;
        if (isSnapped) return;

        CheckAndSnap();
    }

    private void CheckAndSnap()
    {
        if (cuffCollider == null) return;

        foreach (var handcol in handColliders)
        {
            if (handcol == null || !handcol.enabled) continue;

            if (cuffCollider.bounds.Intersects(handcol.bounds))
            {
                transform.position = handcol.bounds.center;
                isSnapped = true;
                snappedHand = handcol;

                if (cuffCollider != null)
                    cuffCollider.enabled = false;

                minigame.TryResolveRound();
                break;
            }
        }
    }

    private void MoveOffscreen()
    {
        transform.position = hiddenWorldPos;
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderers == null) return;

        foreach (var sr in spriteRenderers)
        {
            if (sr == null) continue;

            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}