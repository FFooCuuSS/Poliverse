using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

public class RhythmManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    public enum ChartLoadMode { TextAsset, Addressables }

    [Header("Chart Source")]
    public ChartLoadMode loadMode = ChartLoadMode.TextAsset;

    [Header("TextAsset (Test)")]
    public TextAsset chartFile; // CSV

    [Header("Addressables")]
    public string addressablesKey; // 예: "1_PoliceNote" 또는 Addressables Address

    [Header("Minigame ID (Test input)")]
    public int idA = 1;
    public int idB = 1;
    private MiniGameBase currentMinigame;

    [Header("Chart Data")]
    public List<RhythmEvent> events = new List<RhythmEvent>();
    private int eventIndex = 0;

    [Header("Judgement Settings (seconds)")]
    public float perfectWindow = 0.03f;
    public float goodWindow = 0.07f;
    public float hitWindow = 0.12f;

    private double dspStartTime;

    public event Action<string> OnEventTriggered;
    public event Action<RhythmJudgement> OnPlayerJudged;

    public enum RhythmJudgement { Perfect, Good, Miss }

    [Serializable]
    public class RhythmEvent
    {
        public string action;
        public double time;
        public bool consumed;
    }

    public string CurrentMinigameId => $"{idA}-{idB}";

    private AsyncOperationHandle<TextAsset>? loadedHandle; // Addressables 핸들 보관



    private async void Start()
    {
        await LoadChartAsync(CurrentMinigameId);
        StartSong();
    }

    private void OnDestroy()
    {
        // Addressables로 로드했으면 해제 (메모리/핸들 누수 방지)
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
            throw new NullReferenceException("[RhythmManager] audioSource is NULL");

        TextAsset csv = null;

        if (loadMode == ChartLoadMode.TextAsset)
        {
            csv = chartFile;
        }
        else // Addressables
        {
            if (string.IsNullOrWhiteSpace(addressablesKey))
                throw new NullReferenceException("[RhythmManager] addressablesKey is EMPTY");

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

        Debug.Log($"[RhythmManager] Loaded {events.Count} notes for {minigameId}");
    }

    private void ParseCsv(string text, string minigameId)
    {
        string raw = text.Replace("\uFEFF", "");
        string[] lines = raw.Split('\n');

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length == 0) continue;

            // 헤더 스킵 (네가 원하면 "minigame" 기준 유지)
            if (parts[0].Trim().ToLower() == "minigame")
                continue;

            string id = parts[0].Trim();
            if (id != minigameId) continue;

            for (int i = 1; i < parts.Length; i++)
            {
                string p = parts[i].Trim();
                if (string.IsNullOrWhiteSpace(p)) continue;

                if (double.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out double t))
                {
                    events.Add(new RhythmEvent
                    {
                        action = id,
                        time = t,
                        consumed = false
                    });
                }
            }
        }

        events.Sort((a, b) => a.time.CompareTo(b.time));
        eventIndex = 0;
    }

    public void StartSong()
    {
        dspStartTime = AudioSettings.dspTime;
        audioSource.PlayScheduled(dspStartTime);
        eventIndex = 0;

        for (int i = 0; i < events.Count; i++)
            events[i].consumed = false;
    }

    private double SongTime => AudioSettings.dspTime - dspStartTime;

    void Update()
    {
        RunEventTimeline();
        DetectPlayerInput();
    }

    void RunEventTimeline()
    {
        while (eventIndex < events.Count && events[eventIndex].time <= SongTime)
        {
            OnEventTriggered?.Invoke(events[eventIndex].action);
            eventIndex++;
        }
    }

    void DetectPlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ReceivePlayerInput();
    }

    public void ReceivePlayerInput()
    {
        double now = SongTime;

        RhythmEvent nearest = null;
        double bestDelta = double.MaxValue;

        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];
            if (e.consumed) continue;

            double delta = Math.Abs(e.time - now);
            if (delta > hitWindow) continue;

            if (delta < bestDelta)
            {
                bestDelta = delta;
                nearest = e;
            }
        }

        RhythmJudgement judgement;
        if (nearest == null) judgement = RhythmJudgement.Miss;
        else if (bestDelta <= perfectWindow) judgement = RhythmJudgement.Perfect;
        else if (bestDelta <= goodWindow) judgement = RhythmJudgement.Good;
        else judgement = RhythmJudgement.Miss;

        if (nearest != null) nearest.consumed = true;
        OnPlayerJudged?.Invoke(judgement);
    }
}
