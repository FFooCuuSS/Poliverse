using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSrChange : MonoBehaviour
{
    [SerializeField] private Sprite tempSprite;
    [SerializeField] private float changeDuration = 0.3f;

    private SpriteRenderer srRenderer;
    private Sprite originalSr;

    private void Awake()
    {
        srRenderer = GetComponent<SpriteRenderer>();
        originalSr = srRenderer.sprite;
    }
    public void ChangeSpriteTemporarily()
    {
        StopAllCoroutines();
        StartCoroutine(ChangeSpriteCoroutine());
     
    }
    private IEnumerator ChangeSpriteCoroutine()
    {
        srRenderer.sprite = tempSprite;
        yield return new WaitForSeconds(changeDuration);
        srRenderer.sprite = originalSr;
    }
}
