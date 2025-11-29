using UnityEngine;
using System;
using System.Collections.Generic;

public class RhythmManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

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

    public TextAsset chartFile; // CSV

    public enum RhythmJudgement { Perfect, Good, Miss }

    [Serializable]
    public class RhythmEvent
    {
        public string action;
        public double time;
    }

    // ---------------------------
    // CSV 로딩 (BOM 제거 포함)
    // ---------------------------
    public void LoadChart(string minigameId)
    {
        events.Clear();

        if (chartFile == null)
        {
            Debug.LogError("[RhythmManager] chartFile is NULL");
            return;
        }

        string raw = chartFile.text;
        raw = raw.Replace("\uFEFF", ""); // BOM 제거

        string[] lines = raw.Split('\n');

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');

            if (parts.Length == 0) continue;

            if (parts[0].Trim().ToLower() == "minigame")
                continue;

            string id = parts[0].Trim();

            if (id != minigameId)
                continue;

            for (int i = 1; i < parts.Length; i++)
            {
                string p = parts[i].Trim();
                if (string.IsNullOrWhiteSpace(p)) continue;

                if (double.TryParse(p, out double t))
                {
                    events.Add(new RhythmEvent
                    {
                        action = id,
                        time = t
                    });
                }
            }
        }

        events.Sort((a, b) => a.time.CompareTo(b.time));

        Debug.Log($"[RhythmManager] Loaded {events.Count} notes for {minigameId}");
    }


    // ---------------------------
    public void StartSong()
    {
        dspStartTime = AudioSettings.dspTime;
        audioSource.PlayScheduled(dspStartTime);
        eventIndex = 0;
    }

    private double SongTime => AudioSettings.dspTime - dspStartTime;

    // ---------------------------
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
        {
            ReceivePlayerInput();
        }
    }

    public void ReceivePlayerInput()
    {
        RhythmEvent nearest = null;
        double bestDelta = 999;

        foreach (var e in events)
        {
            double delta = Math.Abs(e.time - SongTime);
            if (delta < bestDelta)
            {
                bestDelta = delta;
                nearest = e;
            }
        }

        RhythmJudgement judgement =
            bestDelta <= perfectWindow ? RhythmJudgement.Perfect :
            bestDelta <= goodWindow ? RhythmJudgement.Good :
            bestDelta <= hitWindow ? RhythmJudgement.Miss :
                                       RhythmJudgement.Miss;

        OnPlayerJudged?.Invoke(judgement);
    }
}
