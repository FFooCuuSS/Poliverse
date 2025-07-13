using UnityEngine;
using DG.Tweening;

public class DragDistanceJudge : MonoBehaviour
{
    [SerializeField] private GameObject stage_1_9;
    [SerializeField] private float distanceThreshold = 5f;

    [Header("���� �� ������ ������Ʈ")]
    [SerializeField] private GameObject movingObject;

    [Header("���� �� Ȱ��ȭ�� ������Ʈ")]
    [SerializeField] private GameObject activateObject;
    [SerializeField] private GameObject lightEffect;

    [Header("���� �� �ٲ� ��������Ʈ ����")]
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private Sprite successSprite;

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
                StartShaking();
                ActivateObject();
                ChangeSprite();
            }
        }
    }

    private void StartShaking()
    {
        movingObject.transform.DOShakePosition(
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
        {
            activateObject.SetActive(true);
            lightEffect.SetActive(true);
        }
    }

    private void ChangeSprite()
    {
        if (targetSpriteRenderer != null && successSprite != null)
            targetSpriteRenderer.sprite = successSprite;
    }
}
