using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCreamSpawner2_5 : MonoBehaviour
{
    public GameObject iceCreamPrefab;

    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform rightSidePoint; // Ãß°¡

    public IceCream2_5 SpawnIceCream()
    {
        GameObject obj = Instantiate(
            iceCreamPrefab,
            startPoint.position,
            Quaternion.identity
        );

        IceCream2_5 iceCream = obj.GetComponent<IceCream2_5>();

        iceCream.SetFlyTarget(rightSidePoint.position);

        return iceCream;
    }
}
