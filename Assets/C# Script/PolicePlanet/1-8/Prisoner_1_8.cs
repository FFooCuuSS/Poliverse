using System.Collections;
using UnityEngine;

public class Prisoner_1_8 : MonoBehaviour
{
    /*
    public float moveSpeed = 0.1f;
    public GameObject prison;

    private Vector2 moveDirection;

    public float minX = -6.5f;
    public float maxX = 6.5f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    public float spawnMinDistance = 3f; // 감옥으로부터 최소 거리

    [Header("스프라이트")]
    public SpriteRenderer spriteRenderer;
    public Sprite leftSprite;
    public Sprite rightSprite;

    void Update()
    {
        //UpdateMoveDirection();

        Vector2 newPosition = (Vector2)transform.position + moveDirection * moveSpeed * Time.deltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        transform.position = new Vector3(newPosition.x, newPosition.y, 0f);

        if (moveDirection.x < 0)
        {
            spriteRenderer.sprite = leftSprite;
        }
        else
        {
            spriteRenderer.sprite = rightSprite;
        }
    }

    void UpdateMoveDirection()
    {
        Vector2 prisonerPos = transform.position;
        Vector2 prisonPos = prison.transform.position;

        // 감옥에서의 방향 계산
        Vector2 awayFromPrison = (prisonerPos - prisonPos).normalized;

        //  y축 제거: x축 방향만 유지
        awayFromPrison.y = 0f;

        if (awayFromPrison == Vector2.zero)
        {
            //  랜덤 x축 방향 보정
            awayFromPrison = new Vector2(Random.value < 0.5f ? -1f : 1f, 0f);
        }

        moveDirection = awayFromPrison.normalized;
    }
    */
    [Header("Flip Animation")]
    [SerializeField] private Sprite spriteA;
    [SerializeField] private Sprite spriteB;
    [SerializeField] private float flipInterval = 0.5f;

    [Header("Capture Range")]
    public float captureRangeX = 0.7f;
    public float captureRangeY = 0.8f;

    public float moveSpeed = 2f;
    public float destroyX = -9f;

    public GameObject prison;

    // 감옥에 들어갈 수 있는 상태인지
    // public bool canBeCaptured = false;

    private bool isCaptured = false;

    public bool IsCaptured => isCaptured;

    private SpriteRenderer sr;

    private Vector2 moveDir;

    void Awake()
    {
        moveDir = new Vector2(-1f, -0.05f).normalized;
        sr = GetComponent<SpriteRenderer>();

        StartCoroutine(SpriteFlipRoutine());
    }

    void Update()
    {
        if (isCaptured) return;

        transform.Translate(moveDir * moveSpeed * Time.deltaTime);

        if (transform.position.x < destroyX)
        {
            Manager_1_8 manager = FindObjectOfType<Manager_1_8>();
            manager.endedPrisoner++;
            Destroy(gameObject);
        }

        //CheckPrisonRange();
    }

    void CheckPrisonRange()
    {
        if (prison == null) return;

        Vector2 prisonerPos = transform.position;
        Vector2 prisonPos = prison.transform.position;

        float distanceX = Mathf.Abs(prisonerPos.x - prisonPos.x);
        float distanceY = Mathf.Abs(prisonerPos.y - prisonPos.y);

        /*
        canBeCaptured =
            distanceX <= captureRangeX &&
            distanceY <= captureRangeY;
        */
    }

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void Capture()
    {
        if (isCaptured) return;

        isCaptured = true;
        moveSpeed = 0f;

        // 성공 알림
        Manager_1_8 manager = FindObjectOfType<Manager_1_8>();
        if (manager != null)
        {
            manager.hasAnySuccess = true;
            manager.endedPrisoner++;
        }

        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in srs)
        {
            Color c = r.color;
            r.color = new Color(c.r, c.g, c.b, 0.4f);
        }

        StartCoroutine(FadeOutAndDestroy());
    }


    IEnumerator FadeOutAndDestroy()
    {
        yield return new WaitForSeconds(0.4f); // 멈춘 상태 보여주기

        float t = 0f;
        float duration = 0.4f;

        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();

        // 시작 색(각자 컬러가 다를 수 있으니 각각 저장)
        Color[] starts = new Color[srs.Length];
        for (int i = 0; i < srs.Length; i++)
            starts[i] = srs[i].color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);

            for (int i = 0; i < srs.Length; i++)
            {
                Color c = starts[i];
                srs[i].color = new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 0f, k));
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator SpriteFlipRoutine()
    {
        bool toggle = false;

        while (!isCaptured)   // 잡히면 멈추게
        {
            sr.sprite = toggle ? spriteA : spriteB;
            toggle = !toggle;

            yield return new WaitForSeconds(flipInterval);
        }
    }
}
