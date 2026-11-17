using System.Collections;
using UnityEngine;

public class ShowEffact : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }

    public void BlinkEffect()
    {
        if (blinkCoroutine != null)
        {
            return;
        }

        blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator BlinkCoroutine()
    {
        spriteRenderer.enabled = true;

        yield return new WaitForSeconds(0.2f);

        spriteRenderer.enabled = false;
        blinkCoroutine = null;
    }
}
