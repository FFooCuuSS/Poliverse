using System.Collections.Generic;
using UnityEngine;

// 3-10 안전 운전
// 라운드: Show(경고 표지판 2개 = 장애물 올 도로) → Spawn(장애물 생성) → 장애물이 차에 도착
// 판정(직접, 라운드마다 1점):
//  - 장애물이 차에 도착하는 순간(Spawn + arrivalDelay) 차가 경고 없는 도로에 있으면 성공
//  - 경고 도로에 있으면 실패 (충돌)
//  - 미니게임이 먼저 끝나면 그 시점 차 위치로 판정
public class Minigame_3_10_remake : MiniGameBase, IPracticeDemoInput
{
    [Header("효과음")]
    [SerializeField] private AudioClip warningSound;
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;
    public GameObject warningSign1;
    public GameObject warningSign2;
    public GameObject enemy;

    public Transform[] spawnPoints; // 3개라고 가정
    float fixedEnemyPosX = 11f;

    // 현재 선택된 스폰 인덱스 저장
    private int index1;
    private int index2;
    private int count;

    protected override string MinigameTitle => "안전 운전";

    protected override string MinigameExplain => "장애물을 피해 위 아래로 움직이세요.";

    // 직접 판정. 총 개수는 실제 라운드 수로 정산할 때 정함
    protected override bool UseRhythmJudgementScore => false;
    protected override int ManualTotalNodeCount => -1;

    [Header("자동 데모")]
    [SerializeField] private CarMoveRemake car;

    [Header("판정")]
    [Tooltip("Spawn 후 장애물이 차에 닿기까지 걸리는 시간 (x 11 → -4, 속도 30 = 0.5초)")]
    [SerializeField] private float arrivalDelay = 0.5f;
    [Tooltip("차 y와 경고 도로 y 차이가 이보다 작으면 같은 도로로 봄")]
    [SerializeField] private float laneTolerance = 1.0f;
    [Tooltip("경고 표지판 표시 시간")]
    [SerializeField] private float signShowTime = 2f;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    private bool isDemoMode;
    public new bool IsDemoMode => isDemoMode;

    private class Round
    {
        public int index;
        public float dangerY1;
        public float dangerY2;
        public float judgeTime = -1f;   // Spawn 전이면 -1
        public bool done;
    }

    private readonly List<Round> rounds = new List<Round>();
    private bool finalized;

    private float SongT
    {
        get
        {
            if (rhythmManager is RhythmManager rm && rm.IsRunning)
                return (float)rm.SongTime;

            return Time.time;
        }
    }

    private void JudgeLog(string msg)
    {
        if (judgeLog) Debug.Log(msg);
    }

    public override void SetDemoMode(bool isDemo)
    {
        base.SetDemoMode(isDemo);
        isDemoMode = isDemo;

        if (car == null)
            car = GetComponentInChildren<CarMoveRemake>(true);
        if (car == null)
            car = FindObjectOfType<CarMoveRemake>();

        if (car != null)
        {
            car.SetWarningSigns(warningSign1, warningSign2);
            car.SetDemoMode(isDemoMode);
        }

        Debug.Log("[3-10] 데모 모드: " + isDemoMode);
    }

    void Start()
    {
        count = 0;
        rounds.Clear();
        finalized = false;

        if (car == null)
            car = GetComponentInChildren<CarMoveRemake>(true);
        if (car == null)
            car = FindObjectOfType<CarMoveRemake>();

        if (car != null)
        {
            car.SetWarningSigns(warningSign1, warningSign2);
            car.SetDemoMode(isDemoMode);
        }
        else
        {
            Debug.LogError("[3-10] CarMoveRemake를 찾을 수 없습니다.");
        }

        if (spawnPoints == null || spawnPoints.Length < 3)
        {
            Debug.LogError("[3-10] Spawn Points를 3개 이상 연결해야 합니다.");
            return;
        }

        if (warningSign1 == null || warningSign2 == null || enemy == null)
        {
            Debug.LogError("[3-10] 경고 표지판 또는 Enemy가 연결되지 않았습니다.");
            return;
        }

        warningSign1.SetActive(false);
        warningSign2.SetActive(false);

        base.StartGame();
        SetRandomPositions();
    }

