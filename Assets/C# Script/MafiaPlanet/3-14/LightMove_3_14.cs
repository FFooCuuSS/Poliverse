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
        // 1. 시작 위치를 무작위로 설정 (3.5 ~ -3.5)
        float startY = Random.Range(lowerLimit, upperLimit);
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);

        // 2. 속도 랜덤화
        float speed = Random.Range(baseSpeed - speedVariation, baseSpeed + speedVariation);
        float distance = upperLimit - lowerLimit;
        float singleDuration = distance / speed;

        // 3. 시작 위치에 따라 반대편으로 먼저 이동 (최초 방향 랜덤처럼 보이게)
        float firstTargetY = (startY > 0) ? lowerLimit : upperLimit;

        moveTween = transform.DOMoveY(firstTargetY, singleDuration / 2f) // 반대편까지 절반 시간
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                // 왕복 반복 시작
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
            moveTween.Kill(); // 움직임 정지
            L_minigame_3_14.Failure();
        }
    }
}
