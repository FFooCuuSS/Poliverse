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
            bool isVisible = !sr.enabled;

            sr.enabled = isVisible;
            drag.banDragging = !isVisible;

            yield return new WaitForSeconds(interval);
        }
    }
}
