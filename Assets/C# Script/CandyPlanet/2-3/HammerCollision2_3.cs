using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerCollision2_3 : MonoBehaviour
{
    public RectTransform playerRect;

    void Update()
    {
        if (IsOverlapping(transform as RectTransform, playerRect))
        {
            Debug.Log("UI 망치 머리가 플레이어와 충돌했습니다!");
        }
    }

    bool IsOverlapping(RectTransform rect1, RectTransform rect2)
    {
        Rect r1 = GetWorldRect(rect1);
        Rect r2 = GetWorldRect(rect2);
        return r1.Overlaps(r2);
    }

    Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Vector3 bottomLeft = corners[0];
        Vector2 size = new Vector2(
            corners[2].x - corners[0].x,
            corners[2].y - corners[0].y
        );
        return new Rect(bottomLeft, size);
    }
}
