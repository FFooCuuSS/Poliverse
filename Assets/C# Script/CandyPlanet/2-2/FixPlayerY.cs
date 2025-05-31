using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixPlayerY : MonoBehaviour
{
    private float fixedY;

    private void Start()
    {
        fixedY = transform.position.y;
    }
    private void LateUpdate()
    {
        transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);
    }
}
