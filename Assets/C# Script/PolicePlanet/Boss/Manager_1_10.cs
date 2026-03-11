using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static MiniGameBase;

public class Manager_1_10 : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject BossStage;
    [SerializeField] private GameObject ScoreText;
    [SerializeField] private Transform spawnParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject UpPlatformPrefab;
    [SerializeField] private GameObject DownPlatformPrefab;
    [SerializeField] private GameObject PolicePrefab;
    [SerializeField] private GameObject SinnerPrefab;
    [SerializeField] private Sprite[] sinnerBodySprites;
    [SerializeField] private Sprite[] sinnerHeadSprites;

    [Header("Show Spawn Offset")]
    [SerializeField] private float showSpawnStepX = 0.6f;

    [Header("Miss FadeOut")]
    [SerializeField] private float missFadeOutDuration = 0.15f;

    [Header("Move Event (Lift All)")]
    [SerializeField] private float moveEventDeltaY = 5.0f;
    [SerializeField] private float moveEventDuration = 0.5f;
    [SerializeField] private float moveEventClearDelay = 0.2f;

    [SerializeField] private GameObject clearObject;
    [SerializeField] private GameObject failObject;

    private UpDown upPlatform;
    private UpDown downPlatform;

    private Score score;
    private MinigameRemake_1_10 minigame;

    private bool inputOpen = false;
    private bool awaitingJudge = false;
    private bool moveEventRunning = false;
    private int moveCount = 0;
    private int consecutiveShowCount = 0;

    private struct PersonEntry
    {
        public GameObject go;
        public bool isSinner;
        public PersonEntry(GameObject go, bool isSinner)
        {
            this.go = go;
            this.isSinner = isSinner;
        }
    }

    private readonly Queue<PersonEntry> waitingQueue = new Queue<PersonEntry>();
    private readonly List<PersonEntry> sentPeople = new List<PersonEntry>();

    private PersonEntry? current = null;
    private bool pendingGoUp = false;
    private bool pendingCorrect = false;

    private int sentCountUp = 0;
    private int sentCountDown = 0;

    public enum PlatformType { Up, Down }

    public void OnMinigameStart(MinigameRemake_1_10 mg)
    {
        minigame = mg;

        consecutiveShowCount = 0;
        inputOpen = false;
        awaitingJudge = false;
        moveEventRunning = false;

        current = null;
        pendingGoUp = false;
        pendingCorrect = false;

        sentCountUp = 0;
        sentCountDown = 0;

        ResetSessionVisualState();
        ResetScore();
        ClearAllImmediate();
    }

    private void Start()
    {
        if (BossStage != null && minigame == null)
            minigame = BossStage.GetComponent<MinigameRemake_1_10>();

        if (ScoreText != null && score == null)
            score = ScoreText.GetComponent<Score>();

        if (upPlatform == null) upPlatform = SpawnPlatform(PlatformType.Up);
        if (downPlatform == null) downPlatform = SpawnPlatform(PlatformType.Down);
    }

    private UpDown SpawnPlatform(PlatformType type)
    {
        GameObject prefab = (type == PlatformType.Up) ? UpPlatformPrefab : DownPlatformPrefab;
        if (prefab == null) return null;

        GameObject inst = Instantiate(prefab, prefab.transform.position, Quaternion.identity, spawnParent);
        var ud = inst.GetComponent<UpDown>();
        if (ud != null)
        {
            ud.ManagerObj = gameObject;
            ud.goUp = (type == PlatformType.Up);
        }
        return ud;
    }

    private void ResetSessionVisualState()
    {
        if (clearObject != null) clearObject.SetActive(true);
        if (failObject != null) failObject.SetActive(false);
    }

    private void ShowFailState()
    {
        if (clearObject != null) clearObject.SetActive(true);
        if (failObject != null) failObject.SetActive(true);
    }

    private void ResetScore()
    {
        if (score != null)
            score.nScore = 0;
    }

    // =========================
    // Minigame 이벤트
    // =========================
    public void SpawnPersonForShow()
    {
        if (moveEventRunning) return;

        SpawnPersonInternal(offsetX: consecutiveShowCount * showSpawnStepX);
        inputOpen = false;
        awaitingJudge = false;
        consecutiveShowCount++;
    }

    public void OnInputWindowOpened()
    {
        if (moveEventRunning) return;

        inputOpen = true;
        awaitingJudge = false;
        consecutiveShowCount = 0;
        pendingCorrect = false;
    }

    private void SpawnPersonInternal(float offsetX)
    {
        bool spawnSinner = Random.Range(0, 2) == 0;
        GameObject prefab = spawnSinner ? SinnerPrefab : PolicePrefab;
        if (prefab == null) return;

        Vector3 pos = prefab.transform.position;
        pos.x -= 0.5f;
        pos.x += offsetX;

        GameObject person = Instantiate(prefab, pos, Quaternion.identity, spawnParent);

        var rootSr = person.GetComponent<SpriteRenderer>();
        if (rootSr != null)
        {
            if (spawnSinner)
            {
                if (sinnerBodySprites != null && sinnerBodySprites.Length > 0 &&
                    sinnerHeadSprites != null && sinnerHeadSprites.Length > 0)
                {
                    int idx = Random.Range(0, Mathf.Min(sinnerBodySprites.Length, sinnerHeadSprites.Length));

                    rootSr.sprite = sinnerBodySprites[idx];

                    if (rootSr.transform.childCount > 0)
                    {
                        var headSr = rootSr.transform.GetChild(0).GetComponent<SpriteRenderer>();
                        if (headSr != null)
                        {
                            headSr.sprite = sinnerHeadSprites[idx];
                            headSr.color = new Color(headSr.color.r, headSr.color.g, headSr.color.b, 0f);
                            headSr.DOFade(1f, 0.1f);
                        }
                    }
                }
            }

            rootSr.color = new Color(rootSr.color.r, rootSr.color.g, rootSr.color.b, 0f);
            rootSr.DOFade(1f, 0.1f);
        }

        waitingQueue.Enqueue(new PersonEntry(person, spawnSinner));
    }

    // =========================
    // 플랫폼 클릭 입력
    // =========================
    public void RequestMoveFromPlatform(bool goUp)
    {
        if (!inputOpen) return;
        if (awaitingJudge) return;
        if (minigame == null) return;
        if (moveEventRunning) return;

        if (!TryDequeueAlive(out var entry)) return;

        pendingGoUp = goUp;
        current = entry;
        pendingCorrect = (goUp && !entry.isSinner) || (!goUp && entry.isSinner);

        awaitingJudge = true;
        minigame.SubmitPlayerInput("Input");
    }

    private bool TryDequeueAlive(out PersonEntry entry)
    {
        while (waitingQueue.Count > 0)
        {
            var e = waitingQueue.Dequeue();
            if (e.go != null)
            {
                entry = e;
                return true;
            }
        }

        entry = default;
        return false;
    }

    // =========================
    // 판정 콜백
    // =========================
    public void OnAccepted(JudgementResult judgement)
    {
        if (!awaitingJudge) return;

        awaitingJudge = false;
        inputOpen = false;

        MoveCurrentToLane(pendingGoUp);

        if (pendingCorrect)
            AddScore();

        current = null;
    }

    public void OnMiss()
    {
        awaitingJudge = false;
        inputOpen = false;

        if (current.HasValue && current.Value.go != null)
        {
            StartCoroutine(FadeOutAndDestroyOne(current.Value.go));
            current = null;
            return;
        }

        if (TryDequeueAlive(out var e))
        {
            if (e.go != null)
                StartCoroutine(FadeOutAndDestroyOne(e.go));
        }
    }

    private void MoveCurrentToLane(bool goUp)
    {
        if (!current.HasValue) return;

        var e = current.Value;
        if (e.go == null) return;

        float baseX = goUp ? 4.5f : -5.7f;
        int idx = goUp ? sentCountUp : sentCountDown;
        float targetX = baseX + (0.5f * idx);

        if (goUp) sentCountUp++;
        else sentCountDown++;

        DOTween.Kill(e.go.transform);

        e.go.transform.DOMoveX(targetX, moveEventDuration)
            .SetEase(Ease.OutQuad);

        sentPeople.Add(e);
    }

    private void AddScore()
    {
        if (score != null)
            score.nScore++;
    }

    private IEnumerator FadeOutAndDestroyOne(GameObject target)
    {
        float dur = Mathf.Max(0.01f, missFadeOutDuration);

        if (target != null)
        {
            DOTween.Kill(target.transform);

            var srs = target.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < srs.Length; i++)
            {
                if (srs[i] == null) continue;
                srs[i].DOKill();
                srs[i].DOFade(0f, dur).SetEase(Ease.OutQuad);
            }
        }

        yield return new WaitForSeconds(dur);

        if (target != null)
            Destroy(target);
    }

    // =========================
    // Move 이벤트 (세션 종료용)
    // =========================
    public void MoveBothPlatforms()
    {
        if (moveEventRunning) return;
        moveEventRunning = true;

        awaitingJudge = false;
        inputOpen = false;

        if (upPlatform != null) upPlatform.TryMovePlatformImmediate();
        if (downPlatform != null) downPlatform.TryMovePlatformImmediate();

        bool sessionFail = (score != null && score.nScore <= 2);

        moveCount++;

        if (score != null)
        {
            if (moveCount == 1)
                score.SetMaxScore(6);
            else if (moveCount == 2)
                score.SetMaxScore(8);
        }

        if (sessionFail)
            ShowFailState();
        else
            ResetSessionVisualState();

        sentCountUp = 0;
        sentCountDown = 0;

        MoveAllPeopleY(waitingQueue, sentPeople);
        StartCoroutine(FinishSessionAfterMove(moveEventDuration + moveEventClearDelay));
    }

    private void MoveAllPeopleY(Queue<PersonEntry> q, List<PersonEntry> sent)
    {
        foreach (var p in q)
            MoveOneY(p);

        for (int i = 0; i < sent.Count; i++)
            MoveOneY(sent[i]);

        if (current.HasValue)
            MoveOneY(current.Value);
    }

    private void MoveOneY(PersonEntry p)
    {
        if (p.go == null) return;

        Transform tr = p.go.transform;
        DOTween.Kill(tr);

        float dy = p.isSinner ? -moveEventDeltaY : moveEventDeltaY;
        tr.DOMoveY(tr.position.y + dy, moveEventDuration)
          .SetEase(Ease.OutBack, 1.35f)
          .SetUpdate(false);
    }

    private IEnumerator FinishSessionAfterMove(float wait)
    {
        yield return new WaitForSeconds(wait);

        ClearAllImmediate();

        consecutiveShowCount = 0;
        inputOpen = false;
        awaitingJudge = false;
        pendingGoUp = false;
        pendingCorrect = false;
        current = null;

        ResetScore();
        ResetSessionVisualState();

        moveEventRunning = false;
    }

    // =========================
    // Clear
    // =========================
    private void ClearAllImmediate()
    {
        if (current.HasValue && current.Value.go != null)
            Destroy(current.Value.go);
        current = null;

        while (waitingQueue.Count > 0)
        {
            var e = waitingQueue.Dequeue();
            if (e.go != null)
                Destroy(e.go);
        }

        for (int i = sentPeople.Count - 1; i >= 0; i--)
        {
            if (sentPeople[i].go != null)
                Destroy(sentPeople[i].go);
            sentPeople.RemoveAt(i);
        }
    }
}