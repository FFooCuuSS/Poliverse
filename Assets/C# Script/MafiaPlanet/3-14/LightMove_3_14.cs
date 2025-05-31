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
        // 1. ���� ��ġ�� �������� ���� (3.5 ~ -3.5)
        float startY = Random.Range(lowerLimit, upperLimit);
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);

        // 2. �ӵ� ����ȭ
        float speed = Random.Range(baseSpeed - speedVariation, baseSpeed + speedVariation);
        float distance = upperLimit - lowerLimit;
        float singleDuration = distance / speed;

        // 3. ���� ��ġ�� ���� �ݴ������� ���� �̵� (���� ���� ����ó�� ���̰�)
        float firstTargetY = (startY > 0) ? lowerLimit : upperLimit;

        moveTween = transform.DOMoveY(firstTargetY, singleDuration / 2f) // �ݴ������ ���� �ð�
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                // �պ� �ݺ� ����
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
            moveTween.Kill(); // ������ ����
            L_minigame_3_14.Failure();
        }
    }
}
