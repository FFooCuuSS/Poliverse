using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateFood : MonoBehaviour
{
    public float rotationSpeed = 30f;
    private Material donutMaterial;

    private void Start()
    {
        donutMaterial = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        float currentRotation = transform.eulerAngles.z;
        donutMaterial.SetFloat("_Rotation", currentRotation);
    }
}
