using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Bottle2_4 : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Bottle References")]
    [SerializeField] private SpriteRenderer bottleRenderer;
    [SerializeField] private SpriteMask liquidMask;

    private Transform target;
    private bool isFilled = false;

    private void Awake()
    {
        if (bottleRenderer == null)
        {
            bottleRenderer = GetComponent<SpriteRenderer>();
        }

        bottleRenderer.sortingOrder = 10;
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 targetPosition = new Vector3(
            target.position.x,
            transform.position.y,
            transform.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Mathf.Abs(transform.position.x - target.position.x) < 0.01f)
        {
            Destroy(gameObject);
        }
    }

    public void FillBottle(GameObject liquidObject)
    {
        if (liquidObject == null || isFilled) return;

        isFilled = true;

        // 1. 보틀 자식으로 설정 및 기본 Transform 초기화
        liquidObject.transform.SetParent(transform);
        liquidObject.transform.localPosition = Vector3.zero;
        liquidObject.transform.localRotation = Quaternion.identity;
        liquidObject.transform.localScale = Vector3.one;

        SpriteRenderer liquidRenderer = liquidObject.GetComponent<SpriteRenderer>();

        if (liquidRenderer != null)
        {
            // 2. Sorting Order 설정 (보틀 바로 뒤)
            liquidRenderer.sortingOrder = bottleRenderer.sortingOrder - 1;

            // 3. 자식 SpriteMask 컴포넌트가 존재할 경우 잘라내기 적용
            if (liquidMask != null)
            {
                liquidRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }

            // 4. 어떤 모양의 필링이 들어오더라도 꽉 채워지도록 스케일 조정
            FitLiquidToBottle(liquidRenderer);
        }
    }

    private void FitLiquidToBottle(SpriteRenderer liquidRenderer)
    {
        if (bottleRenderer == null || bottleRenderer.sprite == null || liquidRenderer.sprite == null)
            return;

        Vector2 bottleSize = bottleRenderer.sprite.bounds.size;
        Vector2 liquidSize = liquidRenderer.sprite.bounds.size;

        if (liquidSize.x <= 0 || liquidSize.y <= 0) return;

        // 마스크 영역을 충분히 덮어 여백이 생기지 않도록 비율 산출 (* 1.1f)
        float scaleX = bottleSize.x / liquidSize.x;
        float scaleY = bottleSize.y / liquidSize.y;
        float finalScale = Mathf.Max(scaleX, scaleY) * 1.1f;

        liquidRenderer.transform.localScale = new Vector3(finalScale, finalScale, 1f);

        // 피벗 기준 위치 보정
        Vector3 bottleCenter = bottleRenderer.sprite.bounds.center;
        Vector3 liquidCenter = liquidRenderer.sprite.bounds.center;
        liquidRenderer.transform.localPosition = bottleCenter - liquidCenter;
    }
}
