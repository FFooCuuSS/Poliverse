using UnityEngine;

public class Fish4_5 : MonoBehaviour
{
    [Tooltip("왼쪽 복어면 체크 (CSV의 Input과 연결), 오른쪽이면 해제 (Tap과 연결)")]
    public bool isLeft = true;
    public Minigame4_5 owner;

    [Header("Sprites")]
    public SpriteRenderer sr;
    public Sprite normalSprite;    // 비워 두면 시작할 때 이미지를 자동으로 씀
    public Sprite puffedSprite;    // 부푼 이미지
    public float puffTime = 0.3f;

    private void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (normalSprite == null && sr != null) normalSprite = sr.sprite;
    }

    // 복어에 Collider2D가 있어야 클릭이 잡힌다
    private void OnMouseDown()
    {
        if (owner != null) owner.OnFishClicked(this);
    }

    public void Puff()
    {
        if (sr != null && puffedSprite != null) sr.sprite = puffedSprite;
        CancelInvoke(nameof(Unpuff));
        Invoke(nameof(Unpuff), puffTime);
    }

    private void Unpuff()
    {
        if (sr != null && normalSprite != null) sr.sprite = normalSprite;
    }
}