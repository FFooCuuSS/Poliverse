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

    private int clickCount = 0;
    private int roundIndex = 0;
    private int successCount = 0;

    private float roundStartTime;
    private Vector3 keyStartPos;

    private void Awake()
    {
        keyStartPos = key.transform.position;
    }

    public float GetRoundTime()
    {
        return Time.time - roundStartTime;
    }

    public void OnMinigameStart(Minigame_3_3_Remake mg)
    {
        minigame = mg;
        roundIndex = 0;
        successCount = 0;
    }

    // =========================
    // ROUND
    // =========================
    public void StartNextRound()
    {
        StartCoroutine(RoundFlow());
    }

    private IEnumerator RoundFlow()
    {
        foreach (var h in holes)
            FadeOut(h.gameObject);

        FadeOut(key);

        yield return new WaitForSeconds(fadeDuration);

        key.transform.position = keyStartPos;
        key.transform.rotation = Quaternion.identity;
        keyMover.ResetState();

        FadeIn(key);

        clickCount = 0;

        bool clockwise = (roundIndex % 2 == 0);

        roundStartTime = Time.time;
        SetupRound(clockwise);

        foreach (var h in holes)
            FadeIn(h.gameObject);

        roundIndex++;
    }

    private void FadeOut(GameObject go)
    {
        var srs = go.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sr in srs)
            sr.DOFade(0f, fadeDuration);
    }

    private void FadeIn(GameObject go)
    {
        var srs = go.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sr in srs)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
            sr.DOFade(1f, fadeDuration);
        }
    }

    private void SetupRound(bool clockwise)
    {
        for (int i = 0; i < holes.Count; i++)
        {
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
        if (clickCount >= holes.Count) return;

        holes[clickCount].Lock();
        clickCount++;

        if (clickCount == holes.Count)
        {
            StartCoroutine(AutoInsertKey());
        }
    }

    // =========================
    // KEY MOVE
    // =========================
    private IEnumerator AutoInsertKey()
    {
        yield return new WaitForSeconds(0.2f);

        bool allCorrect = true;

        foreach (var h in holes)
        {
            if (!h.IsAligned())
            {
                allCorrect = false;
                break;
            }
        }

        keyMover.StartMove();

        yield return new WaitForSeconds(0.5f);

        if (allCorrect)
        {
            successCount++;
        }

        if (roundIndex >= totalRounds)
        {
            minigame.FinishGame(successCount);
        }
    }
}