    private void Update()
    {
        if (finalized) return;

        float t = SongT;

        for (int i = 0; i < rounds.Count; i++)
        {
            Round r = rounds[i];
            if (!r.done && r.judgeTime >= 0f && t >= r.judgeTime)
                JudgeRound(r, "장애물 도착");
        }
    }

    public override void OnRhythmEvent(string action)
    {
        if (finalized) return;

        if (action == "Show")
        {
            count++;
            if (count >= 5)
                return;

            if (warningSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
                GameRoot.Instance.Audio.PlaySfx(warningSound);

            ShowObject();
        }

        if (action == "Spawn")
        {
            if (clickSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
                GameRoot.Instance.Audio.PlaySfx(clickSound);

            SpawnObject();
        }
    }

    // 랜덤으로 서로 다른 2개 인덱스 뽑기
    void SetRandomPositions()
    {
        index1 = Random.Range(0, spawnPoints.Length);

        do
        {
            index2 = Random.Range(0, spawnPoints.Length);
        } while (index2 == index1);

        warningSign1.transform.position = spawnPoints[index1].position;
        warningSign2.transform.position = spawnPoints[index2].position;
    }

    // Show 처리: 새 라운드 시작
    void ShowObject()
    {
        SetRandomPositions();

        warningSign1.SetActive(true);
        warningSign2.SetActive(true);

        var r = new Round
        {
            index = rounds.Count,
            dangerY1 = spawnPoints[index1].position.y,
            dangerY2 = spawnPoints[index2].position.y
        };
        rounds.Add(r);

        JudgeLog($"[3-10 Judge] 라운드{r.index} 경고 song={SongT:F3} (위험 도로 y={r.dangerY1:F1}, {r.dangerY2:F1})");

        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), signShowTime);
    }

    void Hide()
    {
        warningSign1.SetActive(false);
        warningSign2.SetActive(false);
    }

    // Spawn 처리: 장애물 생성 + 도착 시각 예약
    void SpawnObject()
    {
        Vector3 pos1 = spawnPoints[index1].position;
        Vector3 pos2 = spawnPoints[index2].position;

        pos1.x = fixedEnemyPosX;
        pos2.x = fixedEnemyPosX;

        Instantiate(enemy, pos1, Quaternion.identity);
        Instantiate(enemy, pos2, Quaternion.identity);

        // 가장 최근의 아직 Spawn 안 된 라운드
        for (int i = rounds.Count - 1; i >= 0; i--)
        {
            if (rounds[i].judgeTime < 0f && !rounds[i].done)
            {
                rounds[i].judgeTime = SongT + arrivalDelay;
                JudgeLog($"[3-10 Judge] 라운드{i} 장애물 생성 song={SongT:F3} → 도착 {rounds[i].judgeTime:F3}");
                break;
            }
        }
    }

    private void JudgeRound(Round r, string when)
    {
        r.done = true;

        float carY = car != null ? car.transform.position.y : 0f;

        bool hit =
            Mathf.Abs(carY - r.dangerY1) < laneTolerance ||
            Mathf.Abs(carY - r.dangerY2) < laneTolerance;

        if (hit) ReportManualFail();
        else ReportManualSuccess();

        JudgeLog($"[3-10 Judge] 라운드{r.index} {(hit ? "충돌 → 실패" : "회피 성공")} ({when}, 차 y={carY:F2}) song={SongT:F3}");
    }

    public override ScoreResult FinalizeScoreSession()
    {
        if (!finalized)
        {
            // 미니게임이 먼저 끝나면 지금 차 위치로 남은 라운드 판정
            for (int i = 0; i < rounds.Count; i++)
            {
                if (!rounds[i].done)
                    JudgeRound(rounds[i], "미니게임 종료");
            }

            SetRuntimeTotalNodeCount(rounds.Count);
            finalized = true;
        }

        return base.FinalizeScoreSession();
    }

    // 시범은 CarMoveRemake가 경고 표지판을 보고 안전 도로로 이동한다
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
    }
}