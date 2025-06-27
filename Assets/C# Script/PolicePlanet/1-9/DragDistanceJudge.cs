using UnityEngine;
using DG.Tweening;

public class DragDistanceJudge : MonoBehaviour
{
    [SerializeField] private GameObject stage_1_9;
    [SerializeField] private float distanceThreshold = 5f;

    [Header("성공 시 움직일 오브젝트")]
    [SerializeField] private GameObject movingObject;

    [Header("성공 시 활성화할 오브젝트")]
    [SerializeField] private GameObject activateObject;

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
                StartShaking();              // 오브젝트 이동
                ActivateObject();          // 오브젝트 활성화
            }
        }
    }

    private void StartShaking()
    {
        movingObject.transform.DOShakePosition(
            duration: 1f,       // 진동 시간 (반복 주기)
            strength: 0.1f,     // 진동 폭
            vibrato: 20,        // 진동 빈도
            randomness: 90,     // 무작위 진동 각도
            snapping: false,
            fadeOut: false      // 페이드 아웃 비활성화 (계속 강하게 진동)
        )
        .SetLoops(-1);         // 무한 반복
    }

    private void ActivateObject()
    {
        if (activateObject != null)
        {
            activateObject.SetActive(true);
        }
    }
}
