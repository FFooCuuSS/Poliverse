using UnityEngine;
using System.Collections;

public class BlinkAndControll : MonoBehaviour
{
    public float interval = 1f;

    private SpriteRenderer sr;
    private DragAndDrop drag; // 당신이 만든 스크립트

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
            drag.enabled = isVisible; // ⭐ 핵심

            yield return new WaitForSeconds(interval);
        }
    }
}
