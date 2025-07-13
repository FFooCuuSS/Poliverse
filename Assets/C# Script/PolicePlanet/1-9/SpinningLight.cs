using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningLight : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 0, 180); // �ʴ� 90�� ȸ��

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
