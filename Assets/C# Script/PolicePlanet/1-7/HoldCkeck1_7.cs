using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldCheck1_7 : MonoBehaviour
{
    [Header("UI Prefab & Settings")]
    public GameObject holdUIPrefab;
    public int maxHoldCount = 3;
    [SerializeField] private float successTolerance = 0.2f;

    [Header("Timing Control")]
    [SerializeField] private float shrinkDuration = 0.9f;
    [SerializeField] private float vanishMargin = 0.05f;

    [Header("게임 참조")]
    public Minigame_1_7 minigame;

    public int CurrentUINode { get; private set; } = 0;

    private GameObject currentHoldUI;
    private Transform targetCircle;
    private Transform shrinkingCircle;

    private bool isHolding = false;

    private float shrinkTimer = 0f;
    private float startScale;
    private float endScale;

    private bool currentHoldSuccess = false;

    private Transform cachedPrisoner;

    // 라운드 시작용
    public void StartAllHolds(Transform prisoner)
    {
        Debug.Log("StartAllHolds 호출");

        cachedPrisoner = prisoner;
        CurrentUINode = 0;
        SpawnNextUI();
    }

    // 라운드 리셋용
    public void ResetHoldNodes()
    {
        CleanupUI();
        CurrentUINode = 0;
        isHolding = false;
    }

    public bool CanHold()
    {
        return !isHolding && CurrentUINode < maxHoldCount;
    }

    private void SpawnNextUI()
    {
        if (CurrentUINode >= maxHoldCount || cachedPrisoner == null) 
        {
            return;
        }
          
        CleanupUI();

        currentHoldUI = Instantiate(
            holdUIPrefab,
            cachedPrisoner.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1.0f, 1.5f), 0f),
            Quaternion.identity,
            minigame.transform // 여기가 부모 지정 부분
        );

        SpriteRenderer[] renderers = currentHoldUI.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            sr.sortingLayerName = "UI";
            sr.sortingOrder = 200;
        }

        currentHoldUI.transform.position = cachedPrisoner.position + new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(1.0f, 1.5f),
            0f
        );

        targetCircle = currentHoldUI.transform.Find("TargetCircle");
        shrinkingCircle = currentHoldUI.transform.Find("ShrinkingCircle");

        shrinkTimer = 0f;
        startScale = shrinkingCircle.localScale.x;
        endScale = targetCircle.localScale.x + vanishMargin;

        isHolding = true;
        currentHoldSuccess = false;
    }

    void Update()
    {
        if (!isHolding || shrinkingCircle == null || targetCircle == null)
            return;

        shrinkTimer += Time.deltaTime;
        float k = Mathf.Clamp01(shrinkTimer / Mathf.Max(0.01f, shrinkDuration));

        float currentScale = Mathf.Lerp(startScale, endScale, k);
        shrinkingCircle.localScale = Vector3.one * currentScale;

        float diff = Mathf.Abs(currentScale - targetCircle.localScale.x);

        if (Input.GetMouseButtonDown(0))
        {
            if (diff <= successTolerance)
            {
                currentHoldSuccess = true;
                EndCurrentHold();
                return;
            }
        }

        if (k >= 1f)
        {
            currentHoldSuccess = false;
            EndCurrentHold();
        }
    }

    private void EndCurrentHold()
    {
        isHolding = false;

        CleanupUI();

        // 미니게임에 판정 전달
        if (minigame != null)
        {
            if (currentHoldSuccess)
                minigame.OnJudgement(MiniGameBase.JudgementResult.Good);
            else
                minigame.OnJudgement(MiniGameBase.JudgementResult.Miss);
        }

        CurrentUINode++;

        if (CurrentUINode < maxHoldCount)
        {
            SpawnNextUI();
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