using UnityEngine;

public class PlayerDrag : DragAndDrop
{
    [SerializeField] private float offsetX = 0f;
    [SerializeField] private float offsetY = 0f;
    [SerializeField] private GameObject stage_3_15;

    [Tooltip("피격 후 무적 시간. 레이저에 계속 닿아 있으면 이 간격으로 Miss")]
    [SerializeField] private float hitCooldown = 1f;

    private Minigame_3_15 minigame_3_15;
    private float lastHitTime = -999f;

    private void Start()
    {
        minigame_3_15 = stage_3_15.GetComponent<Minigame_3_15>();
    }

    protected override Vector3 GetConstrainedPosition(Vector3 current, Vector3 target)
    {
        float clampedX = Mathf.Clamp(target.x, -maxX + offsetX, maxX + offsetX);
        float clampedY = Mathf.Clamp(target.y, -maxY + offsetY, maxY + offsetY);
        return new Vector3(clampedX, clampedY, target.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FailureTag"))
            TakeHit($"페이즈2 지팡이 빛 피격 ({collision.name})");
    }

    // EndingWand에서 호출
    public void EndingBlast() => TakeHit("페이즈2 EndingWand 레이저 피격");

    public void TakeHit(string reason)
    {
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        if (minigame_3_15 != null) minigame_3_15.ReportMiss(reason);
    }
}