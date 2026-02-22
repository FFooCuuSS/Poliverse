using UnityEngine;

public class TimeTester : MonoBehaviour
{
    float timer = 0f;
    float counter = 0.5f;
    void Update()
    {
        timer += Time.deltaTime; // 프레임마다 누적

        if (timer >= 0.5f)
        {
            Debug.Log(counter+"초 지남"+Time.time);
            timer = 0f; // 초기화
            counter+=0.5f;
        }
    }
}