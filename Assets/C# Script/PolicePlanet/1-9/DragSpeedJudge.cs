using UnityEngine;

public class DragSpeedJudge : MonoBehaviour
{
    public RectTransform draggableObject; // 드래그 대상 오브젝트
    public float speedThreshold = 500f;   // 성공 기준 속도

    private Vector2 lastPosition;
    private float lastTime;
    private float lastSpeed;

    private bool isDragging = false;

    void Update()
    {
        // 마우스 눌림으로 드래그 시작 감지
        if (Input.GetMouseButtonDown(0))
        {
            lastPosition = draggableObject.anchoredPosition;
            lastTime = Time.time;
            isDragging = true;
        }

        // 드래그 중일 때 속도 계산
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

        // 드래그 끝났을 때 성공/실패 판정
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            if (lastSpeed >= speedThreshold)
            {
                Debug.Log("성공! 드래그 속도: " + lastSpeed);
            }
            else
            {
                Debug.Log("실패! 드래그 속도: " + lastSpeed);
            }
        }
    }
}
