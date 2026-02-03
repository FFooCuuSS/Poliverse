using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldCheck1_7 : MonoBehaviour
{
    //[Header("UI Prefab & Settings")]
    //public GameObject holdUIPrefab;
    //public float shrinkSpeed = 2f;
    //public int maxHoldCount = 4;                   // 전체 UI 노드 수
    //[SerializeField] private float successTolerance = 0.2f;

    //[Header("게임 참조")]
    //public Minigame_1_7 minigame;

    //// 현재 UI 노드 진행
    //public int CurrentUINode { get; private set; } = 0;

    //// 내부 변수
    //private GameObject currentHoldUI;
    //private Transform targetCircle;
    //private Transform shrinkingCircle;
    //private bool isHolding = false;

    //private bool holdStarted = false;

    //public void StartAllHolds(Transform prisoner)
    //{
    //    CurrentUINode = 0;
    //    SpawnNextUI(prisoner);
    //}

    //public bool CanHold()
    //{
    //    return !isHolding && CurrentUINode < maxHoldCount;
    //}

    //public void HideHoldUI()
    //{
    //    if (currentHoldUI != null)
    //    {
    //        Destroy(currentHoldUI);
    //        currentHoldUI = null;
    //    }
    //    isHolding = false;
    //}

    //private void SpawnNextUI(Transform prisoner)
    //{
    //    if (CurrentUINode >= maxHoldCount)
    //    {
    //        // 더 이상 UI 노드 없음
    //        return;
    //    }

    //    // 이전 UI 제거
    //    HideHoldUI();

    //    // 새로운 UI 생성
    //    currentHoldUI = Instantiate(holdUIPrefab);

    //    // 화면 맨 위로 보이게
    //    SpriteRenderer[] renderers = currentHoldUI.GetComponentsInChildren<SpriteRenderer>();
    //    foreach (var sr in renderers)
    //    {
    //        sr.sortingLayerName = "UI";
    //        sr.sortingOrder = 200;
    //    }

    //    // 죄수 주변 랜덤 위치
    //    currentHoldUI.transform.position = prisoner.position + new Vector3(
    //        Random.Range(-0.5f, 0.5f),
    //        Random.Range(1.0f, 1.5f),
    //        0f
    //    );

    //    // 원들 찾기
    //    targetCircle = currentHoldUI.transform.Find("TargetCircle");
    //    shrinkingCircle = currentHoldUI.transform.Find("ShrinkingCircle");

    //    if (shrinkingCircle != null) shrinkingCircle.localScale = Vector3.one * 4f;
    //    if (targetCircle != null) targetCircle.localScale = Vector3.one * 3f;

    //    isHolding = true;
    //    holdStarted = true;
    //}

    //void Update()
    //{
    //    if (!isHolding || shrinkingCircle == null || targetCircle == null || !holdStarted)
    //        return;


    //    // 줄어드는 원 축소
    //    shrinkingCircle.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

    //    float diff = shrinkingCircle.localScale.x - targetCircle.localScale.x;

    //    // 실패 조건: 기준 원보다 작아졌는데 입력 없음
    //    if (diff < -successTolerance)
    //    {
    //        ResolveFail();
    //        return;
    //    }

    //    // 입력 감지
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        // 성공 조건: 겹쳤을 때 입력
    //        if (Mathf.Abs(diff) <= successTolerance)
    //        {
    //            ResolveSuccess();
    //        }
    //        // 겹치지 않았으면 아무 일도 안 함
    //    }
    //}


    //public void OnHoldStart(Transform prisoner)
    //{
    //    if (CanHold())
    //    {
    //        SpawnNextUI(prisoner);
    //    }
    //}

    //public void NotifyHoldInput()
    //{
    //    if (!IsInSuccessWindow())
    //    {
    //        return;
    //    }

    //    minigame.OnHoldButtonPressed();
    //}


    //public bool IsInSuccessWindow()
    //{
    //    if (shrinkingCircle == null || targetCircle == null) return false;

    //    float diff = Mathf.Abs(shrinkingCircle.localScale.x - targetCircle.localScale.x);
    //    return diff <= successTolerance;
    //}

    //public void ResolveAndProceed()
    //{
    //    isHolding = false;

    //    if (currentHoldUI != null)
    //    {
    //        Destroy(currentHoldUI);
    //        currentHoldUI = null;
    //    }

    //    CurrentUINode++;

    //    if (CurrentUINode < maxHoldCount)
    //    {
    //        PrisonerController1_7 prisoner = FindObjectOfType<PrisonerController1_7>();
    //        if (prisoner != null)
    //        {
    //            SpawnNextUI(prisoner.transform);
    //        }
    //    }
    //}

    //private void ResolveSuccess()
    //{
    //    holdStarted = false;
    //    CleanupUI();
    //    CurrentUINode++;
    //    minigame.RegisterHoldSuccess();
    //    SpawnNextIfNeeded();
    //}

    //private void ResolveFail()
    //{
    //    holdStarted = false;
    //    CleanupUI();
    //    CurrentUINode++;
    //    minigame.RegisterHoldFail();
    //    SpawnNextIfNeeded();
    //}


    //private void CleanupUI()
    //{
    //    isHolding = false;

    //    if (currentHoldUI != null)
    //    {
    //        Destroy(currentHoldUI);
    //        currentHoldUI = null;
    //    }
    //}

    //private void SpawnNextIfNeeded()
    //{
    //    if (CurrentUINode >= maxHoldCount) return;

    //    PrisonerController1_7 prisoner = FindObjectOfType<PrisonerController1_7>();
    //    if (prisoner != null)
    //    {
    //        SpawnNextUI(prisoner.transform);
    //    }
    //}


    [Header("UI Prefab & Settings")]
    public GameObject holdUIPrefab;
    public float shrinkSpeed = 2f;
    public int maxHoldCount = 4;
    [SerializeField] private float successTolerance = 0.2f;

    [Header("게임 참조")]
    public Minigame_1_7 minigame;

    public int CurrentUINode { get; private set; } = 0;

    private GameObject currentHoldUI;
    private Transform targetCircle;
    private Transform shrinkingCircle;
    private bool isHolding = false;
    private bool holdStarted = false;

    private bool currentHoldSuccess = false;

    private float holdElapsedTime = 0f;  // 현재 Hold UI가 뜬 후 경과 시간
    public float failDelay = 0.3f;       // 실패 체크 시작까지 최소 대기 시간 (0.1초 정도)

    public void StartAllHolds(Transform prisoner)
    {
        CurrentUINode = 0;
        SpawnNextUI(prisoner);
    }

    public bool CanHold()
    {
        return !isHolding && CurrentUINode < maxHoldCount;
    }

    private void SpawnNextUI(Transform prisoner)
    {
        if (CurrentUINode >= maxHoldCount) return;

        holdElapsedTime = 0f;

        // 이전 UI 제거
        CleanupUI();

        // 새로운 UI 생성
        currentHoldUI = Instantiate(holdUIPrefab);
        SpriteRenderer[] renderers = currentHoldUI.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            sr.sortingLayerName = "UI";
            sr.sortingOrder = 200;
        }

        currentHoldUI.transform.position = prisoner.position + new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(1.0f, 1.5f),
            0f
        );

        targetCircle = currentHoldUI.transform.Find("TargetCircle");
        shrinkingCircle = currentHoldUI.transform.Find("ShrinkingCircle");

        if (shrinkingCircle != null) shrinkingCircle.localScale = Vector3.one * 4f;
        if (targetCircle != null) targetCircle.localScale = Vector3.one * 3f;

        isHolding = true;
        holdStarted = true;
        currentHoldSuccess = false;
    }

    void Update()
    {
        if (!isHolding || shrinkingCircle == null || targetCircle == null || !holdStarted)
            return;

        holdElapsedTime += Time.deltaTime;

        // 줄어드는 원 축소
        shrinkingCircle.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        float diff = shrinkingCircle.localScale.x - targetCircle.localScale.x;

        // 실패 체크는 최소 failDelay 이후부터 하도록
        if (holdElapsedTime >= failDelay)
        {
            if (diff < -successTolerance)
            {
                currentHoldSuccess = false;
                Debug.Log("실패");
                EndCurrentHold();
                return;
            }
        }

        // 입력 감지
        if (Input.GetMouseButtonDown(0))
        {
            if (Mathf.Abs(diff) <= successTolerance)
            {
                currentHoldSuccess = true;
                Debug.Log("성공");
                EndCurrentHold();
            }
        }
    }

    private void EndCurrentHold()
    {
        holdStarted = false;
        isHolding = false;

        CleanupUI();

        // 성공/실패 기록 전달
        minigame.RecordHoldResult(currentHoldSuccess);

        CurrentUINode++;

        // 다음 UI
        if (CurrentUINode < maxHoldCount)
        {
            PrisonerController1_7 prisoner = FindObjectOfType<PrisonerController1_7>();
            if (prisoner != null)
                SpawnNextUI(prisoner.transform);
        }
    }

    private void CleanupUI()
    {
        if (currentHoldUI != null)
        {
            Destroy(currentHoldUI);
            currentHoldUI = null;
        }
    }
}
