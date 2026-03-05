using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PlayerReachChecker : MonoBehaviour
{
    public GameObject stage_1_3;
    private Minigame_1_3 minigame_1_3;
    private MiniGameBase minigameBase;

    private CapsuleCollider2D playerCollider;
    private DragAndDrop dragAndDrop;

    private SpriteRenderer sr;

    [Header("Fail Sprite")]
    public Sprite PlayerFail;

    private Sprite originalSprite;

    // ===== Respawn Settings =====
    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 0.25f; // 실패 연출 후 추가 텀
    [SerializeField] private Vector3 respawnStartPos = new Vector3(-9f, -1.4f, 0f);
    [SerializeField] private Vector3 respawnEndPos = new Vector3(-7.42f, -1.4f, 0f);
    [SerializeField] private float respawnMoveTime = 0.5f;
    [SerializeField] private float invincibleTime = 1.0f;

    [Header("Blink")]
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private float blinkMinAlpha = 0.25f;

    // ===== Fail Animation (old) =====
    [Header("Fail Animation")]
    [SerializeField] private float failDuration = 0.25f;
    [SerializeField] private float failSpinZ = -30f;
    [SerializeField] private float failDropY = 0.10f;
    [SerializeField] private float failScaleDownTo = 0.0f; // 0이면 완전 축소

    private bool isRespawning = false;
    private float invincibleUntil = 0f;

    private Tween respawnTween;
    private Tween blinkTween;
    private Tween failTween;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalSprite = sr.sprite;
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
        if (isRespawning) return;
        if (Time.time < invincibleUntil) return;

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

        if (!isOnPath)
        {
            OnFail();
        }
    }

    private void OnFail()
    {
        if (isRespawning) return;

        // 실패 스프라이트로 교체
        if (PlayerFail != null) sr.sprite = PlayerFail;

        // 트윈 정리
        transform.DOKill(true);
        respawnTween?.Kill(true);
        blinkTween?.Kill(true);
        failTween?.Kill(true);

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        if (dragAndDrop != null) dragAndDrop.enabled = false;

        yield return PlayFailAnimationAndWait();

        if (respawnDelay > 0f)
            yield return new WaitForSeconds(respawnDelay);

        StopBlink();
        SetAlpha(0f);

        transform.DOKill(true);
        respawnTween?.Kill(true);

        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // (중요) 위치 세팅도 여기서
        transform.position = respawnStartPos;

        if (originalSprite != null) sr.sprite = originalSprite;

        SetAlpha(1f);

        // 2) 슬라이드 인
        respawnTween = transform.DOMove(respawnEndPos, respawnMoveTime)
            .SetEase(Ease.OutCubic);

        yield return respawnTween.WaitForCompletion();

        invincibleUntil = Time.time + invincibleTime;
        StartBlink(invincibleTime);

        yield return new WaitForSeconds(invincibleTime);

        StopBlink();
        if (dragAndDrop != null) dragAndDrop.enabled = true;

        isRespawning = false;
    }

    // =========================
    // 실패 연출 (예전 연출 복구)
    // =========================
    private IEnumerator PlayFailAnimationAndWait()
    {
        // 실패 연출은 "현재 위치 기준"으로 진행
        Vector3 fixedPosition = transform.position;
        Vector3 startScale = transform.localScale;

        // 혹시 이전 깜빡임이 남아있으면 정리
        StopBlink();
        SetAlpha(1f);

        // 스케일을 줄였다가 리스폰에서 Vector3.one으로 복구하므로,
        // 여기선 마음껏 줄여도 됨.
        float targetScale = Mathf.Max(0f, failScaleDownTo);

        failTween = DOTween.Sequence()
            .Join(transform.DORotate(
                    new Vector3(0f, 0f, transform.eulerAngles.z + failSpinZ),
                    failDuration,
                    RotateMode.FastBeyond360
                ).SetEase(Ease.OutCubic))
            .Join(transform.DOScale(Vector3.one * targetScale, failDuration)
                .SetEase(Ease.InBack, 1.5f))
            .Join(transform.DOMoveY(fixedPosition.y - failDropY, failDuration)
                .SetEase(Ease.OutQuad));

        yield return failTween.WaitForCompletion();

        // 여기서 오브젝트를 끄지 않는다!
        // (리스폰에서 위치/스케일/스프라이트를 다시 세팅함)
    }

    private void StartBlink(float duration)
    {
        SetAlpha(1f);

        int loops = Mathf.CeilToInt(duration / (blinkInterval * 2f)) * 2;

        blinkTween = sr.DOFade(blinkMinAlpha, blinkInterval)
            .SetEase(Ease.InOutSine)
            .SetLoops(loops, LoopType.Yoyo)
            .OnKill(() => SetAlpha(1f))
            .OnComplete(() => SetAlpha(1f));
    }

    private void StopBlink()
    {
        blinkTween?.Kill(true);
        SetAlpha(1f);
    }

    private void SetAlpha(float a)
    {
        var c = sr.color;
        c.a = a;
        sr.color = c;
    }
}