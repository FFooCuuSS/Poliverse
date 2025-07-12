using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketMove : MonoBehaviour
{
    [SerializeField] private float maxX;
    [SerializeField] private float moveSpeed;

    private int direction = 1; //1 right, -1 left

    private void Update()
    {
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);

        if(transform.position.x >= maxX) direction = -1;
        if(transform.position.x <= -maxX) direction = 1;
    }
}
