using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundScroller2_6 : MonoBehaviour
{
    public float scrollSpeedX = 0f;
    public float scrollSpeedY = 0.2f;
    private Material mat;
    private float offsetX;
    private float offsetY;

    void Start()
    {
        mat = GetComponent<SpriteRenderer>().material;
    }

    void Update()
    {
        offsetX += scrollSpeedX * Time.deltaTime;
        offsetY += scrollSpeedY * Time.deltaTime;
        mat.SetFloat("_ScrollX", offsetX);
        mat.SetFloat("_ScrollY", offsetY);
    }
}
