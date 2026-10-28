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
        prevAccept = false;
        preplaced = false;

        // 이전 라운드에서 손을 떼지 않고 들고 있던 드래그 상태 해제
        StopDragging();

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

        StopDragging();

        MoveOffscreen();
        despawnJob = null;
    }

    private void StopDragging()
    {
        DragAndDrop drag = GetComponent<DragAndDrop>();

        if (drag != null)
            drag.isDragging = false;
    }

    private void Update()
    {
        if (minigame == null) return;
        if (isSnapped) return;

        // 시범 중에는 플레이어 입력/충돌을 받지 않음
        if (minigame.IsDemoMode)
            return;

        CheckAndSnap();
    }

    // 입력창이 열린 순간 이미 손 위에 올려둔 상태였는지
    // (미리 올려두고 기다리면 콜라이더가 켜지는 순간 자동으로 Perfect가 나던 문제)
    private bool prevAccept;
    private bool preplaced;

    private void CheckAndSnap()
    {
        if (cuffCollider == null) return;
        if (minigame == null) return;

        // 입력창 밖(손 이동 중 / 창 닫힌 뒤)에는 스냅하지 않는다
        if (!minigame.CanAcceptSnap)
        {
            prevAccept = false;
            preplaced = false;
            return;
        }

        bool overlapping = TryFindOverlap(out CircleCollider2D hand);

        // 창이 열린 첫 프레임에 이미 겹쳐 있으면 "미리 올려둔 것"으로 표시
        if (!prevAccept)
        {
            prevAccept = true;
            preplaced = overlapping;
        }

        // 한 번 손에서 벗어나면 다시 정상 판정
        if (!overlapping)
            preplaced = false;

        // 새로 손에 닿았을 때만 스냅 (미리 올려둔 채로는 X → 놓거나 다시 갖다 대야 함)
        if (overlapping && !preplaced)
            Snap(hand);
    }

    // 드래그를 놓는 순간도 입력으로 인정 (미리 올려뒀다가 박자에 맞춰 놓기)
    private void OnMouseUp()
    {
        if (isSnapped) return;
        if (minigame == null || minigame.IsDemoMode) return;
        if (!minigame.CanAcceptSnap) return;

        if (TryFindOverlap(out CircleCollider2D hand))
            Snap(hand);
    }

    private bool TryFindOverlap(out CircleCollider2D hand)
    {
        hand = null;

        if (cuffCollider == null || !cuffCollider.enabled || handColliders == null)
            return false;

        foreach (var handcol in handColliders)
        {
            if (handcol == null || !handcol.enabled) continue;

            if (cuffCollider.bounds.Intersects(handcol.bounds))
            {
                hand = handcol;
                return true;
            }
        }

        return false;
    }

    private void Snap(CircleCollider2D handcol)
    {
        if (handcol == null || isSnapped) return;

        transform.position = handcol.bounds.center;
        isSnapped = true;
        snappedHand = handcol;
        preplaced = false;

        StopDragging();

        if (cuffCollider != null)
            cuffCollider.enabled = false;

        if (HandcuffSequenceController.Instance != null)
            HandcuffSequenceController.Instance.ApplyPlayerSnapVisual();

        minigame.TryResolveRound();
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