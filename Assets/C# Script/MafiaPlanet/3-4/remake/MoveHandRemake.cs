using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveHandRemake : MonoBehaviour
{
    public GameObject hand;

    // 손의 시작 위치
    Vector3 startPos = new Vector3(-6.0f, 2.0f, 0f);
    Vector3 startSet = new Vector3(-6.0f, 2.0f, 0f);

    // 한 박 = 0.6초
    float duration = 0.6f;

    // 이 스크립트에서 직접 계산하는 현재 시간
    private float currTime = 0f;

    bool setHand = false;
    bool startHand = false;

    // ===== 이동 상태 =====
    private bool isMoving = false;

    // 이동이 시작된 시간을 저장
    private float moveStartTime = 0f;

    private Vector3 moveFrom;
    private Vector3 moveTo;

    // 첫 이동 시작 시간
    // 6.6초에 손 세팅 후
    // 1박(0.6초) 뒤인 7.2초에 첫 이동 시작
    private float nextMoveTime = 0f;

    // 한 번 이동할 때 X축으로 +2
    private float stepX = 2f;

    // X가 6에 도착하면 종료
    private float stopX = 6f;


    void Update()
    {
        // Time.time을 직접 사용하지 않고
        // 매 프레임 지난 시간을 누적해서 자체 시간 계산
        currTime += Time.deltaTime;


        // =========================
        // 6.0초
        // 손을 위쪽 위치에 세팅
        // =========================
        if (!setHand && currTime >= 6.0f)
        {
            hand.transform.position = startPos;

            setHand = true;

            Debug.Log("[MoveHand] 손 등장 : " + currTime);
        }


        // =========================
        // 6.6초
        // 손을 아래쪽 시작 위치에 세팅
        // =========================
        if (!startHand && currTime >= 6.6f)
        {
            hand.transform.position = startSet;

            startHand = true;

            Debug.Log("[MoveHand] 이동 준비 : " + currTime);
        }


        // 아직 6.6초가 되지 않았다면
        // 이동 로직 실행하지 않음
        if (!startHand)
        {
            return;
        }


        // 손 이동 처리
        MoveHand();
    }


    void MoveHand()
    {
        // =========================
        // 현재 이동 중
        // =========================
        if (isMoving)
        {
            // 현재 이동 진행률 계산
            // 0 -> 1까지 0.6초 동안 증가
            float t =
                (currTime - moveStartTime) / duration;


            // 시작 위치에서 목표 위치까지 이동
            hand.transform.position =
                Vector3.Lerp(moveFrom, moveTo, t);


            // =========================
            // 이동 완료
            // =========================
            if (t >= 1f)
            {
                // 정확한 목표 위치로 고정
                hand.transform.position = moveTo;

                isMoving = false;


                // 이동 완료 후
                // 1박 = 0.6초 대기
                nextMoveTime =
                    currTime + 0.6f;

                Debug.Log(
                    "[MoveHand] 이동 완료 : " +
                    currTime +
                    " / 다음 이동 : " +
                    nextMoveTime
                );
            }

            return;
        }


        // =========================
        // 다음 이동 시간이 될 때까지 대기
        // =========================
        if (currTime < nextMoveTime)
        {
            return;
        }


        // =========================
        // 마지막 위치에 도착했으면 종료
        // =========================
        if (hand.transform.position.x >= stopX)
        {
            return;
        }


        // =========================
        // 새로운 이동 시작
        // =========================

        // 현재 위치 저장
        moveFrom = hand.transform.position;


        // X축으로 +2 이동
        moveTo =
            moveFrom + new Vector3(stepX, 0f, 0f);


        // 이동 시작 시간 저장
        moveStartTime = currTime;

        isMoving = true;


        Debug.Log(
            "[MoveHand] 이동 시작 : " +
            currTime +
            " / " +
            moveFrom.x +
            " -> " +
            moveTo.x
        );
    }
}