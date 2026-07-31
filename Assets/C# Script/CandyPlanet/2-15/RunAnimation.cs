using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerRunAnimation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RhythmManager rhythm;
    [SerializeField] private RhythmManagerTest test;

    [Header("2프레임 스프라이트")]
    [SerializeField] private Sprite frame1;
    [SerializeField] private Sprite frame2;

    [Header("Beat Sync")]
    [Tooltip("한 박의 길이(초). RhythmAnimation의 roundTripTime과 같은 값으로 맞추면 일관된 템포가 됨")]
    [SerializeField] private float beatInterval = 0.5f;
    [Tooltip("몇 박마다 프레임을 바꿀지 (1 = 매 박, 0.5 = 반박마다)")]
    [SerializeField] private float beatsPerSwap = 1f;

    [Header("Options")]
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Fallback (리듬 매니저가 없을 때)")]
    [SerializeField] private bool fallbackAutoSwap = true;
    [SerializeField] private float fallbackPeriod = 0.25f;
    private float fallbackTimer = 0f;

    private SpriteRenderer sr;
    private bool showingFrame1 = true;
    private int lastBeatIndex = int.MinValue;
    private bool isPlaying;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (rhythm == null) rhythm = FindObjectOfType<RhythmManager>();
        if (test == null) test = FindObjectOfType<RhythmManagerTest>();
    }

    private void OnEnable()
    {
        if (!playOnEnable) return;
        isPlaying = true;

        // Enable 순간엔 "지금 박"으로 바로 맞춰 들어가게 리셋
        lastBeatIndex = int.MinValue;
        fallbackTimer = 0f;
    }

    private void OnDisable()
    {
        isPlaying = false;
        if (sr != null) sr.sprite = frame1;
    }

    private void Update()
    {
        if (!isPlaying) return;

        if (rhythm == null && test == null)
        {
            if (!fallbackAutoSwap) return;

            float T = Mathf.Max(0.02f, fallbackPeriod);
            fallbackTimer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (fallbackTimer >= T)
            {
                fallbackTimer -= T;
                SwapFrame();
            }
            return;
        }

        bool running;
        double songTime;

        if (rhythm != null) { running = rhythm.IsRunning; songTime = rhythm.SongTime; }
        else if (test != null) { running = test.IsRunning; songTime = test.SongTimePublic; }
        else return;

        if (!running) return;

        float swapInterval = Mathf.Max(0.02f, beatInterval * beatsPerSwap);
        int beatIndex = Mathf.FloorToInt((float)(songTime / swapInterval));

        if (beatIndex != lastBeatIndex)
        {
            lastBeatIndex = beatIndex;
            SwapFrame();
        }
    }

    private void SwapFrame()
    {
        showingFrame1 = !showingFrame1;
        sr.sprite = showingFrame1 ? frame1 : frame2;
    }
}