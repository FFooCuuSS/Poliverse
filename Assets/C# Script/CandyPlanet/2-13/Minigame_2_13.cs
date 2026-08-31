using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2-13 젤리 블록 철거 미니게임
///
/// 리듬 흐름 (2박 1세트):
///  1박) CSV type = "Show"  -> 탄환 소환 + 낙하 (RigidBody 미사용, Transform 보간)
///  1박) CSV type = "Input" -> 플레이어가 화면을 터치해야 하는 타이밍.
///                             정확한 판정(Perfect/Good/Miss)은 RhythmManager가 수행하고,
///                             그 결과가 OnJudgement()로 전달된다.
///
/// 판정 결과에 따라:
///  - Perfect / Good -> 투석기 발사 모션 + 탄환이 날아가서 현재 타겟 젤리 블록에 명중 (데미지)
///  - Miss           -> 탄환이 그대로 사라짐 (또는 실패 모션)
///
/// 점수 집계는 MiniGameBase의 UseRhythmJudgementScore(기본 true) 경로를 그대로 사용하므로
/// 이 스크립트에서 별도로 점수를 계산하지 않는다.
/// </summary>

public class Minigame_2_13 : MiniGameBase
{
    // 판정 윈도우 오버라이드
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;
    protected override string MinigameExplain => "젤리빌딩 부수기!";

    [Header("Jelly Block Demolition - Positions")]
    [Tooltip("탄환이 처음 소환되는 위치 (좌측 상단)")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("탄환이 낙하해서 도착하는 위치 (투석기 위치)")]
    [SerializeField] private Transform catapultPoint;

    [Tooltip("투석기에서 발사된 탄환이 날아가는 목표 지점 (젤리 빌딩 쪽 고정 포인트)")]
    [SerializeField] private Transform targetPoint;

    [Header("Jelly Block Demolition - Prefab & Timing")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("소환 후 투석기 위치까지 낙하하는 데 걸리는 시간(초). 1박 길이에 맞춰 조정.")]
    [SerializeField] private float fallDuration = 0.5f;

    [Tooltip("투석기에서 목표 지점까지 날아가는 데 걸리는 시간(초). 1박 길이에 맞춰 조정.")]
    [SerializeField] private float flyDuration = 0.3f;

    [Header("Catapult Procedural Motion (애니메이션 클립 없이 코드로 구현)")]
    [Tooltip(
        "회전시킬 투석기 팔 Transform.\n" +
        "★ 중요: 이 Transform의 위치(피벗)는 받침대와 팔이 연결되는 지점에 있어야 함.\n" +
        "팔 스프라이트는 그 지점에서 오프셋되어 배치 (또는 스프라이트 자체 Pivot을 연결 지점으로 설정).\n" +
        "그래야 회전 시 연결 지점을 축으로 자연스럽게 휘두름.")]
    [SerializeField] private Transform catapultArm;

    [Tooltip(
        "씬에 배치해둔 arm의 '평상시 기울어진 자세' 그대로를 Rest로 사용할지 여부.\n" +
        "true면 게임 시작 시 arm의 현재 회전값을 자동으로 Rest 각도로 저장한다.\n" +
        "false면 아래 catapultRestAngleOverride 값을 Rest로 사용한다.")]
    [SerializeField] private bool useCurrentArmRotationAsRest = true;

    [Tooltip("useCurrentArmRotationAsRest가 false일 때 사용할 Rest 각도(Z, degrees)")]
    [SerializeField] private float catapultRestAngleOverride = 0f;

    [Tooltip("Rest 각도에서 받침대 쪽으로 얼마나 더 당겨질지 (상대 오프셋, degrees). " +
             "예: -20이면 Rest보다 20도 더 눕는(당겨지는) 방향.")]
    [SerializeField] private float catapultPullBackOffset = -20f;

    [Tooltip("Rest 각도에서 얼마나 앞으로 튕겨나갈지 (상대 오프셋, degrees). " +
             "예: 70이면 Rest보다 70도 앞으로 휘두르는 방향. 부호/크기는 실제 아트 방향에 맞춰 조정.")]
    [SerializeField] private float catapultThrowOffset = 70f;

    [Tooltip("당김 동작에 걸리는 시간(초). 0이면 당김 동작 없이 바로 던짐.")]
    [SerializeField] private float catapultPullBackDuration = 0.08f;

