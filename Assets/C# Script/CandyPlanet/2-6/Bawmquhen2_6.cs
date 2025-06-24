using UnityEngine;

public class Bawmquhen2_6 : MonoBehaviour
{
    public float jumpHeight = 200f;
    public float jumpSpeed = 800f;
    public float rotationSpeed = -90f;

    private bool isJumping = false;
    private bool isFalling = false;

    private float targetY;
    private float originalY;

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError("RectTransform ������Ʈ�� ã�� �� �����ϴ�!");
        }
        else
        {
            originalY = rectTransform.anchoredPosition.y;
        }
    }

    void Update()
    {
        if (rectTransform == null) return;

        // ȸ��
        rectTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        // Ŭ�� �� ���� ����
        if (Input.GetMouseButtonDown(0) && !isJumping && !isFalling)
        {
            Jump();
        }

        HandleJump();
    }

    void Jump()
    {
        isJumping = true;
        originalY = rectTransform.anchoredPosition.y;
        targetY = originalY + jumpHeight;
    }

    void HandleJump()
    {
        if (isJumping)
        {
            float newY = Mathf.MoveTowards(rectTransform.anchoredPosition.y, targetY, jumpSpeed * Time.deltaTime);
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);

            if (Mathf.Approximately(newY, targetY))
            {
                // ���� �� �� ���� ����
                isJumping = false;
                isFalling = true;
            }
        }
        else if (isFalling)
        {
            float newY = Mathf.MoveTowards(rectTransform.anchoredPosition.y, originalY, jumpSpeed * Time.deltaTime);
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);

            if (Mathf.Approximately(newY, originalY))
            {
                // ���� ��
                isFalling = false;
            }
        }
    }
}
