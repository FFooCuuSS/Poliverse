using UnityEngine;
using System.Collections;

public class BlinkAndControll : MonoBehaviour
{
    public float interval = 1f;

    private SpriteRenderer sr;
    private DragAndDrop drag; 

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        drag = GetComponent<DragAndDrop>();

        StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            bool isVisible = sr.color.a == 0f;

            Color c = sr.color;
            c.a = isVisible ? 1f : 0f;
            sr.color = c;

            drag.banDragging = !isVisible;

            yield return new WaitForSeconds(interval);
        }
    }

}