    [Tooltip("PullBack(or Rest) -> Throw 각도로 휘두르는 데 걸리는 시간(초)")]
    [SerializeField] private float catapultSwingDuration = 0.12f;

    [Tooltip("던진 후 Rest 각도로 복귀하는 데 걸리는 시간(초)")]
    [SerializeField] private float catapultReturnDuration = 0.2f;

    private Coroutine catapultMotionRoutine;
    private float catapultRestAngle; // 실제 사용되는 Rest 각도 (자동 캡처 또는 Override)

    private GameObject currentProjectile;
    private bool isProjectileReadyForInput = false; // Show 이후 ~ Input 판정 전까지 true

    protected override float TimerDuration => 15f;
    protected override string MinigameTitle => "젤리 블록 철거";

    protected override void Awake()
    {
        base.Awake();
        CaptureCatapultRestAngle();
    }

    private void CaptureCatapultRestAngle()
    {
        if (useCurrentArmRotationAsRest && catapultArm != null)
        {
            catapultRestAngle = catapultArm.localEulerAngles.z;
        }
        else
        {
            catapultRestAngle = catapultRestAngleOverride;
        }
    }

    public override void StartGame()
    {
        base.StartGame();

        isProjectileReadyForInput = false;

        if (currentProjectile != null)
        {
            Destroy(currentProjectile);
            currentProjectile = null;
        }
    }

    public override void ResetGame()
    {
        base.ResetGame();

        isProjectileReadyForInput = false;

        if (currentProjectile != null)
        {
            Destroy(currentProjectile);
            currentProjectile = null;
        }
    }

    private void Update()
    {
        if (IsInputLocked || IsSuccess)
            return;

        if (WasTouchedThisFrame())
        {
            // 판정 자체는 RhythmManager가 수행한다.
            // action 이름은 CSV의 type 값과 대소문자 무관하게 매칭된다.
            OnPlayerInput("Input");
        }
    }

    private bool WasTouchedThisFrame()
    {
        if (Input.GetMouseButtonDown(0))
            return true;

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            return true;
        }
#endif

