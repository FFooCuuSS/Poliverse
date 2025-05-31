using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript: MonoBehaviour
{
    public float moveSpeed = 50f; // Speed at which the camera moves

    void Update()
    {
        float input=0f;
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            input = -1f;
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            input = 1f;
        }
        // Get horizontal input (-1 for left, 1 for right)
       //float moveInput = Input.GetAxisRaw("Horizontal");

        // Calculate movement vector
        Vector3 movement = new Vector3(input * moveSpeed * Time.deltaTime, 0f, 0f);

        // Apply movement to the camera's position
        transform.position += movement;
    }
}

