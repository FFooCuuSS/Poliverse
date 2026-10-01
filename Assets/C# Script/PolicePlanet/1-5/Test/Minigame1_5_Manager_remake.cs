using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Minigame1_5_Manager_remake : MiniGameBase
{
    private float prevHandX;
    [System.Serializable]
    public class CaseMoveData
    {
        public Vector2 startPos;
        public Vector2 endPos;
        public float moveTime = 2f;
        public float waitTime = 1.5f;
    }

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
    [SerializeField] private float successRangeX = 0.5f;

    private readonly int[] roundCaseOrder = { 1, 2, 1, 2 };

    private Transform[] case1Targets;
    private Transform[] case2Targets;

    private int currentRoundIndex = -1;
    private int currentCaseNum = 0;

    private bool acceptInput = false;
    private bool gameEnded = false;
    private bool gameLoopStarted = false;

    private HashSet<int> currentRoundHitIndices = new HashSet<int>();

    public int totalSuccessCount { get; private set; }

    private int[] roundSuccessCounts = new int[4];
    private Coroutine roundLoopCoroutine;

    protected override float TimerDuration => 16f;

    protected override string MinigameTitle => "숨은 범인을 찾아라";
    protected override string MinigameExplain => "오른쪽에서 줄이 죄수위로 지나갈때 화면을 터치해주세요.";

    public override void StartGame()
    {
        base.StartGame();
        if (mainCam == null) mainCam = Camera.main;

        CacheTargets();
        ResetState();

        if (roundLoopCoroutine != null)
        {
            StopCoroutine(roundLoopCoroutine);
            roundLoopCoroutine = null;
        }

        if (!gameLoopStarted)
        {
            gameLoopStarted = true;
            roundLoopCoroutine = StartCoroutine(RoundLoop());
        }
    }

    private void Update()
    {
        if (gameEnded) return;
        if (!acceptInput) return;
        if (IsDemoMode)
        {
            AutoPlayTick();
            return; // 시범 중엔 실제 클릭 무시
        }
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }
    private int autoRoundIndex = -1;
    private void AutoPlayTick()
    {
        if (hand == null) return;

        float handX = hand.position.x;

        // 라운드가 바뀐 첫 프레임은 위치만 기록하고 판정 안 함
        if (autoRoundIndex != currentRoundIndex)
        {
            autoRoundIndex = currentRoundIndex;
            prevHandX = handX;
            return;
        }

        // 손이 안 움직였으면(대기 시간) 판정 안 함
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
    private void ResetState()
    {
        totalSuccessCount = 0;
        currentRoundIndex = -1;
        currentCaseNum = 0;
        autoRoundIndex = -1;
        acceptInput = false;
        gameEnded = false;
        gameLoopStarted = false;
        currentRoundHitIndices.Clear();

        for (int i = 0; i < roundSuccessCounts.Length; i++)
            roundSuccessCounts[i] = 0;

        ResetAllCaseVisuals();

        if (case1_Obj != null) case1_Obj.SetActive(false);
        if (case2_Obj != null) case2_Obj.SetActive(false);

        if (hand != null)
            hand.position = Vector3.zero;
    }

    private void ResetAllCaseVisuals()
    {
        ResetPrisonerArray(case1Prisoners);
        ResetPrisonerArray(case2Prisoners);
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

    private Transform[] GetChildren(Transform root)
    {
        if (root == null) return new Transform[0];

        List<Transform> list = new List<Transform>();
        for (int i = 0; i < root.childCount; i++)
        {
            list.Add(root.GetChild(i));
        }
        return list.ToArray();
    }

    private IEnumerator RoundLoop()
    {
        for (int i = 0; i < roundCaseOrder.Length; i++)
        {
            currentRoundIndex = i;
            currentCaseNum = roundCaseOrder[i];
            currentRoundHitIndices.Clear();

            SetCaseVisible(currentCaseNum);
            ResetCurrentCaseVisuals();

            CaseMoveData moveData = GetCurrentMoveData();
            if (moveData == null)
            {
                Debug.LogError("[Minigame1_5] moveData가 없음");
                roundLoopCoroutine = null;
                yield break;
            }

            hand.position = moveData.startPos;

            //Debug.Log($"[Minigame1_5] Round {i + 1} START / Case {currentCaseNum}");
            DebugCurrentCaseTargetPositions();

            acceptInput = true;
            yield return new WaitForSeconds(moveData.waitTime);

            yield return StartCoroutine(MoveHand(moveData.startPos, moveData.endPos, moveData.moveTime));

            acceptInput = false;

            float remain = Mathf.Max(0f, roundCycleTime - moveData.waitTime - moveData.moveTime);
            if (remain > 0f)
                yield return new WaitForSeconds(remain);

            //Debug.Log($"[Minigame1_5] Round {i + 1} END / RoundSuccess = {roundSuccessCounts[i]} / TotalSuccess = {totalSuccessCount}");
        }

        EndByRule();
        roundLoopCoroutine = null;
    }

    private void ResetCurrentCaseVisuals()
    {
        PrisonerVisual[] prisoners = GetCurrentPrisoners();

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
    private IEnumerator MoveHand(Vector2 startPos, Vector2 endPos, float moveTime)
    {
        float elapsed = 0f;
        hand.position = startPos;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveTime);
            hand.position = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        hand.position = endPos;
    }
    private bool TryHitTarget(int i)
    {
        Transform[] targets = GetCurrentTargets();
        PrisonerVisual[] prisoners = GetCurrentPrisoners();

        if (targets == null || i < 0 || i >= targets.Length) return false;
        if (currentRoundHitIndices.Contains(i)) return false;

        currentRoundHitIndices.Add(i);
        totalSuccessCount++;
        roundSuccessCounts[currentRoundIndex]++;
        ReportManualSuccess();

        if (prisoners != null && i < prisoners.Length && prisoners[i] != null)
            prisoners[i].PlayHit();

        Debug.Log($"[1-5] HIT R={currentRoundIndex} Case={currentCaseNum} Idx={i} " +
                  $"HandX={hand.position.x:F2} TargetX={targets[i].position.x:F2}");
        return true;
    }

    private void HandleClick()
    {
        if (hand == null) return;

        float handX = hand.position.x;
        Transform[] targets = GetCurrentTargets();

        // 범위 안에서 "가장 가까운" 죄수를 친다
        int best = -1;
        float bestDiff = successRangeX;

        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null || !targets[i].gameObject.activeInHierarchy) continue;
                if (currentRoundHitIndices.Contains(i)) continue;

                float diff = Mathf.Abs(handX - targets[i].position.x);
                if (diff <= bestDiff)
                {
                    bestDiff = diff;
                    best = i;
                }
            }
        }

        if (best >= 0) TryHitTarget(best);

        SpawnClickEffect(new Vector3(handX, -1.5f, 0f));
    }

    private void SpawnClickEffect(Vector3 pos)
    {
        if (clickEffectPrefab == null) return;

        if (mainParent != null)
            Instantiate(clickEffectPrefab, pos, Quaternion.identity, mainParent);
        else
            Instantiate(clickEffectPrefab, pos, Quaternion.identity);
    }

    private void CheckCase1Success(float handX)
    {
        if (case1Targets == null || case1Targets.Length < 4)
        {
            Debug.LogWarning("[Minigame1_5] case1 target 부족");
            return;
        }

        for (int i = 0; i < case1Prisoners.Length; i++)
        {
            if (currentRoundHitIndices.Contains(i))
                continue;

            float targetX = case1Targets[i].position.x;
            float diff = Mathf.Abs(handX - targetX);

            if (diff <= successRangeX)
            {
                currentRoundHitIndices.Add(i);
                totalSuccessCount++;
                roundSuccessCounts[currentRoundIndex]++;

                ReportManualSuccess();

                if (case1Prisoners != null && i < case1Prisoners.Length && case1Prisoners[i] != null)
                {
                    case1Prisoners[i].PlayHit();
                }

                //Debug.Log($"[Minigame1_5] CASE1 SUCCESS | TargetIndex={i}, HandX={handX:F2}, TargetX={targetX:F2}, Diff={diff:F2}, TotalSuccess={totalSuccessCount}");
                return;
            }
        }

        //Debug.Log($"[Minigame1_5] CASE1 MISS | HandX={handX:F2}");
    }

    private void CheckCase2Success(float handX)
    {
        if (case2Targets == null || case2Targets.Length < 7)
        {
            Debug.LogWarning("[Minigame1_5] case2 target 부족");
            return;
        }

        for (int i = 0; i < case2Prisoners.Length; i++)
        {
            if (currentRoundHitIndices.Contains(i))
                continue;

            float targetX = case2Targets[i].position.x;
            float diff = Mathf.Abs(handX - targetX);

            if (diff <= successRangeX)
            {
                currentRoundHitIndices.Add(i);
                totalSuccessCount++;
                roundSuccessCounts[currentRoundIndex]++;

                ReportManualSuccess();

                if (case2Prisoners != null && i < case2Prisoners.Length && case2Prisoners[i] != null)
                    case2Prisoners[i].PlayHit();

                //Debug.Log($"[Minigame1_5] CASE2 SUCCESS | TargetIndex={i}, HandX={handX:F2}, TargetX={targetX:F2}, Diff={diff:F2}, TotalSuccess={totalSuccessCount}");
                return;
            }
        }

        //Debug.Log($"[Minigame1_5] CASE2 MISS | HandX={handX:F2}");
    }

    private void DebugCurrentCaseTargetPositions()
    {
        Transform[] targets = GetCurrentTargets();

        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning($"[Minigame1_5] Case {currentCaseNum} target 없음");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append($"[Minigame1_5] Case {currentCaseNum} target x positions: ");

        for (int i = 0; i < targets.Length; i++)
        {
            sb.Append($"[{i}] {targets[i].position.x:F2}");

            if (i < targets.Length - 1)
                sb.Append(" / ");
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

        Debug.Log($"[Minigame1_5] GAME END | TotalSuccess={totalSuccessCount}");

        DebugCurrentCaseTargetPositions();
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
}