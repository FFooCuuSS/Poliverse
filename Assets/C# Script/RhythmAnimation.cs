using UnityEngine;
using DG.Tweening;

public class RhythmAnimation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RhythmManager rhythm;
    [SerializeField] private RhythmManagerTest test;

    [Header("Bob (Y Move)")]
    [SerializeField] private float moveY = 0.18f;
    [SerializeField] private float roundTripTime = 0.50f;  // 왕복 시간(Up+Down)

    [Header("Cartoon Feel")]
    [SerializeField] private float squashX = 0.06f;
    [SerializeField] private float squashY = 0.1f;
    [SerializeField] private float scaleDelay = 0.02f;

    [Header("Options")]
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Fallback (no rhythm managers)")]
    [SerializeField] private bool fallbackAutoBounce = true;
    [SerializeField] private float fallbackPeriod = 0.50f;   // 리듬 없을 때도 같은 템포로
    private float fallbackTimer = 0f;

    private Vector3 baseLocalPos;
    private Vector3 baseLocalScale;
    private Tween mainTween;

    // 동기화용
    private int lastBeatIndex = int.MinValue;
    private bool isPlaying;

    private void Awake()
    {
        baseLocalPos = transform.localPosition;
        baseLocalScale = transform.localScale;

        if (rhythm == null) rhythm = FindObjectOfType<RhythmManager>();
        if (test == null) test = FindObjectOfType<RhythmManagerTest>();
    }

    private void OnEnable()
    {
        if (!playOnEnable) return;
        isPlaying = true;

        // Enable 순간엔 "지금 박"으로 바로 맞춰 들어가게 lastBeatIndex 리셋
        lastBeatIndex = int.MinValue;

        fallbackTimer = 0f;
    }

    private void OnDisable()
    {
        isPlaying = false;
        Stop();
        transform.localPosition = baseLocalPos;
        transform.localScale = baseLocalScale;
    }

    private void Update()
    {
        if (!isPlaying) return;

        if (rhythm == null && test == null)
        {
            if (!fallbackAutoBounce) return;

            float T = Mathf.Max(0.02f, fallbackPeriod > 0f ? fallbackPeriod : roundTripTime);
            fallbackTimer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (fallbackTimer >= T)
            {
                fallbackTimer -= T;
                Restart();
            }
            return;
        }

        bool running;
        double songTime;

        if (rhythm != null) { running = rhythm.IsRunning; songTime = rhythm.SongTime; }
        else if (test != null) { running = test.IsRunning; songTime = test.SongTimePublic; }
        else return;

        if (!running) return;

        float TRhythm = Mathf.Max(0.02f, roundTripTime);
        int beatIndex = Mathf.FloorToInt((float)(songTime / TRhythm));

        if (beatIndex != lastBeatIndex)
        {
            lastBeatIndex = beatIndex;
            Restart();
        }
    }

    // =========================
    // 기존 DOTween 모션 (거의 그대로)
    // =========================

    public void Stop()
    {
        mainTween?.Kill(true);
        mainTween = null;
    }

    private void Restart()
    {
        Stop();

        float half = roundTripTime * 0.5f;

        Vector3 down = new Vector3(0f, -moveY, 0f);
        Vector3 up = new Vector3(0f, moveY, 0f);

        Vector3 squashScale = new Vector3(
            baseLocalScale.x * (1f + squashX),
            baseLocalScale.y * (1f - squashY),
            baseLocalScale.z
        );

        mainTween = DOTween.Sequence()
            .AppendInterval(scaleDelay)

            // Move: Blendable 유지
            .Append(transform.DOBlendableLocalMoveBy(down, half).SetEase(Ease.InOutSine))
            // Scale: 절대값으로 찍기 (누적/드리프트 제거)
            .Join(transform.DOScale(squashScale, half).SetEase(Ease.InQuad))

            .Append(transform.DOBlendableLocalMoveBy(up, half).SetEase(Ease.OutBack, 1.25f))
            .Join(transform.DOScale(baseLocalScale, half).SetEase(Ease.OutQuad))

            .SetUpdate(useUnscaledTime);
    }
}