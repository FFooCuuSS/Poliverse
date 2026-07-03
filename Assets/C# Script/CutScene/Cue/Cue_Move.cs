using DG.Tweening;
using UnityEngine;

public class Cue_Move : CutsceneCue
{
    [SerializeField] private RectTransform target;

    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private Ease ease = Ease.OutCubic;

    private Vector2 startPosition;
    private bool cached;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        Cache();
    }

    private void Cache()
    {
        if (target == null || cached) return;

        startPosition = target.anchoredPosition;
        cached = true;
    }

    public override void Play(CutSceneLoader loader)
    {
        if (target == null) return;

        Cache();

        target.DOKill();
        target.DOAnchorPos(targetPosition, duration).SetEase(ease);
    }

    public override void Restore(CutSceneLoader loader)
    {
        if (target == null) return;

        target.DOKill();
        target.anchoredPosition = targetPosition;
    }

    public override void ResetCue(CutSceneLoader loader)
    {
        if (target == null) return;

        Cache();

        target.DOKill();
        target.anchoredPosition = startPosition;
    }
}