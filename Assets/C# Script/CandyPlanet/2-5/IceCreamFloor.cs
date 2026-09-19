using UnityEngine;

public class IceCreamFloor : MonoBehaviour
{
    public void OnIceCreamLanded(Transform iceCream)
    {
        IceCream2_5 ic = iceCream.GetComponent<IceCream2_5>();
        //ic.StartConveyorMove();
    }
}