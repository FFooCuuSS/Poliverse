using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PatternSlot3_9
{
    [Header("해당 칸이 빛날 때 켜지는 오브젝트")]
    public GameObject onBox;

    [Header("해당 칸의 실제 클릭 대상")]
    public GameObject pattern;
}

// 3-9 진행 (모든 시각은 노래 시간 기준)
//  1) 표시: startDelay부터 (on+off) 간격으로 정답 칸이 하나씩 켜짐
//  2) 입력: 입력 시작부터 inputInterval 간격으로 6박. 박마다 모든 칸이 빨갛게 켜짐(신호)
//           → 그 박에 '이번 순서의 칸'을 누른다.
// 판정(박마다 1개, 총 6개):
//  - 박 ±goodWindow 안에 맞는 칸 → 성공 (±perfectWindow면 Perfect)
//  - 박 ±goodWindow 안에 틀린 칸 → 실패
//  - 박 + goodWindow까지 안 누름 → 실패
//  - 박 범위 밖 클릭은 무시
public class Manager3_9 : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;
    [Header("효과음")]
    [SerializeField] private AudioClip showSound;
    [Header("효과음")]
    [SerializeField] private AudioClip wrongSound;

    [Header("1~6번 패턴")]
    public PatternSlot3_9[] slots = new PatternSlot3_9[6];

    [Header("미니게임 판정")]
    public Minigame_3_9_remake minigame;

    [Header("게임 시작 대기 시간 (첫 패턴 표시 시각)")]
    public float startDelay = 1.8f;

    [Header("패턴 ON 시간")]
    public float patternOnTime = 0.6f;

    [Header("패턴 OFF 시간")]
    public float patternOffTime = 0.6f;

    [Header("패턴 표시 후 입력까지 대기 시간")]
    public float inputStartDelay = 1.8f;

    [Header("입력 박자")]
    [Tooltip("입력 박 간격 (초)")]
    [SerializeField] private float inputInterval = 1.2f;
    [Tooltip("박마다 모든 칸이 빨갛게 켜져 있는 시간 (초)")]
    [SerializeField] private float cueOnTime = 0.6f;

    [Header("판정 범위 (초)")]
    [SerializeField] private float perfectWindow = 0.1f;
    [SerializeField] private float goodWindow = 0.3f;

    // 랜덤 정답 순서 (slot index 0~5)
    private readonly List<int> answerPattern = new List<int>();

    // 현재 몇 번째 박인지
    private int inputIndex = 0;

    // 입력 단계 여부
    private bool canInput = false;

    private bool isDemoMode;

    private bool patternMade;
    private int shownIndex = -1;
    private bool inputStarted;
    private bool cueOn;
    private bool finished;

    private float InputStartTime =>
        startDelay + slots.Length * (patternOnTime + patternOffTime) + inputStartDelay;

    private float BeatTime(int k) => InputStartTime + inputInterval * k;

    // 미니게임이 중간에 끝날 때: 남은 입력 수
    public int RemainingInputs => Mathf.Max(0, slots.Length - inputIndex);

    private void Start()
    {
        SetAllOnBoxes(false);
    }

    private void Update()
    {
        if (finished || minigame == null) return;

        float t = minigame.SongTime;
        if (t < 0f) return;

        if (!patternMade)
        {
            patternMade = true;
            Debug.Log("[3-9] 게임 시작");
            CreateRandomPattern();
        }

        // ===== 1) 표시 =====
        if (!inputStarted)
        {
            UpdateShowPattern(t);

            if (t >= InputStartTime)
            {
                isDemoMode = minigame.IsDemoMode;
                inputStarted = true;
                canInput = true;
                inputIndex = 0;
                SetAllOnBoxes(false);
                Debug.Log($"[3-9 Judge] 입력 시작 song={t:F3}");
            }
            else
            {
                return;
            }
        }

        // ===== 2) 입력 =====
        UpdateCue(t);

        if (canInput)
        {
            // 시범: 박 시각에 정답 칸 자동 입력
            if (isDemoMode && t >= BeatTime(inputIndex))
                JudgeClick(answerPattern[inputIndex], t);

            // 안 누르고 지나간 박
            while (canInput && t > BeatTime(inputIndex) + goodWindow)
                Resolve(false, $"{inputIndex + 1}번째 박 안 누름");
        }

        if (!canInput)
        {
            finished = true;
            SetAllOnBoxes(false);
            Debug.Log("[3-9] 모든 입력 완료");
            minigame.FinishGame();
        }
    }

    private void UpdateShowPattern(float t)
    {
        float step = patternOnTime + patternOffTime;
        float since = t - startDelay;

        int idx = -1;
        if (since >= 0f && step > 0f)
        {
            int i = Mathf.FloorToInt(since / step);
            if (i < answerPattern.Count && since - i * step < patternOnTime)
                idx = i;
        }

        if (idx == shownIndex) return;

        if (shownIndex >= 0 && slots[answerPattern[shownIndex]].onBox != null)
            slots[answerPattern[shownIndex]].onBox.SetActive(false);

        shownIndex = idx;

        if (idx >= 0)
        {
            int slotIndex = answerPattern[idx];

            PlaySfx(showSound);

            if (slots[slotIndex].onBox != null)
                slots[slotIndex].onBox.SetActive(true);

            Debug.Log($"[3-9 Judge] 패턴 {idx + 1}번째 표시 (칸 {slotIndex + 1}) song={t:F3}");
        }
    }

    // 박마다 모든 칸 빨갛게 (신호)
    private void UpdateCue(float t)
    {
        bool on = false;
        float since = t - InputStartTime;

        if (since >= 0f && inputInterval > 0f)
        {
            int k = Mathf.FloorToInt(since / inputInterval);
            on = k < slots.Length && since - k * inputInterval < cueOnTime;
        }

        if (on == cueOn) return;
        cueOn = on;
        SetAllOnBoxes(on);
    }

    /// <summary>PatternButton3_9에서 호출</summary>
    public void OnPatternClicked(int slotIndex)
    {
        if (isDemoMode)
        {
            Debug.Log("[3-9] 데모 중 플레이어 입력 무시");
            return;
        }

        if (minigame == null) return;
        JudgeClick(slotIndex, minigame.SongTime);
    }

    private void JudgeClick(int slotIndex, float t)
    {
        if (!canInput)
        {
            Debug.Log($"[3-9 Judge] 클릭 무시 (입력 단계 아님) song={t:F3}");
            return;
        }

        if (slotIndex < 0 || slotIndex >= slots.Length)
        {
            Debug.LogError("[3-9] 잘못된 slotIndex : " + slotIndex);
            return;
        }

        float diff = t - BeatTime(inputIndex);

        if (diff < -goodWindow)
        {
            Debug.Log($"[3-9 Judge] 클릭 무시 (너무 일찍 {diff * 1000:F0}ms) song={t:F3}");
            return;
        }

        int correctIndex = answerPattern[inputIndex];
        string grade = Mathf.Abs(diff) <= perfectWindow ? "Perfect" : "Good";

        if (slotIndex == correctIndex)
            Resolve(true, $"{inputIndex + 1}번째 칸 {slotIndex + 1} 정답 diff={diff * 1000:F0}ms → {grade}");
        else
            Resolve(false, $"{inputIndex + 1}번째 틀린 칸 클릭 {slotIndex + 1} (정답 {correctIndex + 1})");
    }

    private void Resolve(bool success, string reason)
    {
        if (success)
        {
            PlaySfx(clickSound);
            minigame?.ReportPatternSuccess();
        }
        else
        {
            PlaySfx(wrongSound);
            minigame?.ReportPatternFail();
        }

        Debug.Log($"[3-9 Judge] {(success ? "성공" : "실패")} ({reason})");

        inputIndex++;

        if (inputIndex >= answerPattern.Count)
            canInput = false;
    }

    /// <summary>1~6을 중복 없이 랜덤 순서로 만든다.</summary>
    private void CreateRandomPattern()
    {
        answerPattern.Clear();

        List<int> available = new List<int>();
        for (int i = 0; i < slots.Length; i++)
            available.Add(i);

        while (available.Count > 0)
        {
            int r = UnityEngine.Random.Range(0, available.Count);
            answerPattern.Add(available[r]);
            available.RemoveAt(r);
        }

        string debugPattern = "";
        for (int i = 0; i < answerPattern.Count; i++)
        {
            debugPattern += (answerPattern[i] + 1);
            if (i < answerPattern.Count - 1) debugPattern += " -> ";
        }

        Debug.Log("[3-9] 정답 패턴 : " + debugPattern);
    }

    private void SetAllOnBoxes(bool active)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].onBox != null)
                slots[i].onBox.SetActive(active);
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            GameRoot.Instance.Audio.PlaySfx(clip);
    }
}