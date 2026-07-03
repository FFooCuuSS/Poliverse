using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Manager_3_3 : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Minigame_3_3_Remake minigame;
    [SerializeField] private AutoMoveKey_3_3 keyMover;

    [Header("Scene Objects")]
    [SerializeField] private List<Hole_3_3> holes;
    [SerializeField] private GameObject key;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private int totalRounds = 3;
    [SerializeField] private float[] targetTimes = { 1f, 2f, 3f };

    [Header("Round")]
    [SerializeField] private float nextRoundDelay = 0.25f;

    private Rigidbody2D keyRb;

    private int clickCount;
    private int roundIndex;
    private int successCount;

    private bool roundChanging;
    private bool inputClosed;
    private bool autoInsertRunning;

    private float roundStartTime;
    private Vector3 keyStartPos;

    private void Awake()
    {
        if (key != null)
        {
            keyStartPos = key.transform.position;
            keyRb = key.GetComponent<Rigidbody2D>();
        }
    }

    public float GetRoundTime()
    {
        return Time.time - roundStartTime;
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

        roundStartTime = Time.time;
        SetupRound(clockwise);

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
    public void OnPlayerClick()
    {
        if (inputClosed) return;
        if (roundChanging) return;
        if (autoInsertRunning) return;
        if (holes == null || holes.Count == 0) return;
        if (clickCount >= holes.Count) return;

        holes[clickCount].Lock();
        clickCount++;

        if (clickCount == holes.Count)
        {
            inputClosed = true;
            minigame?.ForceCloseInput();
            StartCoroutine(AutoInsertKey());
        }
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

    private void ResetKeyPhysics()
    {
        if (keyRb == null) return;

        keyRb.velocity = Vector2.zero;
        keyRb.angularVelocity = 0f;
        keyRb.Sleep();
    }
}