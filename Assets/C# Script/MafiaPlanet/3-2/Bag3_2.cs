using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bag3_2 : MonoBehaviour
{
    // Start is called before the first frame update
    float verticalSpeed;
    public bool isBag;
    void Start()
    {
        verticalSpeed = -2f;
        isBag = false;

    }

    // Update is called once per frame
    void Update()
    {
        if(isBag)
        {
            moveBag();
        }
    }
    void moveBag()
    {

        Vector2 yPos = transform.position;
        if(yPos.y>=0.1)
        {
            verticalSpeed = 0f;
        }
        yPos.y -= verticalSpeed * Time.deltaTime;
        transform.position = yPos;
    }
}
