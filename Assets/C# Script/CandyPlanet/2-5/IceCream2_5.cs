using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCream2_5 : MonoBehaviour
{
    public float fallSpeed = 5f; // �������� �ӵ�
    private bool isStopped = false;
    
    

    void Update()
    {
        if (isStopped)
        {

            
        }
    }

    // �ٱ��Ͽ� ������ ����
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Basket"))
        {
            isStopped = true;
            
        }
        if(other.CompareTag("Floor"))
        {

        }
    }
}
