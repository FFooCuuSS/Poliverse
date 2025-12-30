using UnityEngine;

public class DragZoneChecker : MonoBehaviour
{
    private DragAndDrop drag;

    void Awake()
    {
        drag = GetComponent<DragAndDrop>();
        drag.banDragging = true; // 기본 금지
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DragZone"))
        {
            Debug.Log("dragzone입장");
            drag.banDragging = false; // 허용
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("DragZone"))
        {
            Debug.Log("dragzone끝");

            drag.banDragging = true; // 다시 금지
        }
    }
}
