using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveHandRemake : MonoBehaviour
{
    public GameObject hand;

    Vector3 startPos = new Vector3(-6.0f, 4.5f, 0f);
    Vector3 startSet = new Vector3(-6.0f, 2.0f, 0f);

    float duration = 0.5f; // 이동에 걸리는 시간(0.5초)

    bool setHand = false;
    bool startHand = false;

    // ===== 이동 스케줄/상태 =====
    private bool isMoving = false;       // 지금 이동 중인지
    private float moveStartTime = 0f;    // 현재 이동 시작 시각
    private Vector3 moveFrom;            // 이동 시작 위치
    private Vector3 moveTo;              // 이동 목표 위치

    private float nextMoveTime = 5.5f;   // 다음 이동이 시작될 시각(처음은 5.5초)
    private float stepX = 2f;            // 한 번에 x로 +2 이동
    private float stopX = 6f;            // x가 6 이상이면 종료

    void Update()
    {
        // 5초에 손 위치 세팅(위쪽)
        if (!setHand && Time.time >= 5f)
        {
            hand.transform.position = startPos;
            setHand = true;
        }

        // 5.5초에 시작 위치 세팅(아래쪽) + 이동 루프 시작 준비
        if (!startHand && Time.time >= 5.5f)
        {
            hand.transform.position = startSet;
            startHand = true;

            // 첫 이동은 5.5초에 바로 시작되도록 이미 nextMoveTime을 5.5로 잡아둠
        }

        // 시작 전이면 아무 것도 안 함
        if (!startHand) return;

        MoveHand();
    }

    void MoveHand()
    {
        float now = Time.time;

        // 1) 이동 중이면: Lerp로 진행
        if (isMoving)
        {
            float t = (now - moveStartTime) / duration; // 0~1 진행률
            hand.transform.position = Vector3.Lerp(moveFrom, moveTo, t);

            // 이동 완료
            if (t >= 1f)
            {
                hand.transform.position = moveTo;
                isMoving = false;

                // 다음 이동은 "도착한 시각 기준 +0.5초 대기 후" = 지금부터 0.5초 뒤
                // (즉 이동 0.5초 + 대기 0.5초 = 총 1초마다 한 칸)
                nextMoveTime = now + 0.5f;
            }

            return; // 이동 중엔 여기서 끝
        }

        // 2) 이동 중이 아니면: 다음 이동 시작 시각이 되었는지 체크
        if (now < nextMoveTime) return;

        // 종료 조건: x가 6 이상이면 더 이상 이동 안 함
        if (hand.transform.position.x >= stopX)
        {
            return;
        }

        // 3) 새 이동 시작
        moveFrom = hand.transform.position;
        moveTo = moveFrom + new Vector3(stepX, 0f, 0f); // 현재 위치에서 x +2

        // 혹시 마지막에 6을 넘기면 딱 6에 맞추고 싶으면 아래처럼 클램프 가능
        //if (moveTo.x > stopX) moveTo.x = stopX;

        moveStartTime = now;
        isMoving = true;
    }
}