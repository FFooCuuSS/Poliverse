using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTester3_11 : MonoBehaviour
{
    private float timer = 0f;

    private void Update()
    {
        // 시간 누적
        timer += Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"입력 시간 tester : {timer:F2}초");
        }
    }
}
