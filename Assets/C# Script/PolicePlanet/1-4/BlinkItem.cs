using UnityEngine;

public class BlinkItem : MonoBehaviour
{
    public SpriteRenderer sr;
    public DragAndDrop drag;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        drag = GetComponent<DragAndDrop>();
    }

    public void SetVisible(bool value)
    {
        Color c = sr.color;
        c.a = value ? 1f : 0f;
        sr.color = c;

        drag.banDragging = !value;
    }
}
