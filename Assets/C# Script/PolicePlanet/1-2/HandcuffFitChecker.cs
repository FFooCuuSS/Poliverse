using System.Collections.Generic;
using UnityEngine;

public class HandcuffFitChecker : MonoBehaviour
{
    [Header("Refs")]
    public Minigame_1_2 minigame;

    [SerializeField] private List<CircleCollider2D> handColliders;

    private CircleCollider2D cuffCollider;
    private Vector3 startPos;

    private bool isSnapped;
    private CircleCollider2D snappedHand;

    private void Awake()
    {
        cuffCollider = GetComponent<CircleCollider2D>();
        startPos = transform.position;
    }

    public bool IsSnapped => isSnapped;
    public CircleCollider2D SnappedHand => snappedHand;

    public void ResetForRound()
    {
        isSnapped = false;
        snappedHand = null;

        transform.position = startPos;

        if (cuffCollider != null) cuffCollider.enabled = true;
    }

    public void Despawn(float delay = 0.05f)
    {
        if (delay <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }
        Invoke(nameof(Deactivate), delay);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (minigame == null) return;
        if (!minigame.IsInputWindowOpen) return;
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

                if (cuffCollider != null) cuffCollider.enabled = false;

                minigame.TryResolveRound();
                break;
            }
        }
    }
}