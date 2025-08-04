using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoopDrag : MonoBehaviour
{
    public Transform centerPoint;
    public float radius = 1.5f;
    public float CurSpeed { get; private set; }

    private bool isDragging = false;
    private float originalZRotation;
    private Vector2 lastPosition;
    private float lastTime;



    private void Start()
    {
        originalZRotation = transform.localEulerAngles.z;
        lastPosition = transform.localPosition;
        lastTime = Time.time;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D col = GetComponent<Collider2D>();

            if (col != null && col.OverlapPoint(mouseWorld))
            {
                isDragging = true;
                lastPosition = transform.position;
            }
        }

        if(Input.GetMouseButtonUp(0)) isDragging = false;

        if (isDragging)
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 center = centerPoint.position;

            Vector2 direction = (mouseWorld - center).normalized;
            Vector2 newPos = center + direction * radius;

            transform.position = newPos;

            if(transform.position.x < centerPoint.position.x)
            {
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, -originalZRotation);
            }
            if (transform.position.x > centerPoint.position.x)
            {
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, originalZRotation);
            }

            float distance = Vector2.Distance(transform.position, lastPosition);
            float deltaTime = Time.time - lastTime;
            if(deltaTime > 0)
            {
                CurSpeed = distance / deltaTime;
            }

            lastPosition = transform.position;
            lastTime = Time.time;
        }
    }
}
