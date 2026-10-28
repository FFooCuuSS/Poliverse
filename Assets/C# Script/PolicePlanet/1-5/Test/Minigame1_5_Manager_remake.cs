using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Minigame1_5_Manager_remake : MiniGameBase
{
    [System.Serializable]
    public class CaseMoveData
    {
        public Vector2 startPos;
        public Vector2 endPos;
        public float moveTime = 2f;
        public float waitTime = 1.5f;
    }

    [Header("효과음")]
    [SerializeField] private AudioClip hitSound;

    [Header("References")]
    [SerializeField] private Transform hand;
    [SerializeField] private Camera mainCam;
    [SerializeField] private Transform mainParent;

    [Header("Case Objects")]
    [SerializeField] private GameObject case1_Obj;
    [SerializeField] private GameObject case2_Obj;
    [SerializeField] private PrisonerVisual[] case1Prisoners;
    [SerializeField] private PrisonerVisual[] case2Prisoners;

    [Header("Click Effect")]
    [SerializeField] private GameObject clickEffectPrefab;

    [Header("Case Move Settings")]
    [SerializeField] private CaseMoveData case1Move;
    [SerializeField] private CaseMoveData case2Move;

    [Header("Timing")]
    [SerializeField] private float roundCycleTime = 4f;
    [Tooltip("(구버전) 거리 기준 판정 범위. 지금은 judgeWindowSeconds를 사용")]
    [SerializeField] private float successRangeX = 0.6f;
    [Tooltip("줄이 죄수 위를 지나는 순간 기준 ±이 시간 안에 터치하면 성공")]
    [SerializeField] private float judgeWindowSeconds = 0.15f;

    [Header("Debug")]
    [SerializeField] private bool judgeLog = true;

    private readonly int[] roundCaseOrder = { 1, 2, 1, 2 };

    private Transform[] case1Targets;
    private Transform[] case2Targets;

    private int currentRoundIndex = -1;
    private int currentCaseNum = 0;

    private bool running = false;
    private bool gameEnded = false;
    private double localStartTime;

    private readonly HashSet<int> currentRoundHitIndices = new HashSet<int>();

    public int totalSuccessCount { get; private set; }

    private readonly int[] roundSuccessCounts = new int[4];

    // 데모 자동 입력용
    private int autoRoundIndex = -1;
    private float prevHandX;

    protected override float TimerDuration => 16f;

    protected override string MinigameTitle => "숨은 범인을 찾아라";
    protected override string MinigameExplain => "오른쪽에서 줄이 죄수위로 지나갈때 화면을 터치해주세요.";

    // ───────── 시간: 노래 시간 기준 (없으면 로컬 시간) ─────────
    private double Now
    {
        get
        {
            if (rhythmManager is RhythmManager rm && rm.IsRunning)
                return rm.SongTime;

            return Time.timeAsDouble - localStartTime;
        }
    }

    private void JudgeLog(string msg)
    {
        if (judgeLog) Debug.Log(msg);
    }

    public override void StartGame()
    {
        base.StartGame();
        if (mainCam == null) mainCam = Camera.main;

        CacheTargets();
        ResetState();

        localStartTime = Time.timeAsDouble;
        running = true;
    }

    private void ResetState()
    {
        totalSuccessCount = 0;
        currentRoundIndex = -1;
        currentCaseNum = 0;
        autoRoundIndex = -1;
        gameEnded = false;
        running = false;
        currentRoundHitIndices.Clear();

        for (int i = 0; i < roundSuccessCounts.Length; i++)
            roundSuccessCounts[i] = 0;

        ResetPrisonerArray(case1Prisoners);
        ResetPrisonerArray(case2Prisoners);

        if (case1_Obj != null) case1_Obj.SetActive(false);
        if (case2_Obj != null) case2_Obj.SetActive(false);

        if (hand != null)
            hand.position = Vector3.zero;
    }

    private void Update()
    {
        if (!running || gameEnded) return;
        if (hand == null) return;

        double t = Now;
        if (t < 0) return;

        int round = (int)(t / roundCycleTime);

        if (round >= roundCaseOrder.Length)
        {
            EndByRule();
            return;
        }

        if (round != currentRoundIndex)
            EnterRound(round, t);

        CaseMoveData move = GetCurrentMoveData();
        if (move == null) return;

        float local = (float)(t - round * roundCycleTime);

        // 손 위치를 노래 시간으로 직접 계산 → 프레임/로딩 지연이 누적되지 않음
        float progress = move.moveTime > 0f
            ? Mathf.Clamp01((local - move.waitTime) / move.moveTime)
            : 1f;

        hand.position = Vector2.Lerp(move.startPos, move.endPos, progress);

        bool acceptInput = local <= move.waitTime + move.moveTime + judgeWindowSeconds;
        if (!acceptInput) return;

        if (IsDemoMode)
        {
            AutoPlayTick();
            return;
        }

        if (Input.GetMouseButtonDown(0))
            HandleClick(local, move);
    }

    private void EnterRound(int round, double t)
    {
        currentRoundIndex = round;
        currentCaseNum = roundCaseOrder[round];
        currentRoundHitIndices.Clear();

        SetCaseVisible(currentCaseNum);
        ResetPrisonerArray(GetCurrentPrisoners());

        CaseMoveData move = GetCurrentMoveData();
        if (move != null)
            hand.position = move.startPos;

        JudgeLog($"[1-5 Judge] ROUND {round + 1} 시작 Case={currentCaseNum} song={t:F3}");
        DebugCurrentCaseTargetPositions();
    }

    // 줄이 targetX를 지나는 라운드 내 시각
    private float GetPassTime(CaseMoveData move, float targetX)
    {
        float dx = move.startPos.x - move.endPos.x;
        if (Mathf.Approximately(dx, 0f)) return move.waitTime;

        float ratio = (move.startPos.x - targetX) / dx;
        return move.waitTime + move.moveTime * ratio;
    }

    private void HandleClick(float local, CaseMoveData move)
    {
        Transform[] targets = GetCurrentTargets();

        int best = -1;
        float bestAbs = float.MaxValue;
        float bestDiff = 0f;

        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null || !targets[i].gameObject.activeInHierarchy) continue;
                if (currentRoundHitIndices.Contains(i)) continue;

                float diff = local - GetPassTime(move, targets[i].position.x);
                float abs = Mathf.Abs(diff);

                if (abs < bestAbs)
                {
                    bestAbs = abs;
                    bestDiff = diff;
                    best = i;
                }
            }
        }

        if (best >= 0 && bestAbs <= judgeWindowSeconds)
        {
            JudgeLog($"[1-5 Judge] HIT R={currentRoundIndex + 1} idx={best} diff={bestDiff * 1000:F0}ms");
            TryHitTarget(best);
        }
        else if (best >= 0)
        {
            JudgeLog($"[1-5 Judge] MISS 클릭 R={currentRoundIndex + 1} 가장 가까운 idx={best} diff={bestDiff * 1000:F0}ms (허용 ±{judgeWindowSeconds * 1000:F0}ms)");
        }

        SpawnClickEffect(new Vector3(hand.position.x, -1.5f, 0f));
    }

    private void AutoPlayTick()
    {
        float handX = hand.position.x;

        if (autoRoundIndex != currentRoundIndex)
        {
            autoRoundIndex = currentRoundIndex;
            prevHandX = handX;
            return;
        }

        if (Mathf.Approximately(prevHandX, handX)) return;

        Transform[] targets = GetCurrentTargets();

        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null || !targets[i].gameObject.activeInHierarchy) continue;
                if (currentRoundHitIndices.Contains(i)) continue;

                float tx = targets[i].position.x;

                if ((prevHandX - tx) * (handX - tx) <= 0f)
                {
                    TryHitTarget(i);
                    SpawnClickEffect(new Vector3(tx, -1.5f, 0f));
                    break;
                }
            }
        }

        prevHandX = handX;
    }

    private bool TryHitTarget(int i)
    {
        Transform[] targets = GetCurrentTargets();
        PrisonerVisual[] prisoners = GetCurrentPrisoners();

        if (targets == null || i < 0 || i >= targets.Length) return false;
        if (currentRoundHitIndices.Contains(i)) return false;

        currentRoundHitIndices.Add(i);
        totalSuccessCount++;

        if (currentRoundIndex >= 0 && currentRoundIndex < roundSuccessCounts.Length)
            roundSuccessCounts[currentRoundIndex]++;

        ReportManualSuccess();

        if (prisoners != null && i < prisoners.Length && prisoners[i] != null)
            prisoners[i].PlayHit();

        if (hitSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            GameRoot.Instance.Audio.PlaySfx(hitSound);

        return true;
    }

    private void SpawnClickEffect(Vector3 pos)
    {
        if (clickEffectPrefab == null) return;

        if (mainParent != null)
            Instantiate(clickEffectPrefab, pos, Quaternion.identity, mainParent);
        else
            Instantiate(clickEffectPrefab, pos, Quaternion.identity);
    }

    private void CacheTargets()
    {
        case1Targets = GetPrisonerTransforms(case1Prisoners);
        case2Targets = GetPrisonerTransforms(case2Prisoners);

        Debug.Log($"[Minigame1_5] case1 target count = {case1Targets.Length}, case2 target count = {case2Targets.Length}");
    }

    private Transform[] GetPrisonerTransforms(PrisonerVisual[] prisoners)
    {
        if (prisoners == null) return new Transform[0];

        Transform[] result = new Transform[prisoners.Length];
        for (int i = 0; i < prisoners.Length; i++)
            result[i] = prisoners[i] != null ? prisoners[i].transform : null;

        return result;
    }

    private void ResetPrisonerArray(PrisonerVisual[] prisoners)
    {
        if (prisoners == null) return;

        for (int i = 0; i < prisoners.Length; i++)
        {
            if (prisoners[i] != null)
                prisoners[i].ResetVisual();
        }
    }

    private PrisonerVisual[] GetCurrentPrisoners()
    {
        switch (currentCaseNum)
        {
            case 1: return case1Prisoners;
            case 2: return case2Prisoners;
            default: return null;
        }
    }

    private void DebugCurrentCaseTargetPositions()
    {
        if (!judgeLog) return;

        Transform[] targets = GetCurrentTargets();
        CaseMoveData move = GetCurrentMoveData();

        if (targets == null || targets.Length == 0 || move == null)
        {
            Debug.LogWarning($"[Minigame1_5] Case {currentCaseNum} target 없음");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append($"[1-5 Judge] Case {currentCaseNum} 통과 시각(라운드 기준): ");

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null) continue;
            sb.Append($"[{i}] x={targets[i].position.x:F2} t={GetPassTime(move, targets[i].position.x):F3}  ");
        }

        Debug.Log(sb.ToString());
    }

    private void SetCaseVisible(int caseNum)
    {
        if (case1_Obj != null) case1_Obj.SetActive(caseNum == 1);
        if (case2_Obj != null) case2_Obj.SetActive(caseNum == 2);
    }

    private CaseMoveData GetCurrentMoveData()
    {
        switch (currentCaseNum)
        {
            case 1: return case1Move;
            case 2: return case2Move;
            default: return null;
        }
    }

    private Transform[] GetCurrentTargets()
    {
        switch (currentCaseNum)
        {
            case 1: return case1Targets;
            case 2: return case2Targets;
            default: return null;
        }
    }

    private void EndByRule()
    {
        if (gameEnded) return;
        gameEnded = true;
        running = false;

        Debug.Log($"[Minigame1_5] GAME END | TotalSuccess={totalSuccessCount}");
    }
}

    //public override void ExecutePracticeAction(int actionIndex, string actionType)
    //{
    //    if (!string.Equals(actionType, "Action", System.StringComparison.OrdinalIgnoreCase))
    //    {
    //        return;
    //    }

    //    Debug.Log($"[Minigame1_5 Demo] Action Index: {actionIndex}");

    //    // 1-5의 라운드 순서: { 1, 2, 1, 2 } (총 4라운드)
    //    // actionIndex가 어떤 라운드/케이스에 속하는지 매핑합니다.
    //    // 예를 들어 actionIndex 0,1,2... 가 각각 어느 라운드의 몇 번째 타겟인지 지정할 수 있습니다.

    //    // 예시로 actionIndex를 순서대로 라운드와 타겟 인덱스로 분기 처리합니다.
    //    int targetRound = actionIndex / 2; // 라운드 인덱스 (0, 1, 2, 3)
    //    int targetIndex = actionIndex % 2; // 해당 라운드 내의 타겟 순서

    //    if (targetRound >= roundCaseOrder.Length)
    //    {
    //        Debug.LogWarning($"[Minigame1_5 Demo] 범위를 벗어난 Action Index: {actionIndex}");
    //        return;
    //    }

    //    // 현재 라운드와 케이스 강제 동기화
    //    currentRoundIndex = targetRound;
    //    currentCaseNum = roundCaseOrder[targetRound];

    //    // 케이스 오브젝트 켜기 및 비주얼 리셋
    //    SetCaseVisible(currentCaseNum);
    //    ResetCurrentCaseVisuals();

    //    // 해당 타겟 히트 실행
    //    ExecuteHitForPractice(targetIndex);
    //}

    // 시연 모드 전용 강제 히트 헬퍼 메서드
    //private void ExecuteHitForPractice(int targetIndex)
    //{
    //    Transform[] currentTargets = GetCurrentTargets();
    //    PrisonerVisual[] currentPrisoners = GetCurrentPrisoners();

    //    if (currentTargets == null || currentPrisoners == null) return;

    //    if (targetIndex >= 0 && targetIndex < currentTargets.Length)
    //    {
    //        if (!currentRoundHitIndices.Contains(targetIndex))
    //        {
    //            currentRoundHitIndices.Add(targetIndex);
    //            totalSuccessCount++;

    //            if (currentRoundIndex >= 0 && currentRoundIndex < roundSuccessCounts.Length)
    //            {
    //                roundSuccessCounts[currentRoundIndex]++;
    //            }

    //            ReportManualSuccess();

    //            if (targetIndex < currentPrisoners.Length && currentPrisoners[targetIndex] != null)
    //            {
    //                currentPrisoners[targetIndex].PlayHit();
    //            }

    //            Vector3 effectPos = new Vector3(currentTargets[targetIndex].position.x, -1.5f, 0f);
    //            SpawnClickEffect(effectPos);

    //            Debug.Log($"[Minigame1_5 Demo] 시범 성공! Round={currentRoundIndex}, Case={currentCaseNum}, TargetIndex={targetIndex}");
    //        }
    //    }
    //}
