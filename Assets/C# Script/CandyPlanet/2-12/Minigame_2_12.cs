using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Minigame_2_12 : MiniGameBase, IPracticeDemoInput
{
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;
    protected override float TimerDuration => timerDurationOverride;

    protected override string MinigameTitle => "초코링 받기";
    protected override string MinigameExplain => "떨어지는 순서를 기억하고 접시로 받으세요.";

    [System.Serializable]
    public class RingLaneOrderSet
    {
        [Tooltip("이 세트에서 초코링이 표시/낙하되는 레인 순서. 각 값은 PlateController.LaneAnchors의 인덱스 (0 ~ laneCount-1)")]
        public int[] lanes = new int[4];
    }

    [Header("레인 / 초코링 공통 (세트별)")]

    [Header("효과음")]
    [SerializeField] private AudioClip fallingSound;

    [SerializeField]
    private List<RingLaneOrderSet> ringLaneOrderSets =
        new List<RingLaneOrderSet> { new RingLaneOrderSet() };

    [SerializeField] private GameObject ringPrefab;

    [Header("타이머")]
    [SerializeField] private float timerDurationOverride = 15f;

    [Header("프리뷰 낙하 (Show)")]
    [SerializeField] private Transform spawnRow;
    [SerializeField] private float previewFallSpeed = 8f;

    [Header("캐치 낙하 (Input)")]
    [SerializeField] private Transform catchSpawnRow;
    [SerializeField] private Transform bowlRow;
    [SerializeField] private float catchFallDuration = 0.8f;

    [Header("접시")]
    [SerializeField] private PlateController plate;

    [Header("카메라 연출")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 cameraDownOffset = new Vector3(0f, -15f, 0f);
    [SerializeField] private float cameraMoveDuration = 2f;

    private Vector3 originalCameraPosition;

    public static Minigame_2_12 Instance { get; private set; }

    private bool ended;
    private bool resultPending;
    public int missCount = 0;

    private int shownCount;
    private int dropCount;
    private int fallsInProgress;

    private int TotalRingCount => ringLaneOrderSets.Sum(s => s.lanes.Length);

    private bool isDemoMode = false;
    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log($"[2-12] SetDemoMode = {isDemoMode}");
    }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;

        if (cameraTransform == null)
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogError($"[Minigame_2_12] cameraTransform이 비어있고 Camera.main도 찾을 수 없습니다.");
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        RestoreCamera();
    }

    public override void StartGame()
    {
        base.StartGame();
        ended = false;
        resultPending = false;
        missCount = 0;
        shownCount = 0;
        dropCount = 0;
        fallsInProgress = 0;

        foreach (var marker in FindObjectsOfType<ChocoRingMarker>())
            if (marker != null) Destroy(marker.gameObject);

        cameraTransform.DOKill();
        originalCameraPosition = cameraTransform.position;
        plate.SetInputEnabled(false);
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        Debug.Log($"{gameObject.name} 리듬메세지: {action} (frame={Time.frameCount})");
        action = action.Trim();

        switch (action)
        {
            case "Show":
                SpawnPreviewRing();
                break;

            case "Cue":
                DropCatchRing();
                break;

            case "CameraDown":
                MoveCameraDown();
                break;

            case "Input":
                break;

            case "CameraUp":
                MoveCameraUp();
                break;
        }
    }

    private Transform GetLaneAnchor(int lane)
    {
        Transform[] laneAnchors = plate.LaneAnchors;

        if (laneAnchors == null || lane < 0 || lane >= laneAnchors.Length)
        {
            Debug.LogError($"[Minigame_2_12] laneAnchors 범위 초과: lane={lane}");
            return null;
        }

        return laneAnchors[lane];
    }

    private int GetLaneForIndex(int index)
    {
        int remaining = index;

        foreach (var set in ringLaneOrderSets)
        {
            if (remaining < set.lanes.Length)
                return set.lanes[remaining];

            remaining -= set.lanes.Length;
        }
        return 0;
    }

    private void SpawnPreviewRing()
    {
        if (shownCount >= TotalRingCount) return;


        int lane = GetLaneForIndex(shownCount);
        Transform anchor = GetLaneAnchor(lane);
        if (anchor == null) { shownCount++; return; }

        Vector3 pos = new Vector3(anchor.position.x, spawnRow.position.y, 0f);
        GameObject ring = Instantiate(ringPrefab, pos, Quaternion.identity, anchor.parent);

        var marker = ring.AddComponent<ChocoRingMarker>();
        marker.lane = lane;
        marker.orderIndex = shownCount;

        var rb = ring.GetComponent<Rigidbody2D>();
        if (rb == null) rb = ring.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var faller = ring.AddComponent<ChocoRingFaller>();
        faller.speed = previewFallSpeed;
        if (fallingSound != null)
        {
            GameRoot.Instance.Audio.PlaySfx(fallingSound);
        }

        shownCount++;
    }

    private void MoveCameraDown()
    {
        Vector3 target = originalCameraPosition + cameraDownOffset;
        cameraTransform.DOKill();
        cameraTransform.DOMove(target, cameraMoveDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                plate.SetInputEnabled(!IsDemoMode);
            });
    }

    private void MoveCameraUp(System.Action onComplete = null)
    {
        plate.SetInputEnabled(false);
        cameraTransform.DOKill();
        cameraTransform.DOMove(originalCameraPosition, cameraMoveDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => onComplete?.Invoke());
    }

    private void RestoreCamera()
    {
        if (cameraTransform == null) return;
        cameraTransform.DOKill();
        cameraTransform.position = originalCameraPosition;
    }

    private void DropCatchRing()
    {
        if (dropCount >= TotalRingCount) return;

        int lane = GetLaneForIndex(dropCount);
        Transform anchor = GetLaneAnchor(lane);
        if (anchor == null) { dropCount++; return; }

        // 데모 모드일 때: Cue 시점에 해당 레인으로 즉시 이동 후,
        // 초코링이 바닥(그릇)에 닿을 때까지(catchFallDuration 동안) 그 위치를 고정 유지
        if (IsDemoMode && plate != null)
        {
            plate.transform.DOKill();
            Vector3 snapPos = new Vector3(anchor.position.x, plate.transform.position.y, plate.transform.position.z);
            plate.transform.position = snapPos;
        }

        Vector3 startPos = new Vector3(anchor.position.x, catchSpawnRow.position.y, 0f);
        Vector3 targetPos = new Vector3(anchor.position.x, bowlRow.position.y, 0f);

        GameObject ring = Instantiate(ringPrefab, startPos, Quaternion.identity, anchor.parent);
        var marker = ring.AddComponent<ChocoRingMarker>();
        marker.lane = lane;
        marker.orderIndex = dropCount;

        // 만약 데모 모드라면 이 초코링이 닿을 때까지 그릇이 그 자리에 고정되도록 참조 전달 가능
        // (트리거 충돌이 일어나기 전에 데모 Input 시그널이 먼저 들어와 위치가 틀어지는 것을 방지)

        fallsInProgress++;
        dropCount++;

        ring.transform.DOMove(targetPos, catchFallDuration).SetEase(Ease.InQuad).OnComplete(() =>
        {
            if (ring != null) Destroy(ring);
            OnFallFinished();
        });
    }

    public void HandleCatchAttempt(GameObject ring, ChocoRingMarker marker)
    {
        if (ended || marker.caught) return;
        if (marker.lane != plate.CurrentLane) return;

        marker.caught = true;
        OnPlayerInput();
        Destroy(ring);
        OnFallFinished();
    }

    private void OnFallFinished()
    {
        fallsInProgress--;
        if (fallsInProgress <= 0 && dropCount >= TotalRingCount && !resultPending)
        {
            resultPending = true;
            Debug.Log("[Minigame_2_12] 마지막 캐치 완료 - 카메라 복귀 연출 (성공/실패 판정 없음)");

            MoveCameraUp(() => { ended = true; });
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        if (IsInputLocked) return;
        base.OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended) return;
        base.OnJudgement(judgement);

        if (judgement == JudgementResult.Miss)
        {
            missCount++;
        }
    }

    public void ForceComplete()
    {
        if (ended) return;
        ended = true;
        RestoreCamera();
    }

    // --- 시범(Demo) 모드용 액션 실행 함수 구현 ---
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (string.IsNullOrEmpty(actionType))
            return;

        actionType = actionType.Trim();

        if (actionType != "Input")
            return;

        if (ended)
            return;

        Debug.Log($"[2-12 Demo] ExecutePracticeAction 호출 - Index={actionIndex}, Type={actionType}");

        // 데모 모드의 Input 타이밍에는 이미 Cue 때 이동해 자리 잡고 있으므로
        // 불필요한 위치 재조정 없이 바로 성공 입력 신호만 리듬 매니저로 전달
        if (rhythmManager != null)
        {
            rhythmManager.ReceivePlayerInput("Input");
        }
        else
        {
            base.OnPlayerInput("Input");
        }
    }
}

public class ChocoRingMarker : MonoBehaviour
{
    public int lane;
    public int orderIndex;
    public bool caught;
}