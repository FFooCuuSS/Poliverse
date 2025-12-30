using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class enemy_1_1_test : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;
    }

    public void Highlight(bool on)
    {
        if (sr == null) return;

        sr.color = on ? Color.yellow : originalColor;
    }

    public void Clear()
    {
        Highlight(false);
        gameObject.SetActive(false);
    }

    public void ResetEnemy()
    {
        Highlight(false);
    }
}
