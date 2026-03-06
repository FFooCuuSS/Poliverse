using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static MiniGameBase;

public class RhythmManagerTest : MonoBehaviour, MiniGameBase.IRhythmManager
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Minigame Target (Test)")]
    public MiniGameBase currentMinigame;

    public enum ChartLoadMode { TextAsset, Addressables }

    [Header("Chart Source")]
    public ChartLoadMode loadMode = ChartLoadMode.TextAsset;

    [Header("TextAsset (Test)")]
    public TextAsset chartFile;

    [Header("Addressables")]
    public string addressablesKey;

    [Header("Minigame ID (Test input)")]
    public int idA = 1;
    public int idB = 1;

    [Header("Judgement Settings (seconds)")]
    public float perfectWindow = 0.03f;
    public float goodWindow = 0.07f;
    public float hitWindow = 0.12f;

    [Serializable]
    public class RhythmEvent
    {
        public string action;   // 노트/액션 이름 (없으면 minigameId로 통일해도 됨)
        public string type;
        public double time;
        public bool consumed;
    }

    [Header("Chart Data")]
    public List<RhythmEvent> events = new List<RhythmEvent>();
    private int eventIndex = 0;

    private double dspStartTime;

    public bool IsRunning => isRunning;
    public double SongTimePublic => AudioSettings.dspTime - dspStartTime; // 외부 노출용
    private bool isRunning;
    public event Action<double> OnSongStarted;
    public event Action OnSongStopped;

    // IRhythmManager 구현 이벤트 (MiniGameBase가 구독)
    public event Action<string> OnEventTriggered;
    public event Action<MiniGameBase.JudgementResult> OnPlayerJudged;

    public string CurrentMinigameId => $"{idA}-{idB}";
    private AsyncOperationHandle<TextAsset>? loadedHandle;

    private async void Start()
    {
        await LoadChartAsync(CurrentMinigameId);

        // 테스트: currentMinigame이 있으면 바인딩
        if (currentMinigame != null)
        {
            currentMinigame.BindRhythmManager(this);

            ApplyWindowsFromMinigame();
        }
        else
            Debug.LogWarning("[RhythmManagerTest] currentMinigame is NULL. 이벤트만 발행됩니다.");

        StartSong();
    }

    private void ApplyWindowsFromMinigame()
    {
        if (currentMinigame == null) return;

        perfectWindow = currentMinigame.perfectWindowOverride;
        goodWindow = currentMinigame.goodWindowOverride;
        hitWindow = currentMinigame.hitWindowOverride;

        Debug.Log($"[RhythmManagerTest] Override windows from {currentMinigame.name} " +
                  $"(Perfect={perfectWindow}, Good={goodWindow}, Hit={hitWindow})");
    }


    private void OnDestroy()
    {
        if (currentMinigame != null)
            currentMinigame.BindRhythmManager(null);

        if (loadedHandle.HasValue)
        {
            Addressables.Release(loadedHandle.Value);
            loadedHandle = null;
        }
    }

    public async Task LoadChartAsync(string minigameId)
    {
        events.Clear();

        if (audioSource == null)
            throw new NullReferenceException("[RhythmManagerTest] audioSource is NULL");

        TextAsset csv = null;

        if (loadMode == ChartLoadMode.TextAsset)
        {
            csv = chartFile;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(addressablesKey))
                throw new NullReferenceException("[RhythmManagerTest] addressablesKey is EMPTY");

            var handle = Addressables.LoadAssetAsync<TextAsset>(addressablesKey);
            loadedHandle = handle;

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
                throw new Exception($"[RhythmManagerTest] Failed to load Addressables CSV: {addressablesKey}");

            csv = handle.Result;
        }

        if (csv == null)
            throw new NullReferenceException("[RhythmManagerTest] CSV asset is NULL");

        ParseCsv(csv.text, minigameId);

        Debug.Log($"[RhythmManagerTest] Loaded {events.Count} notes for {minigameId}");
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

            // 1) 시간 줄
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

            // 2) 타입 줄
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

        // 3) 조립
        if (times == null)
            throw new Exception($"[RhythmManagerTest] No time row found for {minigameId}");

        // 타입줄이 없으면 기본값 처리(전부 Tap 등)
        if (types == null)
            types = new List<string>(new string[times.Count]);

        int count = Math.Min(times.Count, types.Count);

        for (int i = 0; i < count; i++)
        {
            if (!times[i].HasValue) continue;

            string type = types[i];
            if (string.IsNullOrWhiteSpace(type)) type = "Tap"; // 기본 타입

            events.Add(new RhythmEvent
            {
                time = times[i].Value,
                type = type,
                action = type,   // 여기 중요: action을 type으로 쓰면 입력 매칭이 쉬워짐
                consumed = false
            });
        }

        events.Sort((a, b) => a.time.CompareTo(b.time));
        eventIndex = 0;
    }

    private static bool IsShowType(string type)
    {
        return string.Equals(type, "Show", StringComparison.OrdinalIgnoreCase);
    }
    public void StartSong()
    {
        dspStartTime = AudioSettings.dspTime;

        if (audioSource != null && audioSource.clip != null)
            audioSource.PlayScheduled(dspStartTime);
        else
            Debug.LogWarning("[RhythmManagerTest] audioSource.clip is NULL. 타임라인만 진행됩니다.");

        eventIndex = 0;
        for (int i = 0; i < events.Count; i++)
            events[i].consumed = false;

        isRunning = true;
    OnSongStarted?.Invoke(dspStartTime);
    }

    private double SongTime => AudioSettings.dspTime - dspStartTime;

    private void Update()
    {
        RunEventTimeline();
        CheckMisses();

        // 테스트 입력
        if (Input.GetKeyDown(KeyCode.Space))
            ReceivePlayerInput(); // action 필요하면 ReceivePlayerInput("tap") 같은 식으로
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

    // 미니게임에서 호출하는 판정 진입점
    public void ReceivePlayerInput(string action = null)
    {
        double now = SongTime;

        RhythmEvent nearest = null;
        double bestDelta = double.MaxValue;

        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];
            if (e.consumed) continue;
            if (IsShowType(e.type)) continue;
            if (action != null && e.action != action) continue;

            // 액션 분리하고 싶으면 여기서 action 매칭 걸어라
            // if (action != null && e.action != action) continue;

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

    // 플레이어가 입력하지 않고 넘길 때
    void CheckMisses()
    {
        double now = SongTime;

        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];
            if (e.consumed) continue;
            if (IsShowType(e.type)) continue;
            if (now <= e.time + hitWindow) break;

            e.consumed = true;
            events[i] = e;

            OnPlayerJudged?.Invoke(MiniGameBase.JudgementResult.Miss);
            Debug.Log("CheckMiss");
        }
    }
}
