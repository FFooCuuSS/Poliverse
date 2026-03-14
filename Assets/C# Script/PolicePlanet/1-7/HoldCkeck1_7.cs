using DG.Tweening;
using UnityEngine;

public class HoldCheck1_7 : MonoBehaviour
{
    [Header("UI Prefab")]
    [SerializeField] private GameObject holdUIPrefab;

    [Header("Triangle Random Offset")]
    [SerializeField] private float minDownOffset = 0.6f;
    [SerializeField] private float maxDownOffset = 1.8f;
    [SerializeField] private float maxSideOffset = 1f;

    [Header("Timing")]
    [SerializeField] private float previewDuration = 0.5f;
    [SerializeField] private float judgeFadeDuration = 0.2f;

    private Minigame_1_7 minigame;

    private GameObject currentHoldUI;
    private Transform cachedPrisoner;

    private Transform targetCircle;
    private Transform shrinkingCircle;
    private Tween shrinkTween;

    private SpriteRenderer[] cachedRenderers;
    private Collider2D currentCollider;

    public void SetMinigame(Minigame_1_7 target)
    {
        minigame = target;
    }

    public void PrepareRound(Transform prisoner, int holdCount)
    {
        cachedPrisoner = prisoner;
        CleanupUI();
    }

    public void ShowPreviewUI(int inputIndex, Transform prisoner)
    {
        cachedPrisoner = prisoner;
        CleanupUI();

        if (holdUIPrefab == null || cachedPrisoner == null || minigame == null)
            return;

        Vector3 apexPos = cachedPrisoner.position;
        apexPos.x = 0f;

        Vector3 spawnPos = GetTriangleRandomPosition(apexPos);

        currentHoldUI = Instantiate(
            holdUIPrefab,
            spawnPos,
            Quaternion.identity,
            minigame.transform
        );

        var relay = currentHoldUI.GetComponent<HoldUIClickRelay1_7>();
        if (relay == null)
            relay = currentHoldUI.AddComponent<HoldUIClickRelay1_7>();

        relay.SetOwner(this);

        currentCollider = currentHoldUI.GetComponent<Collider2D>();
        if (currentCollider == null)
            currentCollider = currentHoldUI.AddComponent<CircleCollider2D>();

        currentCollider.isTrigger = true;
        currentCollider.enabled = true;   // 즉시 클릭 가능

        cachedRenderers = currentHoldUI.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in cachedRenderers)
        {
            sr.sortingLayerName = "UI";
            sr.sortingOrder = 200;

            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        targetCircle = currentHoldUI.transform.Find("TargetCircle");
        shrinkingCircle = currentHoldUI.transform.Find("ShrinkingCircle");

        if (targetCircle == null || shrinkingCircle == null)
            return;

        float targetScale = targetCircle.localScale.x;
        float startScale = targetScale * 2.2f;

        shrinkingCircle.localScale = Vector3.one * startScale;

        shrinkTween = shrinkingCircle.DOScale(Vector3.one * targetScale, previewDuration)
            .SetEase(Ease.Linear);
    }

    private Vector3 GetTriangleRandomPosition(Vector3 apexPos)
    {
        float down = Random.Range(minDownOffset, maxDownOffset);
        float t = Mathf.InverseLerp(minDownOffset, maxDownOffset, down);
        float sideLimit = Mathf.Lerp(0.04f, maxSideOffset, t);
        float side = Random.Range(-sideLimit, sideLimit);

        return apexPos + new Vector3(side, -down, 0f);
    }

    public void OpenJudgeWindow()
    {
        // 이제 사실상 의미 없음. 호환용으로만 둠.
        if (currentCollider != null)
            currentCollider.enabled = true;
    }

    public void CloseJudgeWindow()
    {
        if (currentCollider != null)
            currentCollider.enabled = false;
    }

    public void OnHoldUIClicked()
    {
        if (minigame == null) return;
        if (currentHoldUI == null) return;

        minigame.OnHoldButtonPressed();
    }

    public void PlayJudgeFeedback(MiniGameBase.JudgementResult judgement)
    {
        CloseJudgeWindow();

        if (currentHoldUI == null) return;

        if (shrinkTween != null && shrinkTween.IsActive())
        {
            shrinkTween.Kill();
            shrinkTween = null;
        }

        if (cachedRenderers == null || cachedRenderers.Length == 0)
        {
            CleanupUI();
            return;
        }

        Sequence seq = DOTween.Sequence();

        if (judgement == MiniGameBase.JudgementResult.Perfect ||
            judgement == MiniGameBase.JudgementResult.Good)
        {
            foreach (var sr in cachedRenderers)
                seq.Join(sr.DOFade(0f, judgeFadeDuration));
        }
        else
        {
            currentHoldUI.transform.DOShakePosition(0.08f, 0.08f, 8, 90f, false, true);

            foreach (var sr in cachedRenderers)
                seq.Join(sr.DOFade(0f, judgeFadeDuration));
        }

        seq.OnComplete(CleanupUI);
    }

    public void ResetAll()
    {
        cachedPrisoner = null;
        CleanupUI();
    }

    private void CleanupUI()
    {
        if (shrinkTween != null && shrinkTween.IsActive())
        {
            shrinkTween.Kill();
            shrinkTween = null;
        }

        if (currentHoldUI != null)
        {
            Destroy(currentHoldUI);
            currentHoldUI = null;
        }

        currentCollider = null;
        cachedRenderers = null;
        targetCircle = null;
        shrinkingCircle = null;
    }
}