using DG.Tweening;
using UnityEngine;

public class HandleMover_1_9 : MonoBehaviour
{
    [Header("È¿°úÀ½")]
    [SerializeField] private AudioClip clickSound;
    private Vector3 initialLocalPosition;
    private Tween moveTween;

    [SerializeField] private Vector3 stretchOffset = new Vector3(1.2f, 0f, 0f);
    [SerializeField] private float stretchDuration = 0.12f;
    [SerializeField] private float shrinkDuration = 0.12f;

    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
    }

    public void PlayStretch()
    {
        if (clickSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
        {
            GameRoot.Instance.Audio.PlaySfx(clickSound);
        }

        moveTween?.Kill();
        transform.localPosition = initialLocalPosition;

        moveTween = DOTween.Sequence()
            .Append(transform.DOLocalMove(initialLocalPosition + stretchOffset, stretchDuration))
            .Append(transform.DOLocalMove(initialLocalPosition, shrinkDuration));
    }

    public void ResetHandle()
    {
        moveTween?.Kill();
        moveTween = null;
        transform.localPosition = initialLocalPosition;
    }
}