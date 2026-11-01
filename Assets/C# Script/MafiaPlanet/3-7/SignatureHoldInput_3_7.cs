using UnityEngine;
using UnityEngine.EventSystems;

// 3-7 홀드 입력. 화면 어디든 누르고 있으면 IsHolding = true.
// UI 레이캐스트가 막혀도 동작하도록 마우스/터치 상태를 직접 읽는다.
public class SignatureHoldInput_3_7 : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool IsHolding { get; private set; }

    private void Update()
    {
        bool held = Input.GetMouseButton(0);

        if (held && !IsHolding && Input.GetMouseButtonDown(0))
        {
            IsHolding = true;
        }
        else if (!held && IsHolding)
        {
            IsHolding = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsHolding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsHolding = false;
    }

    public void ResetInput()
    {
        IsHolding = false;
    }

    private void OnDisable()
    {
        IsHolding = false;
    }
}