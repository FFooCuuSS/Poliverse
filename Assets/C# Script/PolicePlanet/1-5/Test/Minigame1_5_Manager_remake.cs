using System.Collections;
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

    [Header("References")]
    [SerializeField] private Transform hand;
    [SerializeField] private Camera mainCam;
    [SerializeField] private Transform mainParent;

    [Header("Case Objects")]
    [SerializeField] private GameObject case1_Obj;
    [SerializeField] private GameObject case2_Obj;

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
    protected override string MinigameExplain => "숨은 죄수를 찾아라!";

    private void Start()
    {
        //StartGame();
    }

    public override void StartGame()
    {
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

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void ResetState()
    {
        totalSuccessCount = 0;
        currentRoundIndex = -1;
        currentCaseNum = 0;
        acceptInput = false;
        gameEnded = false;
        gameLoopStarted = false;
        currentRoundHitIndices.Clear();

        for (int i = 0; i < roundSuccessCounts.Length; i++)
            roundSuccessCounts[i] = 0;

        if (case1_Obj != null) case1_Obj.SetActive(false);
        if (case2_Obj != null) case2_Obj.SetActive(false);

        if (hand != null)
            hand.position = Vector3.zero;
    }

    private void CacheTargets()
    {
        case1Targets = GetChildren(case1_Obj != null ? case1_Obj.transform : null);
        case2Targets = GetChildren(case2_Obj != null ? case2_Obj.transform : null);

        Debug.Log($"[Minigame1_5] case1 target count = {case1Targets.Length}, case2 target count = {case2Targets.Length}");
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

            CaseMoveData moveData = GetCurrentMoveData();
            if (moveData == null)
            {
                Debug.LogError("[Minigame1_5] moveData가 없음");
                roundLoopCoroutine = null;
                yield break;
            }

            hand.position = moveData.startPos;

            Debug.Log($"[Minigame1_5] Round {i + 1} START / Case {currentCaseNum}");
            DebugCurrentCaseTargetPositions();

            acceptInput = true;
            yield return new WaitForSeconds(moveData.waitTime);

            yield return StartCoroutine(MoveHand(moveData.startPos, moveData.endPos, moveData.moveTime));

            acceptInput = false;

            float remain = Mathf.Max(0f, roundCycleTime - moveData.waitTime - moveData.moveTime);
            if (remain > 0f)
                yield return new WaitForSeconds(remain);

            Debug.Log($"[Minigame1_5] Round {i + 1} END / RoundSuccess = {roundSuccessCounts[i]} / TotalSuccess = {totalSuccessCount}");
        }

        EndByRule();
        roundLoopCoroutine = null;
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

    private void HandleClick()
    {
        if (hand == null)
        {
            Debug.LogError("[Minigame1_5] hand 없음");
            return;
        }

        float handX = hand.position.x;

        switch (currentCaseNum)
        {
            case 1:
                CheckCase1Success(handX);
                break;

            case 2:
                CheckCase2Success(handX);
                break;

            default:
                Debug.LogWarning($"[Minigame1_5] 알 수 없는 caseNum: {currentCaseNum}");
                break;
        }

        Vector3 effectPos = new Vector3(handX, -1.5f, 0f);
        SpawnClickEffect(effectPos);
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

        for (int i = 0; i < 4; i++)
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

                Debug.Log($"[Minigame1_5] CASE1 SUCCESS | TargetIndex={i}, HandX={handX:F2}, TargetX={targetX:F2}, Diff={diff:F2}, TotalSuccess={totalSuccessCount}");
                return;
            }
        }

        Debug.Log($"[Minigame1_5] CASE1 MISS | HandX={handX:F2}");
    }

    private void CheckCase2Success(float handX)
    {
        if (case2Targets == null || case2Targets.Length < 3)
        {
            Debug.LogWarning("[Minigame1_5] case2 target 부족");
            return;
        }

        for (int i = 0; i < 3; i++)
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

                Debug.Log($"[Minigame1_5] CASE2 SUCCESS | TargetIndex={i}, HandX={handX:F2}, TargetX={targetX:F2}, Diff={diff:F2}, TotalSuccess={totalSuccessCount}");
                return;
            }
        }

        Debug.Log($"[Minigame1_5] CASE2 MISS | HandX={handX:F2}");
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
}