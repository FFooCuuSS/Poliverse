using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Prisoner_1_8 : MonoBehaviour
{
    public RectTransform prison;       // °¨¿Á Image
    public float moveSpeed = 100f;     // UI ´ÜÀ§¿¡¼­ ¼Óµµ (px/s)

    private RectTransform rectTransform;
    private UIDragAndDrop drag;
    private Vector2 moveDirection;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        drag = GetComponent<UIDragAndDrop>();

        SetNewDirection();
    }

    void Update()
    {
        if (drag != null && drag.IsDragging)
            return;

        rectTransform.anchoredPosition += moveDirection * moveSpeed * Time.deltaTime;

        // °¨¿Á°ú °ãÃÆ´ÂÁö È®ÀÎ
        if (RectOverlaps(rectTransform, prison))
        {
            SetNewDirection();
        }
    }

    void SetNewDirection()
    {
        Vector2 fromPrison = rectTransform.anchoredPosition - prison.anchoredPosition;
        if (fromPrison == Vector2.zero)
        {
            fromPrison = Random.insideUnitCircle.normalized;
        }

        float angle = Random.Range(-45f, 45f);
        moveDirection = Quaternion.Euler(0, 0, angle) * fromPrison.normalized;
    }

    bool RectOverlaps(RectTransform a, RectTransform b)
    {
        Rect aRect = GetWorldRect(a);
        Rect bRect = GetWorldRect(b);
        return aRect.Overlaps(bRect);
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector2 size = corners[2] - corners[0];
        return new Rect(corners[0], size);
    }
}
