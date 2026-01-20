using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControlerTest : MonoBehaviour
{
    public int caseNum;
    bool chooseCase;

    private void Update()
    {
        switch (caseNum)
        {
            case 1:
                StartCoroutine(Move1());
                caseNum = 0;
                break;
            case 2:
                StartCoroutine(Move2());
                caseNum = 0;
                break;

        }
    }
    void Start()
    {
        //chooseCase = false;

        
    }

    IEnumerator Move1()
    {
        Vector2 startPos = new Vector2(17.5f, 2);   // 시작 좌표
        Vector2 endPos = new Vector2(-12.5f, 2);     // 도착 좌표
        float moveTime = 6f; // 이동 시간
    float elapsed = 0f;

        // 시작 위치 강제 지정
        transform.position = startPos;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / moveTime;

            // 위치 보간
            transform.position = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        // 오차 방지용 최종 위치 고정
        transform.position = endPos;
    }
    IEnumerator Move2()
    {
        Vector2 startPos = new Vector2(15, 2);   // 시작 좌표
        Vector2 endPos = new Vector2(-15, 2);     // 도착 좌표
        float moveTime = 6f; // 이동 시간
        float elapsed = 0f;

        // 시작 위치 강제 지정
        transform.position = startPos;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / moveTime;

            // 위치 보간
            transform.position = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        // 오차 방지용 최종 위치 고정
        transform.position = endPos;
    }
}
