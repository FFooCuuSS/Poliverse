using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardColor : MonoBehaviour
{
    public bool isTrapCard = false;

    // 인스펙터에서 넣어줄 스프라이트
    [SerializeField] private Sprite trapSprite;
    [SerializeField] private Sprite normalSprite;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // SpriteRenderer 캐싱
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void IsSetTrap()
    {
        isTrapCard = true;
        spriteRenderer.sprite = trapSprite;
    }

    public void IsNotTrap()
    {
        isTrapCard = false;
        spriteRenderer.sprite = normalSprite;
    }
}
