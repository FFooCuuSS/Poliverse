using System.Collections;
using UnityEngine;

public class PrisonerVisual : MonoBehaviour
{
    [Header("Sprite References")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hitSprite;

    [Header("Hit Animation")]
    [SerializeField] private float hitMoveDistanceX = 0.6f;
    [SerializeField] private float hitMoveDistanceY = 0.6f;
    [SerializeField] private float hitDuration = 0.4f;

    private SpriteRenderer sr;
    private Vector3 originLocalPos;
    private Color originColor;
    private Coroutine hitCoroutine;

    private bool initialized = false;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (initialized) return;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        originLocalPos = transform.localPosition;

        if (sr != null)
        {
            originColor = sr.color;

            if (normalSprite == null)
                normalSprite = sr.sprite;
        }

        initialized = true;
    }

    public void ResetVisual()
    {
        EnsureInitialized();

        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }

        transform.localPosition = originLocalPos;

        if (sr != null)
        {
            sr.color = originColor;
            sr.sprite = normalSprite;
        }
    }

    public void PlayHit()
    {
        EnsureInitialized();

        if (sr == null) return;

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        hitCoroutine = StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        EnsureInitialized();

        transform.localPosition = originLocalPos;

        Color c = originColor;
        c.a = 1f;
        sr.color = c;

        if (hitSprite != null)
            sr.sprite = hitSprite;

        Vector3 start = originLocalPos;
        Vector3 end = originLocalPos + new Vector3(hitMoveDistanceX, hitMoveDistanceY, 0f);

        float elapsed = 0f;
        while (elapsed < hitDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / hitDuration);

            transform.localPosition = Vector3.Lerp(start, end, t);

            c.a = Mathf.Lerp(1f, 0f, t);
            sr.color = c;

            yield return null;
        }

        transform.localPosition = end;
        c.a = 0f;
        sr.color = c;
    }
}