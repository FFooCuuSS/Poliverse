using UnityEngine;

public class HoldUIClickRelay1_7 : MonoBehaviour
{
    private HoldCheck1_7 owner;

    public void SetOwner(HoldCheck1_7 target)
    {
        owner = target;
    }

    private void OnMouseDown()
    {
        owner?.OnHoldUIClicked();
    }
}