using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCreamPipe : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;

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
        if (spriteRenderer == null)
            return;

        if (isHighlighted && highlightedSprite != null)
        {
            spriteRenderer.sprite = highlightedSprite;
        }
        else
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }

    // 실제 플레이어 입력 등에서 필요할 때만 호출
    public void PlayClickSound()
    {
        if (clickSound == null)
            return;

        if (GameRoot.Instance == null)
            return;

        if (GameRoot.Instance.Audio == null)
            return;

        GameRoot.Instance.Audio.PlaySfx(clickSound);
    }

    public float GetCenterX()
    {
        return transform.position.x;
    }
}
