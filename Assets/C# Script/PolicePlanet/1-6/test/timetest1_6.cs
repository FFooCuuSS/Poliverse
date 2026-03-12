using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timetest1_6 : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // 우클릭 감지
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("현재 Time.time : " + Time.time);
        }
    }
}