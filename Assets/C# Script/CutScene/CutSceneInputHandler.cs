using UnityEngine;
using UnityEngine.EventSystems;

public class CutsceneInputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private CutSceneLoader cutSceneLoader;

    [Header("스와이프 판정 거리")]
    [SerializeField] private float swipeThreshold = 120f;

    private Vector2 pointerDownPos;

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Vector2 pointerUpPos = eventData.position;
        Vector2 delta = pointerUpPos - pointerDownPos;

        if (Mathf.Abs(delta.x) >= swipeThreshold && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x < 0)
            {
                // 왼쪽으로 밀기 = 다음 슬라이드
                cutSceneLoader.GoNextSlide();
            }
            else
            {
                // 오른쪽으로 밀기 = 이전 슬라이드
                cutSceneLoader.GoPrevSlide();
            }

            return;
        }

        // 짧게 클릭/터치 = Step 진행
        cutSceneLoader.OnTap();
    }
}