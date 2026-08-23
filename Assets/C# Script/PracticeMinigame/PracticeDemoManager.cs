using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PracticeDemoManager : MonoBehaviour
{
    private class GuideEvent
    {
        public double time;
        public string type;
        public int value;
    }

    private MiniGameBase currentMinigame;
    private RhythmManager rhythmManager;

    private readonly List<GuideEvent> guideEvents =
        new List<GuideEvent>();

    private Action<int> onGuideTextUnlock;
    private Coroutine guideCoroutine;

    private bool isRunning;
    private int actionIndex;

    public bool IsRunning => isRunning;

    public void Begin(
        MiniGameBase minigame,
        RhythmManager manager,
        TextAsset guideCsv,
        string minigameId,
        Action<int> guideTextUnlock = null)
    {
        Stop();

        if (minigame == null)
        {
            Debug.LogWarning(
                "[PracticeDemo] MiniGameBase가 없습니다."
            );

            return;
        }

        if (manager == null)
        {
            Debug.LogWarning(
                "[PracticeDemo] RhythmManager가 없습니다."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(minigameId))
        {
            Debug.LogWarning(
                "[PracticeDemo] 미니게임 ID가 없습니다."
            );

            return;
        }

        currentMinigame = minigame;
        rhythmManager = manager;
        onGuideTextUnlock = guideTextUnlock;

        actionIndex = 0;
        isRunning = true;

        LoadGuideEvents(
            guideCsv,
            minigameId
        );

        /*
         * RhythmChart 모드는 기존 Rhythm CSV의
         * Input / Tap / Hold / Swipe 타이밍을 그대로 사용한다.
         *
         * Custom 모드는 Guide CSV의 Action 행을 사용하므로
         * Rhythm 이벤트를 자동 행동으로 사용하지 않는다.
         */
        if (currentMinigame.GetPracticeTimingMode ==
            MiniGameBase.PracticeTimingMode.RhythmChart)
        {
            rhythmManager.OnEventTriggered +=
                HandleRhythmEvent;
        }

        /*
         * Guide CSV의 Custom Action / GuideText는
         * RhythmManager의 타임라인 시작 시점에 맞춰 실행한다.
         */
        rhythmManager.OnSongStarted +=
            HandleSongStarted;

        Debug.Log(
            $"[PracticeDemo] 시작: " +
            $"{currentMinigame.name}, " +
            $"Timing={currentMinigame.GetPracticeTimingMode}, " +
            $"GuideEvents={guideEvents.Count}"
        );
    }

    public void Stop()
    {
        if (rhythmManager != null)
        {
            rhythmManager.OnEventTriggered -=
                HandleRhythmEvent;

            rhythmManager.OnSongStarted -=
                HandleSongStarted;
        }

        if (guideCoroutine != null)
        {
            StopCoroutine(guideCoroutine);
            guideCoroutine = null;
        }

        guideEvents.Clear();

        currentMinigame = null;
        rhythmManager = null;
        onGuideTextUnlock = null;

        actionIndex = 0;
        isRunning = false;
    }

    private void HandleRhythmEvent(
        string action)
    {
        if (!isRunning)
            return;

        if (currentMinigame == null)
            return;

        if (currentMinigame.GetPracticeTimingMode !=
            MiniGameBase.PracticeTimingMode.RhythmChart)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(action))
            return;

        action = action.Trim();

        if (!IsPracticeActionType(action))
            return;

        int currentActionIndex =
            actionIndex;

        actionIndex++;

        Debug.Log(
            $"[PracticeDemo] Rhythm Action " +
            $"#{currentActionIndex} Type={action}"
        );

        currentMinigame.ExecutePracticeAction(
            currentActionIndex,
            action
        );
    }

    private void HandleSongStarted(
        double dspStartTime)
    {
        if (!isRunning)
            return;

        if (guideCoroutine != null)
        {
            StopCoroutine(guideCoroutine);
            guideCoroutine = null;
        }

        if (guideEvents.Count == 0)
            return;

        guideCoroutine =
            StartCoroutine(RunGuideTimeline());
    }

    private IEnumerator RunGuideTimeline()
    {
        for (int i = 0;
             i < guideEvents.Count;
             i++)
        {
            GuideEvent guideEvent =
                guideEvents[i];

            while (isRunning &&
                   rhythmManager != null &&
                   rhythmManager.IsRunning &&
                   rhythmManager.SongTime < guideEvent.time)
            {
                yield return null;
            }

            if (!isRunning ||
                rhythmManager == null ||
                currentMinigame == null)
            {
                yield break;
            }

            if (!rhythmManager.IsRunning)
                yield break;

            ExecuteGuideEvent(guideEvent);
        }

        guideCoroutine = null;
    }

    private void ExecuteGuideEvent(
        GuideEvent guideEvent)
    {
        if (guideEvent == null)
            return;

        if (string.Equals(
                guideEvent.type,
                "Action",
                StringComparison.OrdinalIgnoreCase))
        {
            /*
             * Action은 Custom 미니게임에서만 자동 행동으로 사용한다.
             * RhythmChart 미니게임은 기존 Rhythm CSV 입력을 사용한다.
             */
            if (currentMinigame.GetPracticeTimingMode !=
                MiniGameBase.PracticeTimingMode.Custom)
            {
                return;
            }

            Debug.Log(
                $"[PracticeDemo] Custom Action " +
                $"#{guideEvent.value} " +
                $"Time={guideEvent.time:0.###}"
            );

            currentMinigame.ExecutePracticeAction(
                guideEvent.value,
                "Action"
            );

            return;
        }

        if (string.Equals(
                guideEvent.type,
                "GuideText",
                StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log(
                $"[PracticeDemo] GuideText " +
                $"#{guideEvent.value} " +
                $"Time={guideEvent.time:0.###}"
            );

            onGuideTextUnlock?.Invoke(
                guideEvent.value
            );
        }
    }

    private void LoadGuideEvents(
        TextAsset guideCsv,
        string minigameId)
    {
        guideEvents.Clear();

        if (guideCsv == null)
        {
            if (currentMinigame != null &&
                currentMinigame.GetPracticeTimingMode ==
                MiniGameBase.PracticeTimingMode.Custom)
            {
                Debug.LogWarning(
                    "[PracticeDemo] Custom 미니게임인데 " +
                    "Practice Guide CSV가 없습니다."
                );
            }

            return;
        }

        string raw =
            guideCsv.text.Replace("\uFEFF", "");

        string[] lines =
            raw.Split('\n');

        int nextAutomaticActionIndex = 0;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("#"))
                continue;

            string[] parts =
                line.Split(',');

            if (parts.Length < 3)
                continue;

            string id =
                parts[0].Trim();

            if (string.Equals(
                    id,
                    "minigame",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.Equals(
                    id,
                    minigameId,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!double.TryParse(
                    parts[1].Trim(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double time))
            {
                Debug.LogWarning(
                    $"[PracticeDemo] 잘못된 시간: {line}"
                );

                continue;
            }

            string type =
                parts[2].Trim();

            if (string.IsNullOrWhiteSpace(type))
                continue;

            int value = -1;

            if (parts.Length >= 4 &&
                int.TryParse(
                    parts[3].Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int parsedValue))
            {
                value = parsedValue;
            }

            if (string.Equals(
                    type,
                    "Action",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (value < 0)
                {
                    value =
                        nextAutomaticActionIndex;
                }

                nextAutomaticActionIndex =
                    Mathf.Max(
                        nextAutomaticActionIndex,
                        value + 1
                    );
            }
            else if (string.Equals(
                         type,
                         "GuideText",
                         StringComparison.OrdinalIgnoreCase))
            {
                if (value < 0)
                {
                    Debug.LogWarning(
                        "[PracticeDemo] GuideText에는 " +
                        $"설명 인덱스가 필요합니다: {line}"
                    );

                    continue;
                }
            }
            else
            {
                // 현재 Guide CSV에서 지원하는 이벤트는
                // Action / GuideText 두 종류다.
                continue;
            }

            guideEvents.Add(
                new GuideEvent
                {
                    time = Math.Max(0.0, time),
                    type = type,
                    value = value
                }
            );
        }

        guideEvents.Sort(
            (a, b) =>
                a.time.CompareTo(b.time)
        );
    }

    private static bool IsPracticeActionType(
        string action)
    {
        return
            string.Equals(
                action,
                "Input",
                StringComparison.OrdinalIgnoreCase
            )
            ||
            string.Equals(
                action,
                "Tap",
                StringComparison.OrdinalIgnoreCase
            )
            ||
            string.Equals(
                action,
                "Hold",
                StringComparison.OrdinalIgnoreCase
            )
            ||
            string.Equals(
                action,
                "Swipe",
                StringComparison.OrdinalIgnoreCase
            );
    }

    private void OnDisable()
    {
        Stop();
    }

    private void OnDestroy()
    {
        Stop();
    }
}