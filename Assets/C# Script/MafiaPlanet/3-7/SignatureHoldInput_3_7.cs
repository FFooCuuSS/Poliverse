using UnityEngine;
using UnityEngine.EventSystems;

public class SignatureHoldInput_3_7 : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool IsHolding { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsHolding = true;
        Debug.Log("[3-7] Hold Start");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsHolding = false;
        Debug.Log("[3-7] Hold End");
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