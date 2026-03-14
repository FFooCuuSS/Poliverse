using UnityEngine;

public class UILimitPosition : MonoBehaviour
{
    [Header("Position Limit")]
    [SerializeField] private Vector2 minPosition;
    [SerializeField] private Vector2 maxPosition;

    [Header("Return")]
    [SerializeField] private float returnSpeed = 5f;

    private Vector3 initialPosition;
    private bool isDragging;
    private float dragZ;
    private Vector3 dragOffset;

    private Camera mainCam;

    private void Awake()
    {
        initialPosition = transform.position;
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (isDragging)
        {
            FollowMouse();
            ClampPosition();
        }
        else
        {
            ReturnToInitialPosition();
        }
    }

    private void FollowMouse()
    {
        if (mainCam == null) return;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = dragZ;

        Vector3 worldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        worldPos += dragOffset;
        worldPos.z = initialPosition.z;

        transform.position = worldPos;
    }

    private void ClampPosition()
    {
        Vector3 newPos = transform.position;

        newPos.x = Mathf.Clamp(newPos.x, minPosition.x, maxPosition.x);
        newPos.y = Mathf.Clamp(newPos.y, minPosition.y, maxPosition.y);
        newPos.z = initialPosition.z;

        transform.position = newPos;
    }

    private void ReturnToInitialPosition()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            initialPosition,
            returnSpeed * Time.deltaTime
        );
    }

    private void OnMouseDown()
    {
        isDragging = true;

        if (mainCam == null)
            mainCam = Camera.main;

        Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position);
        dragZ = screenPos.z;

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragZ)
        );

        dragOffset = transform.position - mouseWorld;
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }
}