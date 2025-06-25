using UnityEngine;

public class UILimitPosition : MonoBehaviour
{
    public Vector2 minPosition; // ���� �ּ� ��ǥ
    public Vector2 maxPosition; // ���� �ִ� ��ǥ

    public float returnSpeed = 5f; // ���� �ӵ�

    private Vector3 initialPosition;

    void Awake()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        ClampPosition();
        ReturnToInitialPosition();
    }

    private void ClampPosition()
    {
        Vector3 newPos = transform.position;

        newPos.x = Mathf.Clamp(newPos.x, minPosition.x, maxPosition.x);
        newPos.y = Mathf.Clamp(newPos.y, minPosition.y, maxPosition.y);

        transform.position = newPos;
    }

    private void ReturnToInitialPosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, initialPosition, returnSpeed * Time.deltaTime);
    }
}
