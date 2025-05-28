using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControler : MonoBehaviour
{
    //public float speed = 2f;
    //int changeDirection=1;

    // Update is called once per frame
    public float leftLimit = -7f;   // 왼쪽 끝
    public float rightLimit = 7f;  // 오른쪽 끝
    public float horizontalSpeed = 2f;        // 이동 속도
    public float verticalSpeed = 0f;//수직 이동속도


    private bool movingRight;
    private void Start()
    {
        if(transform.position.x<=0)
        {
           movingRight = true;
        }

    }

    void Update()
    {
        MovingHand();
        ChooseBag();

    }
    void MovingHand()
    {
        Vector2 xPos = transform.position;

        // 이동 방향에 따라 x 위치 변경
        if (movingRight)
        {
            xPos.x += horizontalSpeed * Time.deltaTime;
            if (xPos.x >= rightLimit)
                movingRight = false;
        }
        else
        {
            xPos.x -= horizontalSpeed * Time.deltaTime;
            if (xPos.x <= leftLimit)
                movingRight = true;
        }

        transform.position = xPos;
    }
    void ChooseBag()
    {
        if (Input.GetMouseButton(0))
        {
            horizontalSpeed = 0f;
            verticalSpeed = 2f;
        }
        Vector2 yPos = transform.position;
        yPos.y -= verticalSpeed * Time.deltaTime;
        transform.position = yPos;
        if(yPos.y<-5)
        {
            Debug.Log("Fail");
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bag"))
        {
            verticalSpeed = 0f;
            Debug.Log("isBag");
        }
        else if (!collision.CompareTag("Bag"))
        {
            verticalSpeed = 0f;
            Debug.Log("isNotBag");
        }
    }
    
}
