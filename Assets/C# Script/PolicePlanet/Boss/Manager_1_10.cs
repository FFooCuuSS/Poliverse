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

    // runtime
    private UpDown upPlatform;
    private UpDown downPlatform;

    private Score score;
    private MinigameRemake_1_10 minigame;

    private bool inputOpen = false;
    private bool awaitingJudge = false;
    private bool moveEventRunning = false;

    private int consecutiveShowCount = 0;

    // =========================
    // 데이터 구조: FIFO 큐 + 보낸 사람 목록
    // =========================
    private struct PersonEntry
    {
        public GameObject go;
        public bool isSinner;
        public PersonEntry(GameObject go, bool isSinner) { this.go = go; this.isSinner = isSinner; }
    }

    // Show로 생긴 사람들(대기열) - FIFO
    private readonly Queue<PersonEntry> waitingQueue = new Queue<PersonEntry>();

    // 성공/실패 여부와 상관없이 "플랫폼 쪽으로 보낸 사람들" (Move 이벤트 때도 같이 움직여야 함)
    private readonly List<PersonEntry> sentPeople = new List<PersonEntry>();

    // 현재 판정 대기중인 1명 (Input 1회당 1명)
    private PersonEntry? current = null;
    private bool pendingGoUp = false;
    private bool pendingCorrect = false;

    // 플랫폼 배치 카운트
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

    // =========================
    // Minigame 이벤트
    // =========================
    public void SpawnPersonForShow()
    {
        SpawnPersonInternal(offsetX: consecutiveShowCount * showSpawnStepX);
        inputOpen = false;
        awaitingJudge = false;
        consecutiveShowCount++;
    }

    public void OnInputWindowOpened()
    {
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

        var sr = person.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (spawnSinner)
            {
                int idx = Random.Range(0, sinnerBodySprites.Length);

                // 1) 몸통(루트 SpriteRenderer) 교체
                if (sr != null && sinnerBodySprites != null && sinnerBodySprites.Length > 0)
                    sr.sprite = sinnerBodySprites[idx];

                // 2) 머리(자식 SpriteRenderer) 교체
                var headSr = sr.transform.GetChild(0).GetComponent<SpriteRenderer>();
                if (headSr != null && sinnerHeadSprites != null && sinnerHeadSprites.Length > 0)
                    headSr.sprite = sinnerHeadSprites[idx];

                // 3) 페이드도 머리까지 같이
                if (headSr != null)
                {
                    headSr.color = new Color(headSr.color.r, headSr.color.g, headSr.color.b, 0f);
                    headSr.DOFade(1f, 0.1f);
                }
            }

            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
            sr.DOFade(1f, 0.1f);
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
        // 큐 앞에서부터 null 제거하면서 유효한 사람 찾기
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

        if (pendingCorrect) AddScoreAndCheckWin();

        current = null;
    }

    public void OnMiss()
    {
        // Miss는 무입력 Miss도 들어오므로 awaitingJudge로 막지 말자.
        awaitingJudge = false;
        inputOpen = false;

        // 1) 클릭으로 잡고 있던 current가 있으면 그걸 삭제
        if (current.HasValue && current.Value.go != null)
        {
            StartCoroutine(FadeOutAndDestroyOne(current.Value.go));
            current = null;
            return;
        }

        // 2) 무입력 Miss면 큐에서 "가장 앞" 1명을 삭제(FIFO)
        if (TryDequeueAlive(out var e))
        {
            if (e.go != null) StartCoroutine(FadeOutAndDestroyOne(e.go));
        }
    }

    private void MoveCurrentToLane(bool goUp)
    {
        if (!current.HasValue) return;
        var e = current.Value;
        if (e.go == null) return;

        // 목표 X: -6f 시작 / 4.5f 시작 + 0.4씩 누적
        float baseX = goUp ? 4.5f : -5.7f;
        int idx = goUp ? sentCountUp : sentCountDown;
        float targetX = baseX + (0.5f * idx);

        if (goUp) sentCountUp++;
        else sentCountDown++;

        // 트윈 정리 후 이동
        DOTween.Kill(e.go.transform);

        e.go.transform.DOMoveX(targetX, moveEventDuration)
            .SetEase(Ease.OutQuad);

        sentPeople.Add(e);
    }

    private void AddScoreAndCheckWin()
    {
        if (score != null) score.nScore++;
        if (score != null && score.nScore >= 10)
            minigame?.Succeed();
    }

    private IEnumerator FadeOutAndDestroyOne(GameObject target)
    {
        float dur = Mathf.Max(0.01f, missFadeOutDuration);

        if (target != null)
        {
            // 트윈 정리 (Transform + SpriteRenderer 둘 다)
            DOTween.Kill(target.transform);

            var srs = target.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < srs.Length; i++)
            {
                if (srs[i] == null) continue;
                srs[i].DOKill(); // 기존 페이드/트윈 제거
                srs[i].DOFade(0f, dur).SetEase(Ease.OutQuad);
            }
        }

        yield return new WaitForSeconds(dur);

        if (target != null) Destroy(target);
    }


    // =========================
    // Move 이벤트 (전체 리프트)
    // =========================
    public void MoveBothPlatforms()
    {
        if (moveEventRunning) return;
        moveEventRunning = true;

        // 판정/입력 중단
        awaitingJudge = false;
        inputOpen = false;

        if (upPlatform != null) upPlatform.TryMovePlatformImmediate();
        if (downPlatform != null) downPlatform.TryMovePlatformImmediate();

        if (score.nScore <= 2)
        {
            if (clearObject != null) clearObject.SetActive(false);
            if (failObject != null) failObject.SetActive(true);
            minigame.Fail();
            return;
        }

        sentCountUp = 0;
        sentCountDown = 0;

        MoveAllPeopleY(waitingQueue, sentPeople);
        StartCoroutine(ClearAllAfterMoveEvent(moveEventDuration + moveEventClearDelay));
        minigame.Success();
    }

    private void MoveAllPeopleY(Queue<PersonEntry> q, List<PersonEntry> sent)
    {
        // 큐는 순회가 되지만, null은 그냥 스킵
        foreach (var p in q)
        {
            MoveOneY(p);
        }

        for (int i = 0; i < sent.Count; i++)
        {
            MoveOneY(sent[i]);
        }

        // current도 남아있다면 같이 이동 (클릭 직후 Move가 올 수 있으니)
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

    private IEnumerator ClearAllAfterMoveEvent(float wait)
    {
        yield return new WaitForSeconds(wait);
        ClearAllImmediate();
        moveEventRunning = false;
    }

    // =========================
    // Clear
    // =========================
    private void ClearAllImmediate()
    {
        // current
        if (current.HasValue && current.Value.go != null) Destroy(current.Value.go);
        current = null;

        // queue
        while (waitingQueue.Count > 0)
        {
            var e = waitingQueue.Dequeue();
            if (e.go != null) Destroy(e.go);
        }

        // sent
        for (int i = sentPeople.Count - 1; i >= 0; i--)
        {
            if (sentPeople[i].go != null) Destroy(sentPeople[i].go);
            sentPeople.RemoveAt(i);
        }
    }
}
