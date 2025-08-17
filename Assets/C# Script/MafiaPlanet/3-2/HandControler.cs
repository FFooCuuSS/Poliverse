using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControler : MiniGameBase
{
    //public float speed = 2f;
    //int changeDirection=1;

    // Update is called once per frame
    
    public float leftLimit = -2.3f;   // 왼쪽 끝
    public float rightLimit = 7f;  // 오른쪽 끝
    public float horizontalSpeed = 2f;        // 이동 속도
    public float verticalSpeed = 0f;//수직 이동속도


    private bool movingRight;
    public bool isBag;
    public bool missionFail;
    bool bagFail;
    bool success;
    private void Start()
    {
        if(transform.position.x<=0)
        {
           movingRight = true;
        }
        isBag = false;
        bagFail = false;
        missionFail = false;
        success = false;


    }

    void Update()
    {
        MovingHand();
        ChooseBag();
        /*
         if(isBag)
        {
            verticalSpeed = 2f;
            BagSuccess();
        }   */

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
        Vector2 yPos = transform.position;

        if (Input.GetMouseButton(0))
        {
            horizontalSpeed = 0f;
            verticalSpeed = 2f;
        }
        if(isBag)
        {
            verticalSpeed = -2f;
        }
        if (isBag && yPos.y >= 2)
        {
            verticalSpeed = 0f;
            if(bagFail)
            {
                missionFail = true;
                base.Fail();
            }
            else
            {
                base.Success();
            }

        }
        yPos.y -= verticalSpeed * Time.deltaTime;
        transform.position = yPos;
        if(yPos.y<-5)
        {
            Debug.Log("Fail");
        }
        

    }
    void BagSuccess()
    {
        Vector2 yPos = transform.position;
        yPos.y += verticalSpeed * Time.deltaTime;
        transform.position = yPos ;
        if(yPos.y>=2)
        {
            verticalSpeed = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.GetComponent<Bag3_2>().isBag = true;
        isBag = true;


        if (collision.CompareTag("Bag"))
        {
            //verticalSpeed = -2f;
            Debug.Log("isBag");
        }
        else if (!collision.CompareTag("Bag"))
        {
            Debug.Log("isNotBag");
            bagFail = true;
        }
    }
    
}
