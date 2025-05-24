using UnityEngine;
using DG.Tweening;

public class PistolUp : MonoBehaviour
{
    public GameObject stage_3_1;
    public bool goingUp = false;

    [Header("»ó½Â ¼³Á¤")]
    public float riseTargetY = 1.5f;
    public float riseDuration = 0.5f;

    private bool hasStarted = false;
    private Minigame_3_1 minigame_3_1;

    private void Start()
    {
        minigame_3_1 = stage_3_1.GetComponent<Minigame_3_1>();
    }

    void Update()
    {
        if (!goingUp || hasStarted) return;

        hasStarted = true;

        transform.DOMoveY(riseTargetY, riseDuration)
                 .SetEase(Ease.OutQuad)
                 .OnComplete(() =>
                 {
                     Invoke(nameof(CallSucceedAndDestroy), 0.5f);
                 });
    }

    private void CallSucceedAndDestroy()
    {
        if (minigame_3_1 != null)
        {
            minigame_3_1.Succeed();
        }

        Destroy(gameObject);
    }
}
