using UnityEngine;
using DG.Tweening;

public class UpDown : MonoBehaviour
{
    public GameObject ManagerObj;
    public bool goUp;
    public float speed = 1f;

    private Manager_1_10 manager;
    private bool isMoving;

    private void Awake()
    {
        ResolveManager();
    }

    private void ResolveManager()
    {
        if (manager != null) return;

        if (ManagerObj != null) manager = ManagerObj.GetComponent<Manager_1_10>();
        if (manager == null) manager = GetComponentInParent<Manager_1_10>();
    }

    private void OnMouseDown()
    {
        ResolveManager();

        if (manager != null)
            manager.RequestMoveFromPlatform(goUp);
    }

    public void TryMovePlatformImmediate()
    {
        if (isMoving) return;
        isMoving = true;

        float direction = goUp ? 1f : -1f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * direction * speed;

        // 1) 이동: 부드럽게 가속 (들어올 때 튀지 않음)
        transform.DOMove(targetPos, 0.8f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                // 2) 복귀: 바운스
                transform.DOMove(startPos, 0.5f)
                    .SetEase(Ease.OutBounce, 1.15f)
                    .OnComplete(() => isMoving = false);
            });
    }
}
