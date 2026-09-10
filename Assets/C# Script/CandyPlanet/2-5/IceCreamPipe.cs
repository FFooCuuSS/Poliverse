using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCreamPipe : MonoBehaviour
{
    [Header("변경될 이미지")]
    [SerializeField] private Sprite highlightedSprite;

    private SpriteRenderer spriteRenderer;
    private Sprite defaultSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            defaultSprite = spriteRenderer.sprite;
        }
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (spriteRenderer == null) return;

        if (isHighlighted && highlightedSprite != null)
        {
            spriteRenderer.sprite = highlightedSprite;
        }
        else
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }

    public float GetCenterX()
    {
        return transform.position.x;
    }
}
