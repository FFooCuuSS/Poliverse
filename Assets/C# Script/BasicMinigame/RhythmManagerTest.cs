using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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

    [Header("Test Control")]
    [SerializeField] private bool autoStartOnPlay = true;
    [SerializeField] private KeyCode inputKey = KeyCode.Space;
    [SerializeField] private KeyCode finalizeKey = KeyCode.Return;
    [SerializeField] private bool printDebug = true;

    [Serializable]
    public class RhythmEvent
    {
        public string action;
        public string type;
        public double time;
        public bool consumed;
    }

    [Header("Chart Data")]
    public List<RhythmEvent> events = new List<RhythmEvent>();

    private int eventIndex = 0;
    private double dspStartTime;
    private bool isRunning;

    public bool IsRunning => isRunning;
    public double SongTime => AudioSettings.dspTime - dspStartTime;
    public double SongTimePublic => SongTime;
    public string CurrentMinigameId => $"{idA}-{idB}";

    public event Action<double> OnSongStarted;
    public event Action OnSongStopped;

    public event Action<string> OnEventTriggered;
    public event Action<MiniGameBase.JudgementResult> OnPlayerJudged;

    private AsyncOperationHandle<TextAsset>? loadedHandle;

    private async void Start()
    {
        if (!autoStartOnPlay) return;

        await SetupAndStartTest();
    }

    public async Task SetupAndStartTest()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        await LoadChartAsync(CurrentMinigameId);

        if (currentMinigame != null)
        {
            currentMinigame.BindRhythmManager(this);
            ApplyWindowsFromMinigame();

            // 중요:
            // MiniGameBase.StartGame() 안에서 GetTotalNodeCount()를 읽는 구조라면
            // 반드시 차트 로드 + 바인딩 이후에 호출해야 함.
            currentMinigame.StartGame();
        }
        else
        {
            Debug.LogWarning("[RhythmManagerTest] currentMinigame is NULL. 이벤트만 발행됩니다.");
        }

        StartSong();
    }

    private void Update()
    {
        if (!isRunning) return;

        RunEventTimeline();
        CheckMisses();

        if (Input.GetKeyDown(inputKey))
        {
            ReceivePlayerInput();
        }

        // 테스트용 수동 종료/점수 확정
        if (Input.GetKeyDown(finalizeKey))
        {
            FinalizeCurrentTest();
        }
    }

    public void StartSong()
    {
        dspStartTime = AudioSettings.dspTime;

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayScheduled(dspStartTime);
        }
        else
        {
            Debug.LogWarning("[RhythmManagerTest] audioSource or clip is NULL. 타임라인만 진행됩니다.");
        }

        eventIndex = 0;

        for (int i = 0; i < events.Count; i++)
            events[i].consumed = false;

        isRunning = true;
        OnSongStarted?.Invoke(dspStartTime);

        if (printDebug)
            Debug.Log($"[RhythmManagerTest] StartSong. ID={CurrentMinigameId}, TotalJudgeNodes={GetTotalNodeCount()}");
    }

    public void StopSong()
    {
        StopSongInternal();
    }

    private void StopSongInternal()
    {
        if (!isRunning) return;

        isRunning = false;
        OnSongStopped?.Invoke();

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    public void ClearCurrent()
    {
        StopSongInternal();

        if (currentMinigame != null)
            currentMinigame.BindRhythmManager(null);

        eventIndex = 0;
        events.Clear();
    }

    private void FinalizeCurrentTest()
    {
        StopSongInternal();

        if (currentMinigame != null)
        {
            var result = currentMinigame.FinalizeScoreSession();

            Debug.Log(
                $"[RhythmManagerTest Finalize]\n" +
                $"- Minigame    : {currentMinigame.name}\n" +
                $"- Total Nodes : {result.totalNode}\n" +
                $"- Perfect     : {result.perfect}\n" +
                $"- Good        : {result.good}\n" +
                $"- Miss        : {result.miss}"
            );
        }
        else
        {
            Debug.LogWarning("[RhythmManagerTest] Finalize failed. currentMinigame is NULL.");
        }
    }

    private void ApplyWindowsFromMinigame()
    {
        if (currentMinigame == null) return;

        perfectWindow = currentMinigame.perfectWindowOverride;
        goodWindow = currentMinigame.goodWindowOverride;
        hitWindow = currentMinigame.hitWindowOverride;

        if (printDebug)
        {
            Debug.Log(
                $"[RhythmManagerTest] Override windows from {currentMinigame.name} " +
                $"(Perfect={perfectWindow}, Good={goodWindow}, Hit={hitWindow})"
            );
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

    public async Task LoadChartAsync(string minigameId)
    {
        events.Clear();
        eventIndex = 0;

        TextAsset csv = null;

        if (loadMode == ChartLoadMode.TextAsset)
        {
            csv = chartFile;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(addressablesKey))
                throw new NullReferenceException("[RhythmManagerTest] addressablesKey is EMPTY");

            if (loadedHandle.HasValue)
            {
                Addressables.Release(loadedHandle.Value);
                loadedHandle = null;
            }

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

        if (printDebug)
        {
            Debug.Log(
                $"[RhythmManagerTest] Loaded {events.Count} events for {minigameId}. " +
                $"JudgeNodes={GetTotalNodeCount()}"
            );
        }
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
            throw new Exception($"[RhythmManagerTest] No time row found for {minigameId}");

        if (types == null)
            types = new List<string>(new string[times.Count]);

        int count = Math.Min(times.Count, types.Count);

        for (int i = 0; i < count; i++)
        {
            if (!times[i].HasValue) continue;

            string type = types[i];

            if (string.IsNullOrWhiteSpace(type))
                type = "Tap";

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

    private void RunEventTimeline()
    {
        while (eventIndex < events.Count && events[eventIndex].time <= SongTime)
        {
            var e = events[eventIndex];

            OnEventTriggered?.Invoke(e.action);

            if (printDebug)
                Debug.Log($"[RhythmManagerTest] Event Triggered: {e.action} @ {e.time:F3}");

            eventIndex++;
        }
    }

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
            if (!IsJudgeType(e.type)) continue;

            if (action != null &&
                !string.Equals(e.action, action, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            double delta = Math.Abs(e.time - now);
            if (delta > hitWindow) continue;

            if (delta < bestDelta)
            {
                bestDelta = delta;
                nearest = e;
            }
        }

        MiniGameBase.JudgementResult judgement;

        if (nearest == null)
        {
            judgement = MiniGameBase.JudgementResult.Miss;
        }
        else if (bestDelta <= perfectWindow)
        {
            judgement = MiniGameBase.JudgementResult.Perfect;
        }
        else if (bestDelta <= goodWindow)
        {
            judgement = MiniGameBase.JudgementResult.Good;
        }
        else
        {
            judgement = MiniGameBase.JudgementResult.Miss;
        }

        if (nearest != null)
            nearest.consumed = true;

        OnPlayerJudged?.Invoke(judgement);

        if (printDebug)
        {
            string noteInfo = nearest == null ? "No Note" : $"{nearest.type} @ {nearest.time:F3}";
            Debug.Log($"[RhythmManagerTest] Input Judged: {judgement} / {noteInfo}");
        }
    }

    private void CheckMisses()
    {
        double now = SongTime;

        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];

            if (e.consumed) continue;
            if (!IsJudgeType(e.type)) continue;

            if (now <= e.time + hitWindow)
                break;

            e.consumed = true;
            events[i] = e;

            OnPlayerJudged?.Invoke(MiniGameBase.JudgementResult.Miss);

            if (printDebug)
                Debug.Log($"[RhythmManagerTest] Auto Miss: {e.type} @ {e.time:F3}");
        }
    }

    private void OnDestroy()
    {
        StopSongInternal();

        if (currentMinigame != null)
            currentMinigame.BindRhythmManager(null);

        if (loadedHandle.HasValue)
        {
            Addressables.Release(loadedHandle.Value);
            loadedHandle = null;
        }
    }
}