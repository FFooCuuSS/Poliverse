using UnityEngine;
using DG.Tweening;

public class DragDistanceJudge : MonoBehaviour
{
    [Header("미니게임 참조")]
    [SerializeField] private Minigame_1_9 minigame_1_9;

    [Header("드래그 판정")]
    [SerializeField] private float distanceThreshold = 5f;

    [Header("성공 연출")]
    [SerializeField] private GameObject movingObject;
    [SerializeField] private GameObject activateObject;
    [SerializeField] private GameObject lightEffect;

    [Header("성공 시 스프라이트 변경")]
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private Sprite successSprite;

    private Vector3 lastPosition;
    private float totalDistance;
    private bool isDragging;

    private void OnMouseDown()
    {
        lastPosition = transform.position;
        totalDistance = 0f;
        isDragging = true;
    }

    // 드래그 중 이동 거리 누적
    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 currentPos = transform.position;
        totalDistance += Vector3.Distance(currentPos, lastPosition);
        lastPosition = currentPos;
    }

    // 드래그 종료 시 판정
    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        // minigame_1_9 null 체크
        if (minigame_1_9 != null)
            minigame_1_9.SubmitPlayerInput("Input");

        // 거리 기준 성공
        if (totalDistance >= distanceThreshold)
        {
            Debug.Log("성공");

            //minigame_1_9.NotifySuccess();
            //StartShaking();
            //ActivateObject();
            //ChangeSprite();
        }
    }

    private void StartShaking()
    {
        if (movingObject == null) return;

        movingObject.transform
            .DOShakePosition(
                duration: 1f,
                strength: 0.1f,
                vibrato: 20,
                randomness: 90,
                snapping: false,
                fadeOut: false
            )
            .SetLoops(-1);
    }

    private void ActivateObject()
    {
        if (activateObject != null)
            activateObject.SetActive(true);

        if (lightEffect != null)
            lightEffect.SetActive(true);
    }

    private void ChangeSprite()
    {
        if (targetSpriteRenderer != null && successSprite != null)
            targetSpriteRenderer.sprite = successSprite;
    }


    //public void NotifyMiss()
    //{
    //    minigame_1_9.OnMiss();
    //}
}
