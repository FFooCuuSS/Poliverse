using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEnemyRemake3_10 : MonoBehaviour
{
    float moveSpeed = 30f;
    float destroyX = -11f;

    private void Update()
    {
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        if(transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}
