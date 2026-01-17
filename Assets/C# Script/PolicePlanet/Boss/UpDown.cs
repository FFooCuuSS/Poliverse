using UnityEngine;
using DG.Tweening;

public class UpDown : MonoBehaviour
{
    public GameObject ManagerObj;
    public bool goUp;
    public float speed;

    private Manager_1_10 manager;

    void Start()
    {
        if (ManagerObj != null)
            manager = ManagerObj.GetComponent<Manager_1_10>();
    }

    private void OnMouseDown()
    {
        if (manager == null) return;
        if (manager.platformIsMoving) return;

        // 클릭 순간엔 "요청"만 한다. (플랫폼 이동/사람 이동은 판정 통과 후)
        manager.RequestMoveFromPlatform(goUp, this);
    }

    // Manager가 리듬 판정 통과 후 호출
    public void TriggerPlatformMove(float delay = 0.65f)
    {
        Invoke(nameof(MovePlatform), delay);
    }

    private void MovePlatform()
    {
        if (manager == null) return;

        float direction = goUp ? 1f : -1f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * direction * speed;

        transform.DOMove(targetPos, 0.3f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOMove(startPos, 0.25f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        manager.platformIsMoving = false;

                        // (선택) 플랫폼을 "재생성"하는 구버전 루프가 필요하면:
                        manager.SpawnPlatform(goUp ? Manager_1_10.PlatformType.Up : Manager_1_10.PlatformType.Down);
                        Destroy(gameObject);
                    });
            });
    }
}
