using System.Collections;
using UnityEngine;

public class SpawnedHandControler : MonoBehaviour
{
    public Minigame1_5_Manager_remake minigameManager1_5;

    private SpriteRenderer sr;
    private bool isCollidingEnemy;
    private bool hitProcessed;

    [Header("Flash Settings")]
    public float flashInTime = 0.04f;
    public float flashOutTime = 0.06f;

    private FaceChange currentFace; // 충돌한 Enemy의 FaceChange

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(FlashAndDestroy());
    }

    private IEnumerator FlashAndDestroy()
    {
        if (sr == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Color c = sr.color;

        c.a = 0f;
        sr.color = c;

        float t = 0f;
        while (t < flashInTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / flashInTime);
            sr.color = c;
            yield return null;
        }

        c.a = 1f;
        sr.color = c;

        t = 0f;
        while (t < flashOutTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / flashOutTime);
            sr.color = c;
            yield return null;
        }

        c.a = 0f;
        sr.color = c;

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        isCollidingEnemy = true;

        if (minigameManager1_5 != null)
            minigameManager1_5.collideCnt++;

        // 맞은 Enemy의 FaceChange 캐싱
        currentFace = collision.GetComponent<FaceChange>();

        TryChangeFace();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            isCollidingEnemy = false;
            currentFace = null;
        }
    }

    private void Update()
    {
        // 판정이 충돌 이후에 들어오는 경우 대비
        TryChangeFace();
    }

    private void TryChangeFace()
    {
        if (hitProcessed) return;
        if (!isCollidingEnemy) return;
        if (currentFace == null) return;
        if (minigameManager1_5 == null) return;
        if (!minigameManager1_5.lastJudgeGoodOrPerfect) return;

        // 여기서 얼굴 변경
        currentFace.ChangeToHit();

        hitProcessed = true;
    }
}
