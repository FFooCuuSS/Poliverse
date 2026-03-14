using UnityEngine;
using DG.Tweening;

public class Accessory : MonoBehaviour
{
    public enum AccessoryType
    {
        Hat,
        Glasses,
        Mustache
    }

    [Header("Type")]
    [SerializeField] private AccessoryType accessoryType;
    public AccessoryType Type => accessoryType;

    public bool IsRemoved { get; private set; }
    public bool InputLocked { get; private set; } = true;
    public bool IsInteractableNow { get; private set; } = false;

    private Minigame_1_4 minigame;
    private DragAndDrop drag;
    private Montage curMontage;

    private Tween removeDelayTween;
    private Tween disableTween;
    private Tween[] fadeTweens;

    private void Awake()
    {
        drag = GetComponent<DragAndDrop>();
    }

    private void OnDisable()
    {
        KillTweens();
    }

    public void Init(Minigame_1_4 game)
    {
        minigame = game;
        IsRemoved = false;
        IsInteractableNow = false;

        RestoreAlpha();
        gameObject.SetActive(true);
        LockInput();
    }

    public void SetMontage(Montage montage)
    {
        curMontage = montage;
    }

    public void SetInteractableNow(bool value)
    {
        IsInteractableNow = value;

        if (value && !IsRemoved)
            UnlockInput();
        else
            LockInput();
    }

    public void LockInput()
    {
        InputLocked = true;

        if (drag != null)
            drag.banDragging = true;
    }

    public void UnlockInput()
    {
        if (IsRemoved)
        {
            LockInput();
            return;
        }

        InputLocked = false;

        if (drag != null)
            drag.banDragging = false;
    }

    private void OnMouseUp()
    {
        if (InputLocked) return;
        if (!IsInteractableNow) return;

        OnSlide();
    }

    public void OnSlide()
    {
        if (InputLocked) return;
        if (!IsInteractableNow) return;
        if (IsRemoved) return;
        if (minigame == null) return;

        if (curMontage != null)
            curMontage.PlayHit();

        minigame.TryAccessoryInput(this);
    }

    public void RemoveWithDelay(float delay)
    {
        if (IsRemoved) return;

        IsRemoved = true;
        IsInteractableNow = false;
        LockInput();

        KillTweens();

        removeDelayTween = DOVirtual.DelayedCall(delay, () =>
        {
            FadeOutAndDisable(0.25f);
        });
    }

    private void FadeOutAndDisable(float fadeDuration)
    {
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>(true);

        if (srs == null || srs.Length == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        fadeTweens = new Tween[srs.Length];

        for (int i = 0; i < srs.Length; i++)
        {
            if (srs[i] == null) continue;

            srs[i].DOKill();

            Color c = srs[i].color;
            c.a = 1f;
            srs[i].color = c;

            fadeTweens[i] = srs[i].DOFade(0f, fadeDuration);
        }

        disableTween = DOVirtual.DelayedCall(fadeDuration, () =>
        {
            if (this != null && gameObject != null)
                gameObject.SetActive(false);
        });
    }

    public void DespawnImmediate()
    {
        KillTweens();
        Destroy(gameObject);
    }

    private void RestoreAlpha()
    {
        var srs = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs)
        {
            if (sr == null) continue;
            sr.DOKill();

            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }

    private void KillTweens()
    {
        removeDelayTween?.Kill();
        disableTween?.Kill();

        if (fadeTweens != null)
        {
            for (int i = 0; i < fadeTweens.Length; i++)
                fadeTweens[i]?.Kill();
        }
    }
}