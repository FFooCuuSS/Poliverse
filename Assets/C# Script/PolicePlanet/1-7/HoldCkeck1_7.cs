using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldCheck1_7 : MonoBehaviour
{
    [Header("UI Prefab & Settings")]
    public GameObject holdUIPrefab;
    public float shrinkSpeed = 2f;
    public int maxHoldCount = 4;                   // 전체 UI 노드 수
    [SerializeField] private float successTolerance = 0.2f;

    [Header("게임 참조")]
    public Minigame_1_7 minigame;

    [Header("클릭 버퍼링")]
    private bool clickBuffered = false;
    private float clickBufferTimer = 0f;
    [SerializeField] private float clickBufferTime = 0.15f;

    // 현재 UI 노드 진행
    public int CurrentUINode { get; private set; } = 0;

    // 내부 변수
    private GameObject currentHoldUI;
    private Transform targetCircle;
    private Transform shrinkingCircle;
    private bool isHolding = false;

    public void StartAllHolds(Transform prisoner)
    {
        CurrentUINode = 0;
        SpawnNextUI(prisoner);
    }

    public bool CanHold()
    {
        return !isHolding && CurrentUINode < maxHoldCount;
    }

    public void HideHoldUI()
    {
        if (currentHoldUI != null)
        {
            Destroy(currentHoldUI);
            currentHoldUI = null;
        }
        isHolding = false;
    }

    private void SpawnNextUI(Transform prisoner)
    {
        if (CurrentUINode >= maxHoldCount)
        {
            // 더 이상 UI 노드 없음
            return;
        }

        // 이전 UI 제거
        HideHoldUI();

        // 새로운 UI 생성
        currentHoldUI = Instantiate(holdUIPrefab);

        // 화면 맨 위로 보이게
        SpriteRenderer[] renderers = currentHoldUI.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            sr.sortingLayerName = "UI";
            sr.sortingOrder = 200;
        }

        // 죄수 주변 랜덤 위치
        currentHoldUI.transform.position = prisoner.position + new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(1.0f, 1.5f),
            0f
        );

        // 원들 찾기
        targetCircle = currentHoldUI.transform.Find("TargetCircle");
        shrinkingCircle = currentHoldUI.transform.Find("ShrinkingCircle");

        if (shrinkingCircle != null) shrinkingCircle.localScale = Vector3.one * 4f;
        if (targetCircle != null) targetCircle.localScale = Vector3.one * 3f;

        isHolding = true;
        clickBuffered = false;
        clickBufferTimer = 0f;
    }

    private void Resolve(bool success)
    {
        isHolding = false;
        clickBuffered = false;

        if (currentHoldUI != null)
        {
            Destroy(currentHoldUI);
            currentHoldUI = null;
        }

        CurrentUINode++;

        if (success)
        {
            Debug.Log($"Hold 성공! 노드 {CurrentUINode} / {maxHoldCount}");
            minigame.RegisterHoldSuccess();
        }
        else
        {
            Debug.Log($"Hold 실패! 노드 {CurrentUINode} / {maxHoldCount}");
            minigame.RegisterHoldFail();
        }

        // 다음 UI 노드가 남아 있으면 생성
        if (CurrentUINode < maxHoldCount)
        {
            PrisonerController1_7 prisoner = FindObjectOfType<PrisonerController1_7>();
            if (prisoner != null)
            {
                SpawnNextUI(prisoner.transform);
            }
        }
    }

    void Update()
    {
        // 클릭 입력 버퍼링
        if (Input.GetMouseButtonDown(0))
        {
            clickBuffered = true;
            clickBufferTimer = clickBufferTime;
        }

        if (clickBuffered)
        {
            clickBufferTimer -= Time.deltaTime;
            if (clickBufferTimer <= 0f) clickBuffered = false;
        }

        // Hold 진행 중이 아니면 종료
        if (!isHolding || shrinkingCircle == null || targetCircle == null)
            return;

        // 줄어드는 원 축소
        shrinkingCircle.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        float diff = Mathf.Abs(shrinkingCircle.localScale.x - targetCircle.localScale.x);

        // 성공 판정: 기준원과 차이가 허용범위 내에서 클릭했을 때
        if (diff <= successTolerance && clickBuffered)
        {
            Resolve(true);
            return;
        }

        // 실패 판정: 줄어드는 원이 기준원보다 작아졌을 때
        if (shrinkingCircle.localScale.x < targetCircle.localScale.x - successTolerance)
        {
            Resolve(false);
        }
    }

    public void OnHoldStart(Transform prisoner)
    {
        if (CanHold())
        {
            SpawnNextUI(prisoner);
        }
    }
}
