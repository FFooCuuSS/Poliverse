using UnityEngine;

public class HandAutoMove : MonoBehaviour
{
    public float speed = 2f;
    public float targetY;

    public bool isMoving = false;
    public bool hasArrived = false;

    void Update()
    {
        if (!isMoving || hasArrived) return;

        Vector3 pos = transform.position;
        pos.y -= speed * Time.deltaTime;

        if (pos.y <= targetY)
        {
            pos.y = targetY;
            hasArrived = true;
            isMoving = false;
        }

        transform.position = pos;
    }

    public void StartMove()
    {
        isMoving = true;
        hasArrived = false;
    }
}
