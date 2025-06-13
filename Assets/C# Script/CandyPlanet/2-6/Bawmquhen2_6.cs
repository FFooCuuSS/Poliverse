using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bawmquhen2_6 : MonoBehaviour
{
    private float rotationSpeed = -90f;
    [SerializeField] private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        rectTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
