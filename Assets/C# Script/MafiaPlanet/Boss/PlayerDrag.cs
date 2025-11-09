using UnityEngine;

public class PlayerDrag : DragAndDrop
{
    [SerializeField] private float offsetX = 0f;
    [SerializeField] private float offsetY = 0f;
    [SerializeField] private GameObject stage_3_15;
    private Minigame_3_15 minigame_3_15;

    private void Start()
    {
        minigame_3_15 = stage_3_15.GetComponent<Minigame_3_15>();
    }

    protected override Vector3 GetConstrainedPosition(Vector3 current, Vector3 target)
    {
        // 부모에서 지정한 maxX, maxY 활용
        float clampedX = Mathf.Clamp(target.x, -maxX + offsetX, maxX + offsetX);
        float clampedY = Mathf.Clamp(target.y, -maxY + offsetY, maxY + offsetY);

        return new Vector3(clampedX, clampedY, target.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FailureTag"))
        {
            minigame_3_15.Failure();
            banDragging = true;
        }
    }

    public void EndingBlast()
    {
        minigame_3_15.Failure();
        banDragging = true;
        Debug.Log("end");
    }
}
