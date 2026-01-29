using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Montage : MonoBehaviour
{
    public int montageID;
    public Sprite normalSprite;
    public Sprite hitSprite;

    SpriteRenderer sr;
    Coroutine hitRoutine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = normalSprite;
    }

    public void PlayHit()
    {
        if (hitSprite == null) return;

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        hitRoutine = StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        sr.sprite = hitSprite;
        yield return new WaitForSeconds(0.3f);
        sr.sprite = normalSprite;
    }
}
