using UnityEngine;

// 3-8 미행
// 라운드 4번 (4.8초 간격). 박스가 오른쪽에서 왼쪽으로 오고, 범인이 뒤돌아보는 박자에 터치해서 박스 뒤에 숨는다.
//  - 범인 뒤돌아봄 = CSV 3-8 Input 노트 시각 (3.0 / 7.8 / 12.6 / 17.4)
//  - 박스는 뒤돌아보기 boxLead(2.4)초 전에 생성 → 뒤돌아보는 순간 플레이어 위치에 도착
//
// 판정(직접, 라운드마다 1점):
//  - 뒤돌아보는 순간 ±goodWindow 안에 클릭 → 숨기 성공 (플레이어 숨고 박스 잠깐 멈춤)
//  - 뒤돌아보기 직전 wrongClickRange초 안에 엉뚱하게 클릭 → 그 라운드 들킴 (Miss + '!'), 숨지 않음, 박스도 안 멈춤
//  - 그 밖의 클릭(라운드 사이, 숨은 직후 연타 등)은 무시
//  - 클릭 안 하고 지나감 → 들킴 (Miss + '!')
// 모든 시각은 노래 시간 기준. CSV Input 노트는 판정에 쓰지 않는다.
public class Minigame3_8remake : MiniGameBase, IPracticeDemoInput
{
    // ===== 미니게임 기본 정보 =====
    protected override float TimerDuration => 10f;
    protected override string MinigameTitle => "미행";
    protected override string MinigameExplain => "박스가 오른쪽에서 왼쪽으로 이동합니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "타이밍에 맞춰 화면을 터치해 박스 뒤에 숨으세요."
    };

    public override float perfectWindowOverride => 0.1f;
    public override float goodWindowOverride => 0.3f;
    public override float hitWindowOverride => 0.5f;

    protected override bool UseRhythmJudgementScore => false;
    protected override int ManualTotalNodeCount => roundCount;

    [Header("References (비우면 자동 탐색)")]
    [SerializeField] private BushSpawner3_8 spawner;
    [SerializeField] private EnemyWatch3_8 enemyWatch;
    [SerializeField] private PlayerControl3_8 player;

    [Header("박자 (노래 시간, 초) - CSV 3-8 Input과 맞출 것")]
    [SerializeField] private float firstLookTime = 3.0f;
    [SerializeField] private float roundInterval = 4.8f;
    [SerializeField] private int roundCount = 4;
    [Tooltip("범인이 뒤돌아보기 몇 초 전에 박스를 생성할지")]
    [SerializeField] private float boxLead = 2.4f;
    [SerializeField] private float lookDuration = 0.6f;

    [Header("판정 범위 (초) - 뒤돌아보는 순간 기준")]
    [SerializeField] private float perfectWindow = 0.1f;
    [SerializeField] private float goodWindow = 0.3f;
    [Tooltip("뒤돌아보기 직전 이 시간 안의 엉뚱한 클릭만 들킴 처리 (그 전 클릭은 무시)")]
    [SerializeField] private float wrongClickRange = 1.2f;

    [Header("들킴 표시 (Miss)")]
    [Tooltip("Miss일 때 잠깐 켤 '!' 텍스트 오브젝트 (범인 머리 위 등에 배치)")]
    [SerializeField] private GameObject missMark;
    [SerializeField] private float missMarkDuration = 0.6f;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    public bool finished = false;

    private bool isDemoMode = false;
    public new bool IsDemoMode => isDemoMode;

    private bool started;
    private int spawnedRounds;
    private int lookedRounds;
    private bool lookOn;
    private float localTime;
    private float missMarkHideAt = -1f;
    private bool[] resolved;

    private float SongT
    {
        get
        {
            if (rhythmManager is RhythmManager rm)
                return rm.IsRunning ? (float)rm.SongTime : -1f;

            return localTime;
        }
    }

    private float LookTime(int k) => firstLookTime + roundInterval * k;

    private void JudgeLog(string msg)
    {
        if (judgeLog) Debug.Log(msg);
    }

    public override void SetDemoMode(bool isDemo)
    {
        base.SetDemoMode(isDemo);
        isDemoMode = isDemo;
        Debug.Log($"[3-8] SetDemoMode = {isDemoMode}");
    }

    protected override void Awake()
    {
        base.Awake();

        if (spawner == null) spawner = GetComponentInChildren<BushSpawner3_8>(true);
        if (enemyWatch == null) enemyWatch = GetComponentInChildren<EnemyWatch3_8>(true);
        if (player == null) player = GetComponentInChildren<PlayerControl3_8>(true);

        if (missMark != null) missMark.SetActive(false);   // 로드 직후 깜빡임 방지
    }

    public override void StartGame()
    {
        base.StartGame();
        started = true;

        IsInputLocked = false;
        finished = false;
        spawnedRounds = 0;
        lookedRounds = 0;
        lookOn = false;
        localTime = 0f;
        resolved = new bool[Mathf.Max(1, roundCount)];
        HideMissMark();
    }

    private void Start()
    {
        // 외부에서 StartGame을 안 불러준 경우 대비
        if (!started)
            StartGame();
    }

    private void Update()
    {
        localTime += Time.deltaTime;

        if (missMarkHideAt >= 0f && Time.time >= missMarkHideAt)
            HideMissMark();

        if (finished) return;

        float t = SongT;
        if (t < 0f) return;

        // 박스 생성
        while (spawnedRounds < roundCount && t >= LookTime(spawnedRounds) - boxLead)
        {
            float spawnTime = LookTime(spawnedRounds) - boxLead;
            if (spawner != null) spawner.SpawnBush(t - spawnTime);
            JudgeLog($"[3-8 Judge] 박스{spawnedRounds} 생성 song={t:F3} (뒤돌아봄 {LookTime(spawnedRounds):F3})");
            spawnedRounds++;
        }

        // 범인 뒤돌아봄 / 원위치
        if (lookedRounds < roundCount && t >= LookTime(lookedRounds))
        {
            lookOn = true;
            if (enemyWatch != null) enemyWatch.SetLook(true);
            JudgeLog($"[3-8 Judge] 범인 뒤돌아봄 song={t:F3}");
            lookedRounds++;
        }

        if (lookOn && t >= LookTime(lookedRounds - 1) + lookDuration)
        {
            lookOn = false;
            if (enemyWatch != null) enemyWatch.SetLook(false);
        }

        // 시범: 뒤돌아보는 순간 자동 클릭
        if (isDemoMode)
        {
            for (int k = 0; k < roundCount; k++)
            {
                if (!resolved[k] && t >= LookTime(k))
                {
                    HandleClick(t);
                    break;
                }
            }
        }
        else if (!IsInputLocked && Input.GetMouseButtonDown(0))
        {
            HandleClick(t);
        }

        // 클릭 없이 지나간 라운드 → 들킴
        for (int k = 0; k < roundCount; k++)
        {
            if (!resolved[k] && t > LookTime(k) + goodWindow)
                Resolve(k, false, "클릭 안 함");
        }

        // 마지막 라운드가 끝나면 종료
        if (t > LookTime(roundCount - 1) + Mathf.Max(lookDuration, goodWindow) + 0.1f)
            finished = true;
    }

    private void HandleClick(float t)
    {
        // 가장 가까운 라운드
        int k = Mathf.Clamp(Mathf.RoundToInt((t - firstLookTime) / roundInterval), 0, roundCount - 1);
        float diff = t - LookTime(k);

        if (Mathf.Abs(diff) <= goodWindow)
        {
            if (resolved[k]) return;   // 같은 박자 연타는 무시

            string grade = Mathf.Abs(diff) <= perfectWindow ? "Perfect" : "Good";
            Resolve(k, true, $"diff={diff * 1000:F0}ms → {grade}");

            // 숨기 연출
            if (spawner != null) spawner.SimulateDemoClick();
            if (player != null) player.PlayWatchRoutine();
            return;
        }

        // 뒤돌아보기 직전(wrongClickRange 안)에 너무 일찍 누른 경우만 들킴
        for (int r = 0; r < roundCount; r++)
        {
            if (resolved[r]) continue;

            float look = LookTime(r);
            if (t >= look - wrongClickRange && t < look - goodWindow)
            {
                Resolve(r, false, $"너무 일찍 클릭 song={t:F3} ({(t - look) * 1000:F0}ms)");
                return;
            }
        }

        JudgeLog($"[3-8 Judge] 클릭 무시 song={t:F3} (판정 구간 아님)");
    }

    private void Resolve(int k, bool success, string reason)
    {
        resolved[k] = true;

        if (success)
        {
            ReportManualSuccess();
        }
        else
        {
            ReportManualFail();
            ShowMissMark();
        }

        JudgeLog($"[3-8 Judge] 라운드{k} {(success ? "숨기 성공" : "들킴")} ({reason})");
    }

    public override void OnRhythmEvent(string action)
    {
        // 연출은 노래 시간으로 직접 처리 (Input 노트는 판정용)
    }

    // 시범은 Update에서 뒤돌아보는 순간 직접 클릭한다
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        // RhythmManager 판정은 쓰지 않는다 (직접 판정)
    }

    public override ScoreResult FinalizeScoreSession()
    {
        // 중간에 끝나도 남은 라운드는 들킴 처리
        if (resolved != null)
        {
            for (int k = 0; k < resolved.Length; k++)
                if (!resolved[k]) Resolve(k, false, "미니게임 종료");
        }

        return base.FinalizeScoreSession();
    }

    private void ShowMissMark()
    {
        if (missMark == null) return;

        missMark.SetActive(true);
        missMarkHideAt = Time.time + missMarkDuration;
    }

    private void HideMissMark()
    {
        missMarkHideAt = -1f;
        if (missMark != null) missMark.SetActive(false);
    }
}