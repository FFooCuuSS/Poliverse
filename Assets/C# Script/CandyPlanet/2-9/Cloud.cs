using UnityEngine;
using DG.Tweening;

public class Cloud : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveStep = 1.5f;       // Show 이벤트 1회당 이동 거리
    public float destroyX = -12f;       // 이 x좌표보다 왼쪽으로 가면 삭제 (좌측 이동이므로 음수)

    [Header("이동 연출 (쿠션감)")]
    public float moveDuration = 0.25f;  // 한 번 이동에 걸리는 시간
    public Ease moveEase = Ease.OutCubic;

    [Header("상태")]
    public bool isShiny = false;        // CloudSpawner가 생성 시 세팅
    public bool isGrabbed = false;

    [Header("파편 연출")]
    public GameObject debrisPrefab;
    public int debrisCount = 5;

    private Transform followTarget;
    private Tween moveTween;
    private CloudSpawner spawner;

    /// <summary>
    /// CloudSpawner가 생성 직후 호출. BeatManager 대신 Spawner의 이동 신호를 구독한다.
    /// </summary>
    public void Init(CloudSpawner owner)
    {
        spawner = owner;
        spawner.OnMoveTick += Move;
    }

    void OnDisable()
    {
        if (spawner != null)
            spawner.OnMoveTick -= Move;

        moveTween?.Kill();
    }

    void Move()
    {
        if (isGrabbed) return;

        Vector3 targetPos = transform.position + Vector3.left * moveStep;

        moveTween?.Kill();
        moveTween = transform.DOMove(targetPos, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                if (transform.position.x < destroyX)
                {
                    Destroy(gameObject);
                }
            });
    }

    void Update()
    {
        if (!isGrabbed) return;

        if (followTarget != null)
        {
            transform.position = followTarget.position;
        }
    }

    public void Grab(Transform hand)
    {
        isGrabbed = true;
        followTarget = hand;
        moveTween?.Kill(); // 이동 트윈과 손 추적이 같은 프레임에 충돌하지 않도록 정리
    }

    public void ReleaseAndBreak()
    {
        moveTween?.Kill();

        for (int i = 0; i < debrisCount; i++)
        {
            GameObject debris = Instantiate(debrisPrefab, transform.position, Quaternion.identity);

            Rigidbody2D rb = debris.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 force = new Vector2(
                    Random.Range(-2f, 2f),
                    Random.Range(3f, 6f)
                );

                rb.AddForce(force, ForceMode2D.Impulse);
            }
        }

        Destroy(gameObject);
    }
}