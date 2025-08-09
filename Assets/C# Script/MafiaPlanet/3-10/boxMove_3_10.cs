using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class boxMove_3_10 : MonoBehaviour
{

    private Vector3 targetPosition;
    private float moveSpeed=5f;


    // Update is called once per frame
    void Update()
    {
        targetPosition = new Vector3(-11, this.transform.position.y, this.transform.position.z);
        this.transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if(this.transform.position.x<-10.5)
        {
            Destroy(gameObject);
        }
    }
}
