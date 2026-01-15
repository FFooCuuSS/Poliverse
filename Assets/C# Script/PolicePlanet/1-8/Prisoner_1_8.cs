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

    [Header("Capture Range")]
    public float captureRangeX = 0.7f;
    public float captureRangeY = 0.8f;


    public float moveSpeed = 2f;
    public float destroyX = -7f;

    public GameObject prison;

    // 감옥에 들어갈 수 있는 상태인지
    public bool canBeCaptured = false;

    private bool isCaptured = false;

    public bool IsCaptured => isCaptured;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }


    void Update()
    {
        if (isCaptured) return;

        // 왼쪽으로 이동
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

        // 화면 밖 제거
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }

        CheckPrisonRange();
    }

    void CheckPrisonRange()
    {
        if (prison == null) return;

        Vector2 prisonerPos = transform.position;
        Vector2 prisonPos = prison.transform.position;

        float distanceX = Mathf.Abs(prisonerPos.x - prisonPos.x);
        float distanceY = Mathf.Abs(prisonerPos.y - prisonPos.y);

        canBeCaptured =
            distanceX <= captureRangeX &&
            distanceY <= captureRangeY;
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

        if (sr != null)
        {
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 0.4f);
        }

        StartCoroutine(FadeOutAndDestroy());
    }

    IEnumerator FadeOutAndDestroy()
    {
        yield return new WaitForSeconds(0.4f); // 멈춘 상태 보여주기

        float t = 0f;
        float duration = 0.4f;
        Color start = sr.color;
        Color end = new Color(start.r, start.g, start.b, 0f);

        while (t < duration)
        {
            t += Time.deltaTime;
            sr.color = Color.Lerp(start, end, t / duration);
            yield return null;
        }

        Destroy(gameObject);
    }

}
