using System.Collections;
using UnityEngine;

public class ThrowEffact : MonoBehaviour
{
    [Header("Player Sprite Renderers")]
    [SerializeField]
    private SpriteRenderer[] playerSpriteRenderers;

    [Header("Idle Sprites")]
    [SerializeField]
    private Sprite[] idleSprites;

    [Header("Throw Sprites")]
    [SerializeField]
    private Sprite[] throwSprites;

    [SerializeField]
    private float throwDuration = 0.5f;

    private Coroutine throwCoroutine;

    private void Awake()
    {
        SetSprites(idleSprites);
    }

    public void Throw()
    {
        if (throwCoroutine != null)
        {
            StopCoroutine(throwCoroutine);
        }

        throwCoroutine = StartCoroutine(ThrowCoroutine());
    }

    private IEnumerator ThrowCoroutine()
    {
        SetSprites(throwSprites);

        yield return new WaitForSeconds(throwDuration);

        SetSprites(idleSprites);
        throwCoroutine = null;
    }

    private void SetSprites(Sprite[] sprites)
    {
        if (playerSpriteRenderers == null || sprites == null)
        {
            return;
        }

        int count = Mathf.Min(
            playerSpriteRenderers.Length,
            sprites.Length
        );

        for (int i = 0; i < count; i++)
        {
            if (playerSpriteRenderers[i] != null)
            {
                playerSpriteRenderers[i].sprite = sprites[i];
            }
        }
    }

    private void OnDisable()
    {
        if (throwCoroutine != null)
        {
            StopCoroutine(throwCoroutine);
            throwCoroutine = null;
        }

        SetSprites(idleSprites);
    }
}