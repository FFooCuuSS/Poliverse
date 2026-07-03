using DG.Tweening;
using UnityEngine;

public class Cue_Text : CutsceneCue
{
    public enum Direction
    {
        Right,
        Left,
        Up,
        Down,
        Custom
    }

    [SerializeField] private RectTransform target;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("텍스트가 나아갈 방향")]
    [SerializeField] private Direction direction = Direction.Right;
    [SerializeField] private Vector2 customDirection = Vector2.right;

    [Header("시작 상태")]
    [SerializeField] private float startOffsetDistance = 80f;
    [SerializeField] private float startScaleMultiplier = 0.5f;

    [Header("연출")]
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;
    [SerializeField] private Ease scaleEase = Ease.OutBack;
    [SerializeField] private Ease fadeEase = Ease.OutCubic;

    private Vector2 finalPosition;
    private Vector3 finalScale;
    private bool cached;

    private void Awake()
    {
        AutoAssign();
        CacheFinal();
    }

    private void AutoAssign()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void CacheFinal()
    {
        if (cached || target == null) return;

        finalPosition = target.anchoredPosition;
        finalScale = target.localScale;
        cached = true;
    }

    private Vector2 GetDirection()
    {
        Vector2 dir = direction switch
        {
            Direction.Right => Vector2.right,
            Direction.Left => Vector2.left,
            Direction.Up => Vector2.up,
            Direction.Down => Vector2.down,
            Direction.Custom => customDirection,
            _ => Vector2.right
        };

        if (dir.sqrMagnitude <= 0.001f)
            dir = Vector2.right;

        return dir.normalized;
    }

    private void SetStartState()
    {
        AutoAssign();
        CacheFinal();

        Vector2 dir = GetDirection();

        target.anchoredPosition = finalPosition - dir * startOffsetDistance;
        target.localScale = finalScale * startScaleMultiplier;
        canvasGroup.alpha = 0f;
    }

    public override void Play(CutSceneLoader loader)
    {
        AutoAssign();
        CacheFinal();

        if (target == null || canvasGroup == null) return;

        target.DOKill();
        canvasGroup.DOKill();

        SetStartState();

        target.DOAnchorPos(finalPosition, duration).SetEase(moveEase);
        target.DOScale(finalScale, duration).SetEase(scaleEase);
        canvasGroup.DOFade(1f, duration).SetEase(fadeEase);
    }

    public override void Restore(CutSceneLoader loader)
    {
        AutoAssign();
        CacheFinal();

        if (target == null || canvasGroup == null) return;

        target.DOKill();
        canvasGroup.DOKill();

        target.anchoredPosition = finalPosition;
        target.localScale = finalScale;
        canvasGroup.alpha = 1f;
    }

    public override void ResetCue(CutSceneLoader loader)
    {
        AutoAssign();
        CacheFinal();

        if (target == null || canvasGroup == null) return;

        target.DOKill();
        canvasGroup.DOKill();

        SetStartState();
    }
}