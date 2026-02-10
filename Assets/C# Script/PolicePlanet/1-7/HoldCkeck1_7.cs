using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldCheck1_7 : MonoBehaviour
{
    [Header("UI Prefab & Settings")]
    public GameObject holdUIPrefab;
    public int maxHoldCount = 4;
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

        CleanupUI();

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

        //if (shrinkingCircle != null) shrinkingCircle.localScale = Vector3.one * 4f;
        //if (targetCircle != null) targetCircle.localScale = Vector3.one * 3f;

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

        minigame.RecordHoldResult(currentHoldSuccess);

        CurrentUINode++;

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
