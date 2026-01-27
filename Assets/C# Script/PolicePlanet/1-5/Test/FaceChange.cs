using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceChange : MonoBehaviour
{
    public Sprite normalSprite;   // 기본 스프라이트
    public Sprite hitSprite;      // 바꿀 스프라이트

    private SpriteRenderer sr;
    private void Start()
    {
        ChangeToNormal();
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void ChangeToHit()
    {
        if (sr == null) return;
        sr.sprite = hitSprite;
    }

    public void ChangeToNormal()
    {
        if (sr == null) return;
        sr.sprite = normalSprite;
    }
}
