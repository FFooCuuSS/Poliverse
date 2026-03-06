using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Macaron : MonoBehaviour
{
    public int index;

    private Vector3 originalPos;
    private SpriteRenderer sr;
    private MacaroonPlate bowl;
    private bool isStacked = false;

    private DragAndDrop drag;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        bowl = FindObjectOfType<MacaroonPlate>();
        drag = GetComponent<DragAndDrop>();
        originalPos = transform.position;
    }

    void Update()
    {
        if (!isStacked && drag != null && !drag.isDragging && originalPos != transform.position)
        {
            float dist = Vector2.Distance(transform.position, bowl.transform.position);

            if (dist < 1.0f)
            {
                isStacked = true;
                drag.banDragging = true;
            }
            else
            {
                transform.position = originalPos;
            }

            sr.sortingOrder = 0;
        }
    }

    public void SetOrder(int order)
    {
        sr.sortingOrder = order;
    }

    public void MoveTo(Vector3 targetPos)
    {
        transform.position = targetPos;
    }

    public Vector3 GetOriginalPosition()
    {
        return originalPos;
    }

    void OnMouseDown()
    {
        Minigame_2_11 minigame = FindObjectOfType<Minigame_2_11>();
        if (minigame != null)
            minigame.OnPlayerInput("Tap");

        Fork_2_11 fork = FindObjectOfType<Fork_2_11>();
        if (fork != null)
            fork.GrabMacaron(gameObject);
    }
}
