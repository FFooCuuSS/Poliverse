using UnityEngine;
using DG.Tweening;

public class PistolUp : MonoBehaviour
{
    public GameObject stage_3_1;
    private bool goingUpInternal = false;

    public bool goingUp
    {
        get => goingUpInternal;
        set
        {
            if (goingUpInternal != value)
            {
                goingUpInternal = value;
                if (goingUpInternal)
                {
                    StartRise();
                }
            }
        }
    }

    [Header("»ó½Â ¼³Á¤")]
    public float riseTargetY = 1.5f;
    public float riseDuration = 0.5f;

    private bool hasStarted = false;

    private void StartRise()
    {
        if (hasStarted) return;
        hasStarted = true;

        transform.DOMoveY(riseTargetY, riseDuration)
                 .SetEase(Ease.OutQuad)
                 .OnComplete(() =>
                 {
                     Invoke(nameof(DestroySelf), 0.5f);
                 });
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
