using DG.Tweening;
using UnityEngine;

public class LightMove_3_14 : MonoBehaviour
{
    public Minigame_3_14 L_minigame_3_14;

    public float baseSpeed = 2f;
    public float speedVariation = 0.5f;
    public float upperLimit = 3.5f;
    public float lowerLimit = -3.5f;

    private Tween moveTween;
    private bool isStopped = false;

    void Start()
    {
        float startY = Random.Range(lowerLimit, upperLimit);
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);

        float speed = Random.Range(baseSpeed - speedVariation, baseSpeed + speedVariation);
        float distance = upperLimit - lowerLimit;
        float singleDuration = distance / speed;

        float firstTargetY = (startY > 0) ? lowerLimit : upperLimit;

        moveTween = transform.DOMoveY(firstTargetY, singleDuration / 2f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                moveTween = transform.DOMoveY((firstTargetY == lowerLimit) ? upperLimit : lowerLimit, singleDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            });
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isStopped)
        {
            isStopped = true;
            moveTween.Kill(); // 충돌 시 움직임만 정지 (Fail() 호출 안 함)
        }
    }
}
