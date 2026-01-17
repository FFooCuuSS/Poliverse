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
    [SerializeField] private GameObject UpPlatform;
    [SerializeField] private GameObject DownPlatform;
    [SerializeField] private GameObject Police;
    [SerializeField] private GameObject Sinner;
    [SerializeField] private Sprite[] sinnerSprites;
    private UpDown upPlatform;
    private UpDown downPlatform;

    [Header("Show Spawn Offset")]
    [SerializeField] private float showSpawnStepX = 0.6f;

    [Header("Miss FadeOut")]
    [SerializeField] private float missFadeOutDuration = 0.15f;

    [Header("Player Input (Touch/Swipe)")]
    [SerializeField] private bool enablePointerInput = true;
    [SerializeField] private float swipeThresholdPx = 60f;
    [SerializeField] private float swipeMaxTime = 0.35f; // 너무 느린 드래그는 탭으로 처리

    private Vector2 pointerDownPos;
    private float pointerDownTime;
    private bool pointerDown;
    private bool inputOpen = false;

    public bool platformIsMoving = false;

    public enum PlatformType { Up, Down }

    // ====== 핵심: 스폰할 때 타입을 같이 저장 ======
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

    // Show로 쌓이는 전체(이전 사람 지우지 않음)
    private readonly List<PersonEntry> people = new List<PersonEntry>();

    // Input 시점에 고정되는 "이번 턴 대상"
    private List<PersonEntry> turnPeople = new List<PersonEntry>();

    // 현재 선택된 사람(입력 1회에 1명)
    private PersonEntry? selected = null;

    private Score score;
    private MinigameRemake_1_10 minigame;

    private bool hasPendingMove = false;
    private bool pendingGoUp = false;
    private UpDown pendingPlatform = null;

    private int consecutiveShowCount = 0;

    public void OnMinigameStart(MinigameRemake_1_10 mg)
    {
        minigame = mg;
        consecutiveShowCount = 0;

        hasPendingMove = false;
        pendingGoUp = false;
        pendingPlatform = null;
        platformIsMoving = false;

        ClearAllPeopleImmediate();
    }

    private void Start()
    {
        if (BossStage != null && minigame == null)
            minigame = BossStage.GetComponent<MinigameRemake_1_10>();

        if (ScoreText != null && score == null)
            score = ScoreText.GetComponent<Score>();

        upPlatform = UpPlatform.GetComponent<UpDown>();
        downPlatform = DownPlatform.GetComponent<UpDown>();

        SpawnPlatform(PlatformType.Up);
        SpawnPlatform(PlatformType.Down);
    }
    private void Update()
    {
        if (!enablePointerInput) return;
        if (!inputOpen) return;                 // Input 구간만 받음
        if (platformIsMoving) return;           // gate
        if (hasPendingMove) return;             // 판정 대기중 중복 입력 방지
        if (turnPeople == null || turnPeople.Count == 0) return;

        // 모바일 터치
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                pointerDown = true;
                pointerDownPos = t.position;
                pointerDownTime = Time.unscaledTime;
            }
            else if ((t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) && pointerDown)
            {
                pointerDown = false;
                HandlePointerUp(t.position);
            }
            return;
        }

        // PC 마우스(에디터 테스트)
        if (Input.GetMouseButtonDown(0))
        {
            pointerDown = true;
            pointerDownPos = Input.mousePosition;
            pointerDownTime = Time.unscaledTime;
        }
        else if (Input.GetMouseButtonUp(0) && pointerDown)
        {
            pointerDown = false;
            HandlePointerUp((Vector2)Input.mousePosition);
        }
    }

    private void HandlePointerUp(Vector2 upPos)
    {
        Vector2 delta = upPos - pointerDownPos;
        float dt = Time.unscaledTime - pointerDownTime;

        bool isSwipe = (Mathf.Abs(delta.x) >= swipeThresholdPx) && (dt <= swipeMaxTime);

        bool goUp;
        if (isSwipe)
        {
            // 오른쪽 스와이프 = Up, 왼쪽 = Down
            goUp = delta.x > 0f;
        }
        else
        {
            // 탭이면 화면 좌/우로 결정
            goUp = upPos.x >= (Screen.width * 0.5f); // 오른쪽 탭=Up
        }

        RequestMoveFromInput(goUp);
    }


    // =========================
    // 플랫폼 생성
    // =========================
    public void SpawnPlatform(PlatformType type)
    {
        GameObject prefab = (type == PlatformType.Up) ? UpPlatform : DownPlatform;
        if (prefab == null) return;

        GameObject instance = Instantiate(prefab, prefab.transform.position, Quaternion.identity, spawnParent);

        var upDown = instance.GetComponent<UpDown>();
        if (upDown != null)
        {
            upDown.ManagerObj = this.gameObject;

            if (type == PlatformType.Up) upPlatform = upDown;
            else downPlatform = upDown;
        }
    }

    // =========================
    // Show/Input 이벤트
    // =========================
    public void SpawnPersonForShow()
    {
        SpawnPersonInternal(offsetX: consecutiveShowCount * showSpawnStepX);
        inputOpen = false;
        consecutiveShowCount++;
    }

    public void OnInputWindowOpened()
    {
        // Input 시작 시점에 이번 턴 대상자 고정
        inputOpen = true;
        turnPeople = new List<PersonEntry>(people);

        consecutiveShowCount = 0;
        selected = null;
    }

    private void SpawnPersonInternal(float offsetX)
    {
        bool spawnSinner = Random.Range(0, 2) == 0;
        GameObject prefab = spawnSinner ? Sinner : Police;
        if (prefab == null) return;

        Vector3 pos = prefab.transform.position;
        pos.x -= 0.5f;
        pos.x += offsetX;

        GameObject person = Instantiate(prefab, pos, Quaternion.identity, spawnParent);

        var sr = person.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (spawnSinner && sinnerSprites != null && sinnerSprites.Length > 0)
                sr.sprite = sinnerSprites[Random.Range(0, sinnerSprites.Length)];

            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
            StartCoroutine(FadeIn(sr, 0.1f));
        }

        people.Add(new PersonEntry(person, spawnSinner));
    }

    private IEnumerator FadeIn(SpriteRenderer sr, float duration)
    {
        float timer = 0f;
        Color original = sr.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float a = Mathf.Clamp01(timer / duration);
            sr.color = new Color(original.r, original.g, original.b, a);
            yield return null;
        }
        sr.color = new Color(original.r, original.g, original.b, 1f);
    }

    // =========================
    // 입력 기반 요청(스와이프/터치에서 호출)
    // =========================
    public void RequestMoveFromInput(bool goUp)
    {
        RequestMoveFromPlatform(goUp, null);
    }

    // (플랫폼 클릭도 여전히 쓰면 이걸 호출하면 됨)
    public void RequestMoveFromPlatform(bool goUp, UpDown platform)
    {
        if (platformIsMoving) return;

        platformIsMoving = true;
        hasPendingMove = true;
        pendingGoUp = goUp;
        pendingPlatform = platform;

        SelectForDirection(goUp);

        if (minigame != null) minigame.SubmitPlayerInput();
        else
        {
            Debug.LogWarning("[Manager_1_10] minigame is NULL.");
            platformIsMoving = false;
            hasPendingMove = false;
            pendingPlatform = null;
            selected = null;
        }
    }

    private void SelectForDirection(bool goUp)
    {
        // 규칙: Up=Police, Down=Sinner 라면
        bool wantSinner = !goUp;

        selected = FindLatest(turnPeople, wantSinner);

        // 안전장치: 턴 리스트가 비었으면 전체에서라도
        if (selected == null)
            selected = FindLatest(people, wantSinner);
    }

    private PersonEntry? FindLatest(List<PersonEntry> list, bool wantSinner)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var e = list[i];
            if (e.go == null) continue;
            if (e.isSinner == wantSinner) return e;
        }

        // 없으면 마지막 생존자
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var e = list[i];
            if (e.go == null) continue;
            return e;
        }

        return null;
    }

    // =========================
    // 리듬 판정 콜백
    // =========================
    public void OnRhythmAccepted(JudgementResult judgement)
    {
        if (!hasPendingMove) return;

        DoMoveSelected(pendingGoUp);

        if (pendingPlatform != null)
            pendingPlatform.TriggerPlatformMove(0.65f);
        else
            StartCoroutine(ForceReleasePlatformGate(1.2f));

        hasPendingMove = false;
        pendingPlatform = null;
    }

    public void OnRhythmMiss()
    {
        hasPendingMove = false;
        pendingPlatform = null;
        platformIsMoving = false;
        selected = null;

        StartCoroutine(FadeOutAndClearTurnPeople());
    }

    private IEnumerator ForceReleasePlatformGate(float t)
    {
        yield return new WaitForSeconds(t);
        platformIsMoving = false;
    }

    private IEnumerator FadeOutAndClearTurnPeople()
    {
        var list = turnPeople;
        turnPeople = new List<PersonEntry>();

        float dur = Mathf.Max(0.01f, missFadeOutDuration);

        foreach (var e in list)
        {
            if (e.go == null) continue;
            var sr = e.go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.DOFade(0f, dur).SetEase(Ease.OutQuad);
        }

        yield return new WaitForSeconds(dur);

        foreach (var e in list)
        {
            if (e.go == null) continue;
            RemoveFromPeople(e.go);
            Destroy(e.go);
        }
    }

    private void RemoveFromPeople(GameObject go)
    {
        for (int i = people.Count - 1; i >= 0; i--)
        {
            if (people[i].go == go)
            {
                people.RemoveAt(i);
                return;
            }
        }
    }

    private void ClearAllPeopleImmediate()
    {
        for (int i = people.Count - 1; i >= 0; i--)
        {
            if (people[i].go != null) Destroy(people[i].go);
        }
        people.Clear();
        turnPeople.Clear();
        selected = null;
    }

    // =========================
    // 이동/정답 처리
    // =========================
    private void DoMoveSelected(bool goUp)
    {
        if (selected == null) return;

        var e = selected.Value;
        if (e.go == null) return;

        float targetX = goUp ? 5f : -5f;
        Vector2 targetPos = new Vector2(targetX, e.go.transform.position.y);

        e.go.transform.DOMove(targetPos, 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => StartCoroutine(MoveUpOrDownAfterDelay(e.go, goUp)));

        bool correct = (goUp && !e.isSinner) || (!goUp && e.isSinner);
        if (correct) Success();
        else Failure();
    }

    private IEnumerator MoveUpOrDownAfterDelay(GameObject person, bool goUp)
    {
        yield return new WaitForSeconds(0.15f);
        if (person == null) yield break;

        Vector2 finalPos = (Vector2)person.transform.position + (Vector2)Vector3.up * (goUp ? 12f : -12f);
        person.transform.DOMove(finalPos, 0.3f).SetEase(Ease.OutQuad);
    }

    private void Success()
    {
        if (score != null) score.nScore++;

        if (minigame != null) minigame.OnPlayerInput();

        if (score != null && score.nScore >= 10)
        {
            if (minigame != null) minigame.Succeed();
        }
    }

    private void Failure()
    {
        // 실패 정책
        // minigame.Failure();
    }
}
