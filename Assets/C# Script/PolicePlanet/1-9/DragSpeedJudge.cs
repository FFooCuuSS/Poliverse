using UnityEngine;

public class DragSpeedJudge : MonoBehaviour
{
    public RectTransform draggableObject; // �巡�� ��� ������Ʈ
    public float speedThreshold = 500f;   // ���� ���� �ӵ�

    private Vector2 lastPosition;
    private float lastTime;
    private float lastSpeed;

    private bool isDragging = false;

    void Update()
    {
        // ���콺 �������� �巡�� ���� ����
        if (Input.GetMouseButtonDown(0))
        {
            lastPosition = draggableObject.anchoredPosition;
            lastTime = Time.time;
            isDragging = true;
        }

        // �巡�� ���� �� �ӵ� ���
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 currentPos = draggableObject.anchoredPosition;
            float currentTime = Time.time;

            float deltaDist = Vector2.Distance(currentPos, lastPosition);
            float deltaTime = currentTime - lastTime;

            if (deltaTime > 0)
            {
                lastSpeed = deltaDist / deltaTime;
            }

            lastPosition = currentPos;
            lastTime = currentTime;
        }

        // �巡�� ������ �� ����/���� ����
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            if (lastSpeed >= speedThreshold)
            {
                Debug.Log("����! �巡�� �ӵ�: " + lastSpeed);
            }
            else
            {
                Debug.Log("����! �巡�� �ӵ�: " + lastSpeed);
            }
        }
    }
}
