using System.Collections.Generic;
using UnityEngine;

// 3-5 도청기 설치 (스나이퍼)
// 창문 16칸 중 랜덤 6~12칸에 적이 있다. 스코프가 노래 3.6초부터 0.6초마다 한 칸씩 이동.
// 스코프가 '적이 있는 창문'에 도착하는 박자에 터치하면 처치.
//
// 판정(직접): 가장 가까운 창문 도착 시각 ±goodWindow 안의 터치만 인정.
//  - 적 있는 창문 → 처치(성공)
//  - 빈 창문     → 오발 (처치 수에서 1 차감)
// 점수: 대상 = 적 수. 결과 = 처치 - 오발 (최소 0), 나머지는 Miss.
// 모든 시간은 노래 시간 기준. CSV 3-5 노트는 쓰지 않는다.
public class Minigame_3_5_remake : MiniGameBase, IPracticeDemoInput
{
    public GameObject scope;
    public ScopeMover scopeMover;

    protected override float TimerDuration => 15f;

    protected override string MinigameTitle => "도청기 설치";

    protected override string MinigameExplain => "스코프가 위에서 아래로 움직입니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "타이밍에 맞춰 화면을 터치하세요."
    };

    protected override bool UseRhythmJudgementScore => false;
    protected override int ManualTotalNodeCount => -1;   // 적 수로 런타임에 정함

    [Header("데모")]
    [SerializeField] private ScopeShooter scopeShooter;

    [Header("타이밍 (노래 시간, 초)")]
    [Tooltip("스코프가 나타나 이동을 시작하는 시각")]
    [SerializeField] private float moveStartTime = 3.6f;

    [Header("판정 범위 (초)")]
    [SerializeField] private float perfectWindow = 0.1f;
    [SerializeField] private float goodWindow = 0.3f;

    [Tooltip("창문 위치와 적 위치를 같은 칸으로 볼 거리")]
    [SerializeField] private float matchDistance = 0.6f;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    private bool isDemoMode = false;
    public new bool IsDemoMode => isDemoMode;

    private bool ended;
    private bool scopeShown;
    private float localTime;

    private int enemyCount = -1;
    private int hitCount;
    private int wrongCount;

    // movePoints 인덱스별 적 (없으면 null)
    private GameObject[] enemyAtPoint;
    private bool[] slotUsed;

    public float SongTime
    {
        get
        {
            if (rhythmManager is RhythmManager rm)
                return rm.IsRunning ? (float)rm.SongTime : -1f;

            return localTime;
        }
    }

    private void JudgeLog(string msg)
    {
        if (judgeLog) Debug.Log(msg);
    }

    protected override void Awake()
    {
        base.Awake();

        if (scope != null && scopeShooter == null)
            scopeShooter = scope.GetComponentInChildren<ScopeShooter>(true);
    }

    private void Start()
    {
        if (scope != null)
            scope.SetActive(false);
    }

    public override void SetDemoMode(bool isDemo)
    {
        base.SetDemoMode(isDemo);
        isDemoMode = isDemo;

        if (scopeShooter != null)
            scopeShooter.SetDemoMode(isDemo);

        Debug.Log("[3-5] 데모 모드: " + isDemoMode);
    }

    public override void StartGame()
    {
        base.StartGame();

        ended = false;
        scopeShown = false;
        localTime = 0f;
        hitCount = 0;
        wrongCount = 0;
        enemyCount = -1;
        enemyAtPoint = null;
        slotUsed = null;
    }

    // 적은 Instantiater3_5.Start에서 생성되므로 처음 필요할 때 매핑한다
    private void BuildEnemyMap()
    {
        if (enemyAtPoint != null || scopeMover == null || scopeMover.movePoints == null) return;

        int n = scopeMover.movePoints.Length;
        enemyAtPoint = new GameObject[n];
        slotUsed = new bool[n];

        var enemies = new List<GameObject>();
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("Enemy"))
                enemies.Add(t.gameObject);
        }

        enemyCount = 0;

        for (int i = 0; i < n; i++)
        {
            Vector2 p = scopeMover.movePoints[i];

            foreach (var e in enemies)
            {
                if (e == null) continue;
                if (Vector2.Distance(e.transform.position, p) <= matchDistance)
                {
                    enemyAtPoint[i] = e;
                    enemyCount++;
                    break;
                }
            }
        }

        JudgeLog($"[3-5 Judge] 적 {enemyCount}명 (찾은 Enemy 오브젝트 {enemies.Count}개)");
    }

    private void Update()
    {
        localTime += Time.deltaTime;

        if (ended) return;

        float t = SongTime;
        if (t < 0f) return;

        BuildEnemyMap();

        // 스코프 등장 + 위치 (노래 시간으로 직접 계산)
        if (t >= moveStartTime)
        {
            if (!scopeShown && scope != null)
            {
                scopeShown = true;
                scope.SetActive(true);
            }

            if (scopeMover != null)
                scopeMover.SetByElapsed(t - moveStartTime);
        }

        // 마지막 창문 판정 범위가 지나면 종료
        if (scopeMover != null && scopeMover.movePoints != null && scopeMover.movePoints.Length > 0)
        {
            float last = moveStartTime + scopeMover.ArrivalTime(scopeMover.movePoints.Length - 1);
            if (t > last + goodWindow)
            {
                Finish();
                return;
            }
        }

        if (isDemoMode)
        {
            DemoTick(t);
            return;
        }

        if (IsInputLocked) return;

        if (Input.GetMouseButtonDown(0))
            Shoot(t);
    }

    private int NearestSlot(float t, out float diff)
    {
        diff = 0f;
        if (scopeMover == null || scopeMover.movePoints == null) return -1;

        float step = scopeMover.moveTimePerStep;
        int n = scopeMover.movePoints.Length;

        // 도착 시각: moveStartTime + step * (i + 1)
        int i = Mathf.RoundToInt((t - moveStartTime) / step) - 1;
        i = Mathf.Clamp(i, 0, n - 1);

        diff = t - (moveStartTime + scopeMover.ArrivalTime(i));
        return i;
    }

    private void Shoot(float t)
    {
        if (ended || !scopeShown) return;
        if (enemyAtPoint == null) return;

        int slot = NearestSlot(t, out float diff);
        if (slot < 0) return;

        if (Mathf.Abs(diff) > goodWindow)
        {
            JudgeLog($"[3-5 Judge] 창문 사이 터치 무시 (칸{slot} diff={diff * 1000:F0}ms)");
            return;
        }

        if (slotUsed[slot]) return;
        slotUsed[slot] = true;

        GameObject enemy = enemyAtPoint[slot];
        string grade = Mathf.Abs(diff) <= perfectWindow ? "Perfect" : "Good";

        if (enemy != null)
        {
            hitCount++;
            Destroy(enemy);
            enemyAtPoint[slot] = null;

            if (scopeShooter != null) scopeShooter.PlayHit();
            JudgeLog($"[3-5 Judge] 칸{slot} 처치 diff={diff * 1000:F0}ms → {grade}");
        }
        else
        {
            wrongCount++;
            if (scopeShooter != null) scopeShooter.PlayMiss();
            JudgeLog($"[3-5 Judge] 칸{slot} 빈 창문 오발");
        }
    }

    // 시범: 적이 있는 창문 도착 순간 자동 사격
    private void DemoTick(float t)
    {
        if (enemyAtPoint == null || scopeMover == null) return;

        for (int i = 0; i < enemyAtPoint.Length; i++)
        {
            if (enemyAtPoint[i] == null || slotUsed[i]) continue;

            if (t >= moveStartTime + scopeMover.ArrivalTime(i))
            {
                Shoot(t);
                break;
            }
        }
    }

    private void Finish()
    {
        if (ended) return;
        ended = true;

        int total = Mathf.Max(0, enemyCount);
        int score = Mathf.Clamp(hitCount - wrongCount, 0, total);

        SetRuntimeTotalNodeCount(total);

        for (int i = 0; i < score; i++)
            ReportManualSuccess();

        for (int i = score; i < total; i++)
            ReportManualFail();

        JudgeLog($"[3-5 Judge] 결과 {score}/{total} (처치 {hitCount}, 오발 {wrongCount})");
    }

    public override ScoreResult FinalizeScoreSession()
    {
        // 중간에 끝나도 지금까지 결과로 정산
        Finish();
        return base.FinalizeScoreSession();
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // RhythmManager 판정은 쓰지 않는다
    }

    // 시범은 Update(DemoTick)에서 처리
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
    }
}