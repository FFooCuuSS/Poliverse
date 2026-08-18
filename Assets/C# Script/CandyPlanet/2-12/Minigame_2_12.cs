using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// CSV 타임라인 (6초 고정):
///   0~2s : "Show" x4     -> 초코링 4개를 ringLaneOrder 순서대로 스폰. 스폰되자마자 일정 속도로
///                           계속 낙하하다가 화면 밖 트리거(ChocoRingOffscreenCleanup)에 닿으면 자동 삭제됨.
///                           (정지 표시가 아니라 "빠르게 흘러 나가는" 프리뷰 연출)
///   2s   : "CameraDown"  -> 카메라가 아래로 이동, 완료되면 접시 입력 활성화
///   ~4~6s: "Cue" x4      -> ringLaneOrder 순서 그대로 새 초코링을 catchSpawnRow에서 스폰해 그릇으로 낙하시킴.
///                           반드시 대응하는 "Input" 이벤트 시각보다 catchFallDuration만큼 "먼저" 와야 함.
///                           (예: Input이 4.5초면 Cue는 4.5 - catchFallDuration 초에 위치)
///                           이렇게 해야 낙하가 끝나 그릇에 도착하는 시점 == RhythmManager가 실제로
///                           판정하는 Input 시각이 되어 시각적 낙하와 타이밍 판정이 정확히 일치함.
///   4~6s : "Input" x4    -> Minigame_2_12는 이 이벤트에 별도 반응하지 않음 (스폰은 위 Cue가 담당).
///                           RhythmManager가 이 시각을 기준으로 실제 플레이어 입력 타이밍(Perfect/Good/Miss)을
///                           판정하고, 입력이 없으면 자동 Miss 처리함.
///                           실제 "받았다"는 신호(OnPlayerInput)는 접시가 해당 레인에 있을 때
///                           트리거 충돌이 발생해야만 보냄 -> 레인이 틀리면 판정 윈도우가 닫혀 자동 Miss.
///   (CSV에 "CameraUp"을 안 넣어도 됨 - 마지막 캐치가 끝나면 코드가 자동으로 카메라를 원위치로 되돌린 뒤 결과를 판정함)
/// CSV에는 시간과 액션 문자열만 있으면 되고, 레인 정보는 CSV에 넣지 않고
/// ringLaneOrder 배열(인스펙터)로 미니게임 안에서 관리합니다.
/// laneAnchors는 PlateController(Bowl)에 있는 걸 plate.LaneAnchors로 그대로 참조합니다.
/// </summary>
public class Minigame_2_12 : MiniGameBase
{
    // 판정 범위 오버라이드
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;
    protected override float TimerDuration => 6f;
    protected override string MinigameExplain => "떨어지는 순서를 기억하고 접시로 받으세요!";

    [Header("레인 / 초코링 공통")]
    [Tooltip("초코링이 표시/낙하되는 순서. 각 값은 PlateController.LaneAnchors의 인덱스 (0 ~ laneCount-1)")]
    [SerializeField] private int[] ringLaneOrder = new int[4];
    [SerializeField] private GameObject ringPrefab;

    [Header("프리뷰 낙하 (Show, 0~2초)")]
    [SerializeField] private Transform spawnRow; // Show 단계 스폰 위치 (화면 상단)
    [Tooltip("Show 단계에서 화면 밖으로 흘러나가는 속도 (초당 이동 거리)")]
    [SerializeField] private float previewFallSpeed = 8f;

    [Header("캐치 낙하 (Input, 4~6초)")]
    [SerializeField] private Transform catchSpawnRow; // 카메라 이동 완료 후 기준, 캐치용 링이 새로 스폰되는 상단 위치
    [SerializeField] private Transform bowlRow;        // 낙하 도착 지점 (그릇 위치)
    [Tooltip("Input 이벤트 이후 실제 캐치 낙하 애니메이션 시간 - Show 단계와 무관하게 자유롭게 튜닝")]
    [SerializeField] private float catchFallDuration = 0.8f;

    [Header("접시")]
    [SerializeField] private PlateController plate;

    [Header("카메라 연출")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("아래로 이동할 때의 상대 오프셋 (originalCameraPosition 기준)")]
    [SerializeField] private Vector3 cameraDownOffset = new Vector3(0f, -15f, 0f);
    [SerializeField] private float cameraMoveDuration = 2f;

    private Vector3 originalCameraPosition;

