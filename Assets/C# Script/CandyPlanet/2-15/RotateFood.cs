using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateFood : MonoBehaviour
{
    public Transform center;
    public float speed = 50f;

    private void Update()
    {
        transform.RotateAround(center.position, Vector3.forward, speed * Time.deltaTime);
    }
}
