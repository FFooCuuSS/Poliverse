using System.Collections;
using UnityEngine;

public class SpawnedHandControler : MonoBehaviour
{
    [SerializeField] private float visibleTime = 0.08f;
    [SerializeField] private float fadeOutTime = 0.2f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        if (sr == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Color c = sr.color;
        c.a = 1f;
        sr.color = c;

        if (visibleTime > 0f)
            yield return new WaitForSeconds(visibleTime);

        float elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutTime);

            c.a = Mathf.Lerp(1f, 0f, t);
            sr.color = c;

            yield return null;
        }

        c.a = 0f;
        sr.color = c;

        Destroy(gameObject);
    }
}