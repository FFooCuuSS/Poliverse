using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSr : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite fallLeftSprite;
    [SerializeField] private Sprite fallRightSprite;
    [SerializeField] private float fallDuration = 0.4f;

    private Coroutine fallRoutine;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    // isLeft: true면 왼쪽으로 넘어지는 스프라이트, false면 오른쪽
    public void ShowFall(bool isLeft)
    {
        if (fallRoutine != null)
        {
            StopCoroutine(fallRoutine);
        }
        fallRoutine = StartCoroutine(FallRoutine(isLeft));
    }

    private IEnumerator FallRoutine(bool isLeft)
    {
        spriteRenderer.sprite = isLeft ? fallLeftSprite : fallRightSprite;
        yield return new WaitForSeconds(fallDuration);
        spriteRenderer.sprite = normalSprite;
        fallRoutine = null;
    }
}