        return false;
    }

    // RhythmManager -> MiniGameBase -> 여기 : 차트 타이밍 신호 (Show / Input 등)
    public override void OnRhythmEvent(string action)
    {
        base.OnRhythmEvent(action);

        if (string.Equals(action, "Show", StringComparison.OrdinalIgnoreCase))
        {
            SpawnAndDropProjectile();
        }
        else if (string.Equals(action, "Input", StringComparison.OrdinalIgnoreCase))
        {
            isProjectileReadyForInput = true;
        }
    }

    // RhythmManager -> MiniGameBase -> 여기 : Perfect/Good/Miss 판정 결과
    public override void OnJudgement(JudgementResult judgement)
    {
        // 점수 집계(총 노드/Perfect/Good/Miss)는 베이스에서 그대로 처리한다.
        base.OnJudgement(judgement);

        isProjectileReadyForInput = false;

        if (currentProjectile == null)
            return; // Show 없이 들어온 입력 등 예외 상황 방어

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                LaunchProjectile(hitTarget: true);
                break;

            case JudgementResult.Miss:
                LaunchProjectile(hitTarget: false);
                break;
        }
    }

    private void SpawnAndDropProjectile()
    {
        if (currentProjectile != null)
        {
            Destroy(currentProjectile);
            currentProjectile = null;
        }

        if (projectilePrefab == null || spawnPoint == null || catapultPoint == null)
        {
            Debug.LogWarning("[JellyBlockDemolitionMinigame] projectilePrefab/spawnPoint/catapultPoint가 설정되지 않았습니다.");
            return;
        }

        currentProjectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);

        StartCoroutine(MoveRoutine(
            currentProjectile,
            spawnPoint.position,
            catapultPoint.position,
            fallDuration,
            onComplete: null));
    }

    private void LaunchProjectile(bool hitTarget)
    {
        if (currentProjectile == null)
            return;

        GameObject projectile = currentProjectile;
        currentProjectile = null; // 다음 Show 소환과 겹치지 않도록 즉시 참조 해제

        PlayCatapultThrowMotion();

        Vector3 startPos = projectile.transform.position;
        Vector3 targetPos = targetPoint != null ? targetPoint.position : transform.position;

        StartCoroutine(MoveRoutine(
            projectile,
            startPos,
            targetPos,
            flyDuration,
            onComplete: () =>
            {
                if (hitTarget)
                {
                    // TODO: 젤리 빌딩 명중 연출(흔들림, 파티클, 사운드 등)이 필요하면 여기서 처리
                    OnProjectileHit();
                }

                // 날아가는 모션이 끝난 뒤 탄환 삭제
                if (projectile != null)
                {
                    Destroy(projectile);
                }
            }));
    }

    /// <summary>
    /// 애니메이션 클립 없이 코드로 구현하는 투석기 던지기 모션.
    /// (선택) PullBack 각도 -> Throw 각도로 빠르게 휘두른 뒤 -> Rest 각도로 복귀.
    /// catapultArm이 지정되지 않았다면 아무 동작도 하지 않는다.
    /// </summary>
    private void PlayCatapultThrowMotion()
    {
        if (catapultArm == null)
            return;

        if (catapultMotionRoutine != null)
            StopCoroutine(catapultMotionRoutine);

        catapultMotionRoutine = StartCoroutine(CatapultThrowMotionRoutine());
    }

    private IEnumerator CatapultThrowMotionRoutine()
    {
        float pullBackAngle = catapultRestAngle + catapultPullBackOffset;
        float throwAngle = catapultRestAngle + catapultThrowOffset;

        // 1) (선택) 던지기 직전 받침대 쪽으로 당기는 준비 동작
        if (catapultPullBackDuration > 0f &&
            !Mathf.Approximately(catapultPullBackOffset, 0f))
        {
            yield return RotateArmRoutine(
                catapultRestAngle,
                pullBackAngle,
                catapultPullBackDuration);
        }
        else
        {
            SetArmAngle(catapultRestAngle);
        }

        // 2) 던지는 순간: PullBack(or Rest) -> Throw 각도로 빠르게 휘두름
        float swingFrom = (catapultPullBackDuration > 0f &&
                            !Mathf.Approximately(catapultPullBackOffset, 0f))
            ? pullBackAngle
            : catapultRestAngle;

        yield return RotateArmRoutine(
            swingFrom,
            throwAngle,
            catapultSwingDuration);

        // 3) 던진 뒤 다시 Rest 각도로 복귀
        yield return RotateArmRoutine(
            throwAngle,
            catapultRestAngle,
            catapultReturnDuration);

        catapultMotionRoutine = null;
    }

    private void SetArmAngle(float angle)
    {
        if (catapultArm == null)
            return;

        Vector3 euler = catapultArm.localEulerAngles;
        euler.z = angle;
        catapultArm.localEulerAngles = euler;
    }

    private IEnumerator RotateArmRoutine(float fromAngle, float toAngle, float duration)
    {
        if (catapultArm == null)
            yield break;

        duration = Mathf.Max(0.0001f, duration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (catapultArm == null)
                yield break;

            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / duration);

            // 필요하면 여기 ratio에 easing(ease-out 등)을 적용해 손맛을 더할 수 있음
            float angle = Mathf.LerpAngle(fromAngle, toAngle, ratio);
            SetArmAngle(angle);

            yield return null;
        }

        SetArmAngle(toAngle);
    }

    /// <summary>
    /// 탄환이 targetPoint에 명중했을 때 호출됨.
    /// 젤리 빌딩 쪽 연출(예: 젤리블록 흔들림/파괴 애니메이션)을 연결하고 싶다면 여기를 override하거나
    /// 이벤트를 추가해서 사용.
    /// </summary>
    protected virtual void OnProjectileHit()
    {
    }

    // RigidBody를 사용하지 않는 좌표 보간 이동 (낙하 / 발사 공용)
    private IEnumerator MoveRoutine(
        GameObject target,
        Vector3 from,
        Vector3 to,
        float duration,
        Action onComplete)
    {
        float elapsed = 0f;
        duration = Mathf.Max(0.0001f, duration);

        while (elapsed < duration)
        {
            if (target == null)
                yield break;

            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / duration);

            // 필요하다면 여기서 ratio에 easing / 포물선 곡선을 적용해 궤적을 다듬을 수 있음
            target.transform.position = Vector3.Lerp(from, to, ratio);

            yield return null;
        }

        if (target != null)
            target.transform.position = to;

        onComplete?.Invoke();
    }
}
