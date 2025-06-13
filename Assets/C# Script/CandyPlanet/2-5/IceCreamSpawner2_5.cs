using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCreamSpawner2_5 : MonoBehaviour
{
    public GameObject iceCreamPrefab;
    public int maxIceCreamCount = 3;
    private int currentIceCreamCount = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (currentIceCreamCount < maxIceCreamCount)
            {
                SpawnIceCream();
            }
        }
    }

    void SpawnIceCream()
    {
        Instantiate(iceCreamPrefab, transform.position, Quaternion.identity);
        currentIceCreamCount++;
    }
}
