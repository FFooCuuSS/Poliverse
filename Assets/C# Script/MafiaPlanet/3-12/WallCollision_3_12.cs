using UnityEngine;

public class WallCollision_3_12 : MonoBehaviour
{
    public LayerMask wallLayer;
    public Collider2D realCollider;

    private DragAndDrop drag;

    void Awake()
    {
        drag = GetComponent<DragAndDrop>();
    }

    void Update()
    {
        if (!drag.isDragging) return;

        bool isTouching = realCollider.IsTouchingLayers(wallLayer);
        drag.banDragging = isTouching;

        Debug.Log("∫Æ ¥Í¿Ω ø©∫Œ: " + isTouching);
    }
}
