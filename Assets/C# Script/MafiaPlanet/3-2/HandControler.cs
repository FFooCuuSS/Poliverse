using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControler : MonoBehaviour
{
    //public float speed = 2f;
    //int changeDirection=1;

    // Update is called once per frame
    public float leftLimit = -7f;   // ���� ��
    public float rightLimit = 7f;  // ������ ��
    public float horizontalSpeed = 2f;        // �̵� �ӵ�
    public float verticalSpeed = 0f;//���� �̵��ӵ�


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

        // �̵� ���⿡ ���� x ��ġ ����
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