    public static Minigame_2_12 Instance { get; private set; }

    private bool ended;
    private bool resultPending; // 마지막 캐치 완료 후 카메라업 애니메이션이 진행 중인지 (중복 트리거 방지)
    public int missCount = 0;

    private int shownCount;  // Show 이벤트 처리 횟수
    private int dropCount;   // Input 이벤트 처리 횟수
    private int fallsInProgress; // 아직 캐치 낙하 애니메이션이 끝나지 않은 링 개수

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
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

        // 이전 플레이에서 남아있을 수 있는 초코링(프리뷰/캐치 모두) 정리
        foreach (var marker in FindObjectsOfType<ChocoRingMarker>())
            if (marker != null) Destroy(marker.gameObject);

        cameraTransform.DOKill(); // 혹시 남아있는 이전 트윈 정리
        originalCameraPosition = cameraTransform.position;
        plate.SetInputEnabled(false);
    }

    public void Succeed()
    {
        ended = true;
        RestoreCamera();
        Success();
    }

    public void Failure()
    {
        ended = true;
        RestoreCamera();
        Fail();
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
                // Minigame_2_12는 반응 안 함 - RhythmManager가 자체적으로 이 시각 기준 판정/자동미스 처리.
                // (스폰은 이보다 먼저 온 "Cue" 이벤트가 이미 담당)
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
            Debug.LogError($"[Minigame_2_12] laneAnchors 범위 초과: lane={lane}, " +
                           $"laneAnchors.Length={(laneAnchors == null ? 0 : laneAnchors.Length)}. " +
                           $"PlateController(Bowl)의 Lane Anchors 배열 크기/할당을 확인하세요.");
            return null;
        }

        return laneAnchors[lane];
    }

    /// <summary>Show 이벤트: 순서대로 다음 프리뷰 링을 스폰, 스폰 즉시 일정 속도로 계속 낙하시킴</summary>
    private void SpawnPreviewRing()
    {
        if (shownCount >= ringLaneOrder.Length)
        {
            Debug.LogWarning("[Minigame_2_12] ringLaneOrder 길이보다 Show 이벤트가 더 많이 들어옴");
            return;
        }

        int lane = ringLaneOrder[shownCount];
        Transform anchor = GetLaneAnchor(lane);
        if (anchor == null) { shownCount++; return; }

        Vector3 pos = new Vector3(anchor.position.x, spawnRow.position.y, 0f);

        // anchor와 같은 부모(Canvas 등) 아래에 생성 - UI RectTransform이어도 좌표계가 깨지지 않도록
        GameObject ring = Instantiate(ringPrefab, pos, Quaternion.identity, anchor.parent);

        var marker = ring.AddComponent<ChocoRingMarker>();
        marker.lane = lane;
        marker.orderIndex = shownCount;

        // 트리거 감지를 위해 Kinematic Rigidbody2D 필요 (없으면 추가)
        var rb = ring.GetComponent<Rigidbody2D>();
        if (rb == null) rb = ring.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var faller = ring.AddComponent<ChocoRingFaller>();
        faller.speed = previewFallSpeed;

        Debug.Log($"[Minigame_2_12][진단] 프리뷰 스폰 - shownCount={shownCount}, lane={lane}, pos={ring.transform.position}");
        shownCount++;
    }

    private void MoveCameraDown()
    {
        Vector3 target = originalCameraPosition + cameraDownOffset;
        cameraTransform.DOKill();
        cameraTransform.DOMove(target, cameraMoveDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => plate.SetInputEnabled(true));
    }

    private void MoveCameraUp(System.Action onComplete = null)
    {
        plate.SetInputEnabled(false);
        cameraTransform.DOKill();
        cameraTransform.DOMove(originalCameraPosition, cameraMoveDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>카메라가 확실히 원래 위치로 돌아와 있도록 보장 (성공/실패/중단 시 안전장치)</summary>
    private void RestoreCamera()
    {
        if (cameraTransform == null) return;
        cameraTransform.DOKill();
        cameraTransform.position = originalCameraPosition;
    }

    /// <summary>
    /// Cue 이벤트(캐치 스폰 큐): ringLaneOrder 순서 그대로 "새로운" 캐치용 링을 catchSpawnRow에서 스폰해서
    /// bowlRow까지 catchFallDuration 동안 낙하시킴. Show 단계 프리뷰와는 완전히 다른 오브젝트/시간이고,
    /// 대응하는 Input 이벤트보다 catchFallDuration만큼 먼저 호출되도록 CSV에서 타이밍을 맞춰야 함.
    /// </summary>
    private void DropCatchRing()
    {
        if (dropCount >= ringLaneOrder.Length)
        {
            Debug.LogWarning("[Minigame_2_12] ringLaneOrder 길이보다 Cue 이벤트가 더 많이 들어옴");
            return;
        }

        int lane = ringLaneOrder[dropCount];
        Transform anchor = GetLaneAnchor(lane);
        if (anchor == null) { dropCount++; return; }

        Vector3 startPos = new Vector3(anchor.position.x, catchSpawnRow.position.y, 0f);
        Vector3 targetPos = new Vector3(anchor.position.x, bowlRow.position.y, 0f);

        Debug.Log($"[Minigame_2_12][진단] Cue 낙하 - catchSpawnRow.y={catchSpawnRow.position.y}, bowlRow.y={bowlRow.position.y} " +
                  $"({(catchSpawnRow.position.y > bowlRow.position.y ? "정상: 아래로 낙하함" : "⚠ catchSpawnRow가 bowlRow보다 낮음 -> 위로 이동하게 됨")})");

        GameObject ring = Instantiate(ringPrefab, startPos, Quaternion.identity, anchor.parent);
        var marker = ring.AddComponent<ChocoRingMarker>();
        marker.lane = lane;
        marker.orderIndex = dropCount;
        // ChocoRingFaller는 붙이지 않음 -> 화면밖 트리거는 이 링을 프리뷰로 취급하지 않고 무시함

        fallsInProgress++;
        dropCount++;

        ring.transform.DOMove(targetPos, catchFallDuration).SetEase(Ease.InQuad).OnComplete(() =>
        {
            // 여기 도달했다는 건 HandleCatchAttempt에서 캐치되지 않았다는 뜻
            // (캐치되면 그 즉시 Destroy되어 이 콜백이 실행되지 않음)
            if (ring != null) Destroy(ring);
            OnFallFinished();
        });
    }

    /// <summary>
    /// 접시(PlateCatchTrigger)에서 호출.
    /// 레인이 일치할 때만 실제 입력으로 인정해서 RhythmManager에게 판정을 넘긴다.
    /// 레인이 다르면 무시하고 그대로 낙하를 계속 진행시켜, 결국 Input 판정 윈도우가 닫히며 자동 Miss 처리되게 한다.
    /// </summary>
    public void HandleCatchAttempt(GameObject ring, ChocoRingMarker marker)
    {
        if (ended || marker.caught) return;
        if (marker.lane != plate.CurrentLane) return;

        marker.caught = true;
        OnPlayerInput(); // MiniGameBase -> RhythmManager 표준 흐름 (타이밍 기준 Perfect/Good/Miss는 RhythmManager가 판정)
        Destroy(ring);
        OnFallFinished();
    }

    private void OnFallFinished()
    {
        fallsInProgress--;
        if (fallsInProgress <= 0 && dropCount >= ringLaneOrder.Length && !resultPending)
        {
            resultPending = true;
            Debug.Log("[Minigame_2_12] 마지막 캐치 완료 - 카메라 복귀 연출 후 결과 판정");
            MoveCameraUp(CheckGameResult);
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        // 입력 잠금 상태면 무시
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

    /// <summary>
    /// 외부 시스템(전체 미니게임 진행 관리자)이 이 미니게임을 강제로 넘길 때 호출.
    /// CSV의 남은 라운드와 무관하게, 지금까지 쌓인 결과로 즉시 마무리 처리한다.
    /// </summary>
    public void ForceComplete()
    {
        if (ended) return;
        Debug.Log("[Minigame_2_12] 외부 시스템에 의해 강제 종료됨");
        CheckGameResult();
    }

    public void CheckGameResult()
    {
        if (IsInputLocked || ended) return;
        ended = true;

        if (missCount >= 3)
        {
            Debug.Log("실패");
            Failure();
        }
        else
        {
            Debug.Log("성공");
            Succeed();
        }
    }
}

/// <summary>스폰된 초코링 오브젝트에 붙는 메타데이터 (레인, 순서, 캐치 여부)</summary>
public class ChocoRingMarker : MonoBehaviour
{
    public int lane;
    public int orderIndex;
    public bool caught;
}