using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCreamFloor : MonoBehaviour
{
    public Transform startPoint;
    public float offsetX = 1.2f;

    private int index = 0;

    public void RegisterIceCream(Transform iceCream)
    {
        Vector3 target = GetNextSlotPosition();

        IceCream2_5 ic = iceCream.GetComponent<IceCream2_5>();
        ic.SetStoreTarget(target);
    }

    public Vector3 GetNextSlotPosition()
    {
        Vector3 pos = startPoint.position + new Vector3(offsetX * index, 0f, 0f);
        index++;
        return pos;
    }

    public void OnIceCreamLanded(Transform iceCream)
    {
        Vector3 target = GetNextSlotPosition();

        IceCream2_5 ic = iceCream.GetComponent<IceCream2_5>();

        ic.SetStoreTarget(target);
        ic.StartStoreMove();
    }
}
