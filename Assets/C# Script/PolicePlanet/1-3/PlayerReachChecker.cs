using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerReachChecker : MonoBehaviour
{
    public GameObject stage_1_3;
    private Minigame_1_3 minigame_1_3;
    private MiniGameBase minigameBase;

    private CapsuleCollider2D playerCollider;
    private DragAndDrop dragAndDrop;

    SpriteRenderer sr;
    public Sprite PlayerFail;

    private bool isGameOver = false;
    private Vector3 fixedPosition;
    private Tween failTween;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        minigame_1_3 = stage_1_3.GetComponent<Minigame_1_3>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        dragAndDrop = GetComponent<DragAndDrop>();
        minigameBase = GetComponentInParent<MiniGameBase>();

        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }

    private void Update()
    {
        if (isGameOver)
        {
            transform.position = fixedPosition; // 위치 고정
            return;
        }

        BoundCheck();
    }

    private void BoundCheck()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            playerCollider.bounds.center,
            playerCollider.bounds.size,
            0f
        );

        bool isOnPath = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Path"))
            {
                isOnPath = true;
                break;
            }
        }

        if (!isOnPath && !isGameOver)
        {
            isGameOver = true;
            fixedPosition = transform.position;
            sr.sprite = PlayerFail;

            transform.DOKill();
            PlayFailAnimation();

            minigameBase.Fail();
        }
    }

    // =========================
    // 실패 연출
    // =========================
    private void PlayFailAnimation()
    {
        transform.position = fixedPosition;
        transform.DOKill(true);
        failTween?.Kill(true);

        float duration = 0.25f;
        float spinZ = -30f;
        float dropY = 0.10f;        // (선택) 아주 살짝만 내려가게

        Vector3 startScale = transform.localScale;

        failTween = DOTween.Sequence()
            .Join(transform.DORotate(
                    new Vector3(0f, 0f, transform.eulerAngles.z + spinZ),
                    duration,
                    RotateMode.FastBeyond360
                ).SetEase(Ease.OutCubic))

            .Join(transform.DOScale(Vector3.zero, duration)
                .SetEase(Ease.InBack, 1.5f))

            .Join(transform.DOMoveY(fixedPosition.y - dropY, duration)
                .SetEase(Ease.OutQuad))

            .OnComplete(() =>
            {
                gameObject.SetActive(false);

                // 재사용할 거면 스케일 원복 (비활성화 전에 해두면 안전)
                transform.localScale = startScale;
            });
    }
}
