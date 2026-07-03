using DG.Tweening;
using UnityEngine;

public class Cue_Shake : CutsceneCue
{
    [SerializeField] private RectTransform target;

    [SerializeField] private float duration = 0.25f;
    [SerializeField] private float strength = 30f;
    [SerializeField] private int vibrato = 20;
    [SerializeField] private float randomness = 90f;

    private Vector2 originalPosition;
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

        originalPosition = target.anchoredPosition;
        cached = true;
    }

    public override void Play(CutSceneLoader loader)
    {
        if (target == null) return;

        Cache();

        target.DOKill();

        target.DOShakeAnchorPos(duration, strength, vibrato, randomness)
            .OnComplete(() =>
            {
                if (target != null)
                    target.anchoredPosition = originalPosition;
            });
    }

    public override void Restore(CutSceneLoader loader)
    {
        if (target == null) return;

        target.DOKill();
        target.anchoredPosition = originalPosition;
    }

    public override void ResetCue(CutSceneLoader loader)
    {
        if (target == null) return;

        target.DOKill();
        target.anchoredPosition = originalPosition;
    }
}