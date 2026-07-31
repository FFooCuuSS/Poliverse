using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// CSV 타임라인 (8초 고정):
///   0~2s : "Show"  x4   -> 초코링 4개를 ringLaneOrder 순서대로 스폰 (정지 상태로 표시)
///   2s   : "CameraDown" -> 카메라가 아래로 이동, 완료되면 접시 입력 활성화
///   4~6s : "Input" x4   -> 스폰된 순서 그대로 다음 초코링이 그릇으로 낙하 시작.
///                          RhythmManager가 이 Input 이벤트 기준으로 타이밍(Perfect/Good/Miss)을 판정.
///                          단, 실제 "받았다"는 신호(OnPlayerInput)는 접시가 해당 레인에 있을 때
///                          트리거 충돌이 발생해야만 보냄 -> 레인이 틀리면 자동으로 타이밍 윈도우가
///                          닫혀 Miss 처리됨 (RhythmManager 쪽 로직).
///   6s   : "CameraUp"   -> 카메라 원위치, 접시 입력 비활성화
/// CSV에는 시간과 액션 문자열만 있으면 되고, 레인 정보는 CSV에 넣지 않고
/// ringLaneOrder 배열(인스펙터)로 미니게임 안에서 관리합니다.
/// </summary>
public class Minigame_2_12 : MiniGameBase
{
    // 판정 범위 오버라이드
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;
    protected override float TimerDuration => 8f;
    protected override string MinigameExplain => "떨어지는 순서를 기억하고 접시로 받으세요!";

    [Header("레인 / 초코링")]
    [Tooltip("초코링이 표시/낙하되는 순서. 각 값은 laneAnchors의 인덱스 (0 ~ laneCount-1)")]
    [SerializeField] private int[] ringLaneOrder = new int[4];
    [SerializeField] private Transform[] laneAnchors;
    [SerializeField] private GameObject ringPrefab;
    [SerializeField] private Transform spawnRow;   // Show 단계에서 정지 표시되는 위치 (화면 상단)
    [SerializeField] private Transform bowlRow;    // 낙하 도착 지점 (그릇 위치)
    [SerializeField] private float fallDuration = 0.4f; // Input 이벤트 이후 실제 낙하 애니메이션 시간

    [Header("접시")]
    [SerializeField] private PlateController plate;

    [Header("카메라 연출")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("아래로 이동할 때의 상대 오프셋 (originalCameraPosition 기준)")]
    [SerializeField] private Vector3 cameraDownOffset = new Vector3(0f, -5f, 0f);
    [SerializeField] private float cameraMoveDuration = 2f;

    private Vector3 originalCameraPosition;

    public static Minigame_2_12 Instance { get; private set; }

    private bool ended;
    public int missCount = 0;

    private readonly List<GameObject> spawnedRings = new List<GameObject>();
    private int shownCount;  // Show 이벤트 처리 횟수
    private int dropCount;   // Input 이벤트 처리 횟수
    private int fallsInProgress; // 아직 낙하 애니메이션이 끝나지 않은 링 개수

    protected override void Awake()
    {
        base.Awake();
        Instance = this;

        // 진단용: 씬에 매니저/미니게임이 중복되어 있는지 확인 (원인 파악 후 제거해도 됩니다)
        var managers = FindObjectsOfType<RhythmManagerTest>();
        var minigames = FindObjectsOfType<Minigame_2_12>();
        Debug.Log($"[Minigame_2_12][진단] RhythmManagerTest 개수={managers.Length}, Minigame_2_12 개수={minigames.Length}");
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
        missCount = 0;
        shownCount = 0;
        dropCount = 0;
        fallsInProgress = 0;

        foreach (var ring in spawnedRings)
            if (ring != null) Destroy(ring);
        spawnedRings.Clear();

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
                SpawnNextRing();
                break;

            case "CameraDown":
                MoveCameraDown();
                break;

            case "Input":
                DropNextRing();
                break;

            case "CameraUp":
                MoveCameraUp();
                break;
        }
    }

    private void SpawnNextRing()
    {
        if (shownCount >= ringLaneOrder.Length)
        {
            Debug.LogWarning("[Minigame_2_12] ringLaneOrder 길이보다 Show 이벤트가 더 많이 들어옴");
            return;
        }

        int lane = ringLaneOrder[shownCount];

        if (laneAnchors == null || lane < 0 || lane >= laneAnchors.Length)
        {
            Debug.LogError($"[Minigame_2_12] laneAnchors 범위 초과: lane={lane}, laneAnchors.Length={(laneAnchors == null ? 0 : laneAnchors.Length)}. " +
                           $"인스펙터에서 laneAnchors 배열 크기/할당을 확인하세요.");
            shownCount++; // 카운트는 진행시켜서 이후 Show/Input 인덱스가 안 밀리게 함
            return;
        }

        Transform anchor = laneAnchors[lane];
        Vector3 pos = new Vector3(anchor.position.x, spawnRow.position.y, 0f);

        GameObject ring = Instantiate(ringPrefab, pos, Quaternion.identity);
        var marker = ring.AddComponent<ChocoRingMarker>();
        marker.lane = lane;
        marker.orderIndex = shownCount;

        spawnedRings.Add(ring);
        Debug.Log($"[Minigame_2_12][진단] 스폰됨 - shownCount={shownCount}, lane={lane}, anchor.position.x={anchor.position.x}, ring.position={ring.transform.position}");
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

    private void MoveCameraUp()
    {
        plate.SetInputEnabled(false);
        cameraTransform.DOKill();
        cameraTransform.DOMove(originalCameraPosition, cameraMoveDuration)
            .SetEase(Ease.InOutSine);
    }

    /// <summary>카메라가 확실히 원래 위치로 돌아와 있도록 보장 (성공/실패/중단 시 안전장치)</summary>
    private void RestoreCamera()
    {
        if (cameraTransform == null) return;
        cameraTransform.DOKill();
        cameraTransform.position = originalCameraPosition;
    }

    /// <summary>Input 이벤트 발생 시, 스폰된 순서대로 다음 링을 그릇을 향해 낙하시킴</summary>
    private void DropNextRing()
    {
        if (dropCount >= spawnedRings.Count)
        {
            Debug.LogWarning("[Minigame_2_12] 남은 초코링이 없는데 Input 이벤트가 더 들어옴");
            return;
        }

        GameObject ring = spawnedRings[dropCount];
        dropCount++;

        if (ring == null) return; // 이미 다른 이유로 파괴된 경우 방어

        var marker = ring.GetComponent<ChocoRingMarker>();
        Transform anchor = laneAnchors[marker.lane];
        Vector3 targetPos = new Vector3(anchor.position.x, bowlRow.position.y, 0f);

        fallsInProgress++;

        ring.transform.DOMove(targetPos, fallDuration).SetEase(Ease.InQuad).OnComplete(() =>
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
        if (fallsInProgress <= 0 && dropCount >= ringLaneOrder.Length)
        {
            CheckGameResult();
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