using System.Collections;
using UnityEngine;

public class Prisoner_1_8 : MonoBehaviour
{
    [Header("Flip Animation")]
    [SerializeField] private Sprite spriteA;
    [SerializeField] private Sprite spriteB;
    [SerializeField] private float flipInterval = 0.5f;

    [Header("Move")]
    [SerializeField] private float destroyX = -9f;

    private Manager_1_8 manager;
    private GameObject prison;

    private bool isCaptured = false;
    public bool IsCaptured => isCaptured;

    private SpriteRenderer sr;
    private Vector2 moveDir;
    private float moveSpeed;

    private void Awake()
    {
        moveDir = new Vector2(-1f, -0.05f).normalized;
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(SpriteFlipRoutine());
    }

    public void Initialize(Manager_1_8 owner, GameObject prisonObj, float speed)
    {
        manager = owner;
        prison = prisonObj;
        moveSpeed = speed;
    }

    private void Update()
    {
        if (isCaptured) return;

        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        if (transform.position.x < destroyX)
        {
            if (manager != null)
                manager.NotifyEscaped(this);

            Destroy(gameObject);
        }
    }

    public void Capture()
    {
        if (isCaptured) return;

        isCaptured = true;
        moveSpeed = 0f;

        if (manager != null)
            manager.NotifyCaptured(this);

        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in srs)
        {
            Color c = r.color;
            r.color = new Color(c.r, c.g, c.b, 0.4f);
        }

        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        yield return new WaitForSeconds(0.4f);

        float t = 0f;
        float duration = 0.4f;

        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
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

    private IEnumerator SpriteFlipRoutine()
    {
        bool toggle = false;

        while (!isCaptured)
        {
            if (sr != null)
                sr.sprite = toggle ? spriteA : spriteB;

            toggle = !toggle;
            yield return new WaitForSeconds(flipInterval);
        }
    }
}