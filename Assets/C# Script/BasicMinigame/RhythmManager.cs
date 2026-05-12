using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class RhythmManager : MonoBehaviour, MiniGameBase.IRhythmManager
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    public enum ChartLoadMode { TextAsset, Addressables }

    [Header("Chart Source")]
    public ChartLoadMode loadMode = ChartLoadMode.Addressables;

    [Header("TextAsset (Optional)")]
    public TextAsset chartFile;

    [Header("Addressables")]
    [Tooltip("전체 차트를 하나로 쓸 거면 key를 고정. 미니게임마다 다르면 ConfigureForMinigame에서 교체.")]
    public string addressablesKey;

    [Header("Judgement Settings (seconds)")]
    public float perfectWindow = 0.1f;
    public float goodWindow = 0.3f;
    public float hitWindow = 1f;

    public class RhythmEvent
    {
        public string action;   // 입력 매칭 키
        public string type;
        public double time;
        public bool consumed;
    }

    [Header("Runtime")]
    [SerializeField] private List<RhythmEvent> events = new List<RhythmEvent>();
    private int eventIndex = 0;
    private double dspStartTime;

    private double songTime => AudioSettings.dspTime - dspStartTime;
    public double DspStartTime => dspStartTime;
    public bool IsRunning => isRunning;
    public double SongTime => songTime;

    public event Action<double> OnSongStarted; // dspStartTime 전달
    public event Action OnSongStopped;

    private MiniGameBase currentMinigame;
    private string currentMinigameId;
    private bool isRunning;

    // IRhythmManager events
    public event Action<string> OnEventTriggered;
    public event Action<MiniGameBase.JudgementResult> OnPlayerJudged;

    private AsyncOperationHandle<TextAsset>? loadedHandle;


    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        UnbindCurrentMinigame();

        if (loadedHandle.HasValue)
        {
            Addressables.Release(loadedHandle.Value);
            loadedHandle = null;
        }
    }
    public int GetTotalNodeCount()
    {
        int count = 0;

        for (int i = 0; i < events.Count; i++)
        {
            if (IsJudgeType(events[i].type))
                count++;
        }

        return count;
    }
    private static bool IsCueType(string type)
    {
        return string.Equals(type, "Show", StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, "Move", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsJudgeType(string type)
    {
        return string.Equals(type, "Input", StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, "Tap", StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, "Hold", StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, "Swipe", StringComparison.OrdinalIgnoreCase);
    }

    // =========================
    // 외부(로드씬)에서 호출할 API
    // =========================

    /// <summary>
    /// 현재 미니게임을 교체(또는 초기 세팅)하고, 차트를 로드한 뒤 타임라인을 시작한다.
    /// </summary>
    public async Task ConfigureForMinigameAsync(
    MiniGameBase minigame,
    string minigameId,
    TextAsset csv
)
    {
        if (audioSource == null)
            throw new NullReferenceException("[RhythmManager] audioSource is NULL");

        UnbindCurrentMinigame();
        StopSongInternal();

        currentMinigame = minigame;
        currentMinigameId = minigameId;

        if (csv == null)
            throw new NullReferenceException("[RhythmManager] csv(TextAsset) is NULL");

        loadMode = ChartLoadMode.TextAsset;
        chartFile = csv;

        await LoadChartAsync(currentMinigameId);

        if (currentMinigame != null)
            currentMinigame.BindRhythmManager(this);

        ApplyWindowsFromMinigame();

        // StartSong();
    }

    public void RefreshWindowsFromCurrentMinigame()
    {
        ApplyWindowsFromMinigame();
    }

    private static bool IsShowType(string type)
    {
        return string.Equals(type, "Show", StringComparison.OrdinalIgnoreCase);
    }


    /// <summary>
    /// 현재 타임라인 중단 + 바인딩 해제(미니게임 Destroy 전에 호출 추천)
    /// </summary>
    public void ClearCurrent()
    {
        UnbindCurrentMinigame();
        StopSongInternal();
        currentMinigameId = null;
        events.Clear();
        eventIndex = 0;
    }

    // =========================
    // 핵심 기능(테스트 코드 이식)
    // =========================

    public async Task LoadChartAsync(string minigameId)
    {
        events.Clear();

        TextAsset csv = null;

        if (loadMode == ChartLoadMode.TextAsset)
        {
            csv = chartFile;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(addressablesKey))
                throw new NullReferenceException("[RhythmManager] addressablesKey is EMPTY");

            // 이전 handle 있으면 release
            if (loadedHandle.HasValue)
            {
                Addressables.Release(loadedHandle.Value);
                loadedHandle = null;
            }

            var handle = Addressables.LoadAssetAsync<TextAsset>(addressablesKey);
            loadedHandle = handle;

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
                throw new Exception($"[RhythmManager] Failed to load Addressables CSV: {addressablesKey}");

            csv = handle.Result;
        }

        if (csv == null)
            throw new NullReferenceException("[RhythmManager] CSV asset is NULL");

        ParseCsv(csv.text, minigameId);

        Debug.Log($"[RhythmManager] Loaded {events.Count} notes for {minigameId} (key={addressablesKey})");
    }

    private void ParseCsv(string text, string minigameId)
    {
        string raw = text.Replace("\uFEFF", "");
        string[] lines = raw.Split('\n');

        List<double?> times = null;
        List<string> types = null;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length == 0) continue;

            string first = parts[0].Trim();

            if (first.Equals("minigame", StringComparison.OrdinalIgnoreCase))
                continue;

            if (first == $"{minigameId}_time")
            {
                times = new List<double?>();
                for (int i = 1; i < parts.Length; i++)
                {
                    string p = parts[i].Trim();
                    if (string.IsNullOrWhiteSpace(p) || p == "-")
                    {
                        times.Add(null);
                        continue;
                    }

                    if (double.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out double t))
                        times.Add(t);
                    else
                        times.Add(null);
                }
            }

            if (first == $"{minigameId}_type")
            {
                types = new List<string>();
                for (int i = 1; i < parts.Length; i++)
                {
                    string p = parts[i].Trim();
                    if (string.IsNullOrWhiteSpace(p) || p == "-")
                    {
                        types.Add(null);
                        continue;
                    }
                    types.Add(p);
                }
            }
        }

        if (times == null)
            throw new Exception($"[RhythmManager] No time row found for {minigameId}");

        if (types == null)
            types = new List<string>(new string[times.Count]);

        int count = Math.Min(times.Count, types.Count);

        for (int i = 0; i < count; i++)
        {
            if (!times[i].HasValue) continue;

            string type = types[i];
            if (string.IsNullOrWhiteSpace(type)) type = "Tap";

            events.Add(new RhythmEvent
            {
                time = times[i].Value,
                type = type,
                action = type,
                consumed = false
            });
        }

        events.Sort((a, b) => a.time.CompareTo(b.time));
        eventIndex = 0;
    }

    public void StartSong()
    {
        dspStartTime = AudioSettings.dspTime;

        if (audioSource != null && audioSource.clip != null)
        {
            // 바로 Play()보다 scheduled가 안정적
            //audioSource.PlayScheduled(dspStartTime);
        }
        else
        {
            Debug.LogWarning("[RhythmManager] audioSource.clip is NULL. 타임라인만 진행됩니다.");
        }

        eventIndex = 0;
        for (int i = 0; i < events.Count; i++)
            events[i].consumed = false;

        isRunning = true;
        OnSongStarted?.Invoke(dspStartTime);
    }

    private void StopSongInternal()
    {
        isRunning = false;
        OnSongStopped?.Invoke();
    }

    private void Update()
    {
        if (!isRunning) return;

        RunEventTimeline();
        CheckMisses();
    }

    private void RunEventTimeline()
    {
        while (eventIndex < events.Count && events[eventIndex].time <= SongTime)
        {
            var e = events[eventIndex];
            OnEventTriggered?.Invoke(e.action);
            eventIndex++;
        }
    }

    private void ApplyWindowsFromMinigame()
    {
        if (currentMinigame == null) return;

        perfectWindow = currentMinigame.perfectWindowOverride;
        goodWindow = currentMinigame.goodWindowOverride;
        hitWindow = currentMinigame.hitWindowOverride;

        Debug.Log($"[RhythmManager] Override windows from {currentMinigame.name} " +
                  $"(Perfect={perfectWindow}, Good={goodWindow}, Hit={hitWindow})");
    }


    // 미니게임에서 호출
    public void ReceivePlayerInput(string action = null)
    {
        if (!isRunning) return;

        double now = SongTime;

        RhythmEvent nearest = null;
        double bestDelta = double.MaxValue;

        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];
            if (e.consumed) continue;
            if (!IsJudgeType(e.type)) continue;   // 핵심
            if (action != null && !string.Equals(e.action, action, StringComparison.OrdinalIgnoreCase)) continue;

            double delta = Math.Abs(e.time - now);
            if (delta > hitWindow) continue;

            if (delta < bestDelta)
            {
                bestDelta = delta;
                nearest = e;
            }
        }

        MiniGameBase.JudgementResult judgement;
        if (nearest == null) judgement = MiniGameBase.JudgementResult.Miss;
        else if (bestDelta <= perfectWindow) judgement = MiniGameBase.JudgementResult.Perfect;
        else if (bestDelta <= goodWindow) judgement = MiniGameBase.JudgementResult.Good;
        else judgement = MiniGameBase.JudgementResult.Miss;

        if (nearest != null) nearest.consumed = true;

        OnPlayerJudged?.Invoke(judgement);
    }

    private void CheckMisses()
    {
        double now = SongTime;

        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];
            if (e.consumed) continue;
            if (!IsJudgeType(e.type)) continue;   // 핵심
            if (now <= e.time + hitWindow) break;

            e.consumed = true;
            events[i] = e;

            OnPlayerJudged?.Invoke(MiniGameBase.JudgementResult.Miss);
            Debug.Log($"CheckMiss : {e.type} @ {e.time:F3}");
        }
    }

    private void UnbindCurrentMinigame()
    {
        if (currentMinigame != null)
            currentMinigame.BindRhythmManager(null);

        currentMinigame = null;
    }
}
