using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Manager_3_3 : MonoBehaviour
{

    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;
    [Header("Refs")]
    [SerializeField] private Minigame_3_3_Remake minigame;
    [SerializeField] private AutoMoveKey_3_3 keyMover;

    [Header("Scene Objects")]
    [SerializeField] private List<Hole_3_3> holes;
    [SerializeField] private GameObject key;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private int totalRounds = 3;
    [SerializeField] private float[] targetTimes = { 1.2f, 2.4f, 3.6f };

    [Header("Round")]
    [SerializeField] private float nextRoundDelay = 0.3f;

    [Header("판정 범위 (초) - 구멍이 일직선(0도)이 되는 순간 기준")]
    [SerializeField] private float perfectWindow = 0.1f;
    [SerializeField] private float goodWindow = 0.3f;
    [Tooltip("현재 고리 정렬 시각보다 이만큼 이상 빨리 누르면 무시 (다음 고리를 잡아먹지 않게)")]
    [SerializeField] private float ignoreEarlySeconds = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    // 이번 라운드의 기준 시각 (노래 시간). Show 노트 시각 + fadeDuration
    private float roundStartSong;
    private float pendingShowSong;

    public int TotalTargets => (holes != null ? holes.Count : 0) * totalRounds;

    private Rigidbody2D keyRb;

    private int clickCount;
    private int roundIndex;
    private int successCount;

    private bool roundChanging;
    private bool inputClosed;
    private bool autoInsertRunning;

    private float roundStartTime;
    private Vector3 keyStartPos;

    private bool isDemoMode = false;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log("[3-3 Manager] 데모 모드: " + isDemoMode);
    }

    private void Awake()
    {
        if (key != null)
        {
            keyStartPos = key.transform.position;
            keyRb = key.GetComponent<Rigidbody2D>();
        }
    }

    private float Now()
    {
        if (minigame != null)
        {
            double s = minigame.SongTime;
            if (s >= 0) return (float)s;
        }

        return Time.time;
    }

    // 라운드 시작 후 경과 시간 (노래 시간 기준)
    public float GetRoundTime()
    {
        return Now() - roundStartSong;
    }

    public void OnMinigameStart(Minigame_3_3_Remake mg)
    {
        minigame = mg;

        clickCount = 0;
        roundIndex = 0;
        successCount = 0;

        roundChanging = false;
        inputClosed = true;
        autoInsertRunning = false;

        ResetKeyPhysics();
    }

    public void CloseInput()
    {
        inputClosed = true;
    }

    // =========================
    // ROUND
    // =========================
    public void StartNextRound()
    {
        if (roundChanging) return;
        if (autoInsertRunning) return;
        if (roundIndex >= totalRounds) return;

        // Show 노트가 온 순간을 기준으로 잡는다 (페이드 대기는 연출일 뿐)
        pendingShowSong = Now();

        StartCoroutine(RoundFlow());
    }

    private IEnumerator RoundFlow()
    {
        roundChanging = true;
        inputClosed = true;
        clickCount = 0;

        if (holes != null)
        {
            foreach (var h in holes)
            {
                if (h != null)
                    FadeOut(h.gameObject);
            }
        }

        if (key != null)
            FadeOut(key);

        yield return new WaitForSeconds(fadeDuration);

        ResetKeyPhysics();

        if (key != null)
        {
            key.transform.position = keyStartPos;
            key.transform.rotation = Quaternion.identity;
        }

        ResetKeyPhysics();

        if (keyMover != null)
            keyMover.ResetState();

        if (key != null)
            FadeIn(key);

        bool clockwise = (roundIndex % 2 == 0);

        // 기존 체감 유지: Show + fadeDuration 이 라운드 0초 (WaitForSeconds 지연은 제외)
        roundStartSong = pendingShowSong + fadeDuration;
        roundStartTime = Time.time;
        SetupRound(clockwise);

        if (judgeLog)
        {
            string times = "";
            for (int i = 0; i < holes.Count && i < targetTimes.Length; i++)
                times += $"{roundStartSong + targetTimes[i]:F2} ";
            Debug.Log($"[3-3 Judge] ROUND {roundIndex + 1} 정렬 시각(song): {times}");
        }

        if (holes != null)
        {
            foreach (var h in holes)
            {
                if (h != null)
                    FadeIn(h.gameObject);
            }
        }

        roundIndex++;

        inputClosed = false;
        roundChanging = false;

        if (isDemoMode)
        {
            StartCoroutine(DemoAutoLockFlow());
        }
    }

    private void FadeOut(GameObject go)
    {
        if (go == null) return;

        var srs = go.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sr in srs)
        {
            sr.DOKill();
            sr.DOFade(0f, fadeDuration);
        }
    }

    private void FadeIn(GameObject go)
    {
        if (go == null) return;

        var srs = go.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sr in srs)
        {
            sr.DOKill();

            Color c = sr.color;
            c.a = 0f;
            sr.color = c;

            sr.DOFade(1f, fadeDuration);
        }
    }

    private void SetupRound(bool clockwise)
    {
        if (holes == null) return;

        for (int i = 0; i < holes.Count; i++)
        {
            if (holes[i] == null) continue;

            float t = (i < targetTimes.Length) ? targetTimes[i] : (i + 1);

            holes[i].Init(
                t,
                clockwise,
                i,
                GetRoundTime
            );
        }
    }

    // =========================
    // INPUT
    // =========================
    private float TargetOf(int i)
    {
        return (targetTimes != null && i < targetTimes.Length) ? targetTimes[i] : i + 1f;
    }

    // 놓친 고리는 시간이 지나면 자동 실패 처리하고 다음 고리로 넘긴다
    // (예전엔 못 누른 고리가 계속 '다음 차례'로 남아서, 이후 클릭이 전부 한 칸씩 밀렸다)
    private void Update()
    {
        if (inputClosed || roundChanging || autoInsertRunning) return;
        if (isDemoMode) return;
        if (holes == null || clickCount >= holes.Count) return;

        float t = GetRoundTime();

        while (clickCount < holes.Count && t > TargetOf(clickCount) + goodWindow)
        {
            if (judgeLog)
                Debug.Log($"[3-3 Judge] 구멍{clickCount} 시간 초과 → Miss");

            if (holes[clickCount] != null)
                holes[clickCount].LockWithResult(false);

            minigame?.ReportManualFail();
            clickCount++;
        }

        if (clickCount >= holes.Count)
            FinishRoundInput();
    }

    private void FinishRoundInput()
    {
        if (inputClosed) return;

        inputClosed = true;
        minigame?.ForceCloseInput();
        StartCoroutine(AutoInsertKey());
    }

    public void OnPlayerClick()
    {
        if (inputClosed) return;
        if (roundChanging) return;
        if (autoInsertRunning) return;
        if (holes == null || holes.Count == 0) return;
        if (clickCount >= holes.Count) return;

        // 너무 이른 클릭은 무시 (차례를 소모하지 않음)
        float diff = GetRoundTime() - TargetOf(clickCount);
        if (diff < -ignoreEarlySeconds)
        {
            if (judgeLog)
                Debug.Log($"[3-3 Judge] 너무 이른 클릭 무시 (구멍{clickCount} diff={diff * 1000:F0}ms)");
            return;
        }

        JudgeAndLock(clickCount);
        clickCount++;
        if (clickSound != null)
        {
            if (GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            {
                GameRoot.Instance.Audio.PlaySfx(clickSound);
            }
        }


        if (clickCount == holes.Count)
            FinishRoundInput();
    }

    // =========================
    // KEY MOVE
    // =========================
    private IEnumerator AutoInsertKey()
    {
        autoInsertRunning = true;

        yield return new WaitForSeconds(0.2f);

        bool allCorrect = true;

        if (holes != null)
        {
            foreach (var h in holes)
            {
                if (h == null) continue;

                if (!h.IsAligned())
                {
                    allCorrect = false;
                    break;
                }
            }
        }

        if (keyMover != null)
            keyMover.StartMove();

        yield return new WaitForSeconds(0.5f);

        if (allCorrect)
            successCount++;

        autoInsertRunning = false;

        if (roundIndex >= totalRounds)
        {
            minigame?.FinishGame(successCount);
        }
    }

    // 클릭 순간이 구멍 i의 정렬 시각에서 얼마나 어긋났는지로 판정
    private void JudgeAndLock(int i)
    {
        if (holes == null || i < 0 || i >= holes.Count || holes[i] == null) return;
        if (holes[i].IsLocked) return;

        float target = (targetTimes != null && i < targetTimes.Length) ? targetTimes[i] : i + 1f;
        float diff = GetRoundTime() - target;
        float abs = Mathf.Abs(diff);

        bool success = abs <= goodWindow;
        string grade = abs <= perfectWindow ? "Perfect" : success ? "Good" : "Miss";

        holes[i].LockWithResult(success);

        if (success) minigame?.ReportManualSuccess();
        else minigame?.ReportManualFail();

        if (judgeLog)
            Debug.Log($"[3-3 Judge] 구멍{i} diff={diff * 1000:F0}ms → {grade}");
    }

    private void ResetKeyPhysics()
    {
        if (keyRb == null) return;

        keyRb.velocity = Vector2.zero;
        keyRb.angularVelocity = 0f;
        keyRb.Sleep();
    }

    private IEnumerator DemoAutoLockFlow()
    {
        if (holes == null || holes.Count == 0)
            yield break;

        // 기존 targetTimes에 맞춰 각 구멍을 자동으로 잠근다.
        for (int i = 0; i < holes.Count; i++)
        {
            float targetTime =
                (targetTimes != null && i < targetTimes.Length)
                    ? targetTimes[i]
                    : i + 1f;

            while (GetRoundTime() < targetTime)
                yield return null;

            if (holes[i] != null)
            {
                JudgeAndLock(i);

                Debug.Log(
                    $"[3-3 Demo] Hole {i} 자동 잠금, 시간: {targetTime}"
                );
            }
        }

        inputClosed = true;
        minigame?.ForceCloseInput();

        // 플레이어가 모든 구멍을 클릭했을 때와 같은 후속 동작
        if (!autoInsertRunning)
        {
            StartCoroutine(AutoInsertKey());
        }
    }
}