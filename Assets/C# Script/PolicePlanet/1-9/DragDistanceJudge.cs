using UnityEngine;

public class DragDistanceJudge : MonoBehaviour
{
    [SerializeField] private GameObject stage_1_9;
    [SerializeField] private float distanceThreshold = 5f; // 성공 기준 총 이동 거리

    private Minigame_1_9 minigmae_1_9;
    private Vector3 lastPosition;
    private float totalDistance;

    private bool isDragging = false;

    private void Start()
    {
        minigmae_1_9 = stage_1_9.GetComponent<Minigame_1_9>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastPosition = this.transform.position;
            totalDistance = 0f;
            isDragging = true;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 currentPos = this.transform.position;
            float delta = Vector3.Distance(currentPos, lastPosition);

            totalDistance += delta;

            lastPosition = currentPos;
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            if (totalDistance >= distanceThreshold)
            {
                minigmae_1_9.Succeed();
            }
        }
    }
}
