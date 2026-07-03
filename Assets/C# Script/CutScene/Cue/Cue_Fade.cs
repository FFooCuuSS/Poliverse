using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Cue_Fade : CutsceneCue
{
    [SerializeField] private Image target;

    [SerializeField] private float fromAlpha = 0f;
    [SerializeField] private float toAlpha = 1f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private Ease ease = Ease.OutCubic;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<Image>();
    }

    private void SetAlpha(float alpha)
    {
        if (target == null) return;

        Color color = target.color;
        color.a = alpha;
        target.color = color;
    }

    public override void Play(CutSceneLoader loader)
    {
        if (target == null) return;

        target.DOKill();
        SetAlpha(fromAlpha);
        target.DOFade(toAlpha, duration).SetEase(ease);
    }

    public override void Restore(CutSceneLoader loader)
    {
        if (target == null) return;

        target.DOKill();
        SetAlpha(toAlpha);
    }

    public override void ResetCue(CutSceneLoader loader)
    {
        if (target == null) return;

        target.DOKill();
        SetAlpha(fromAlpha);
    }
}