using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingIconController2_3 : MonoBehaviour
{
    public Transform barTransform;   // 바 전체 영역의 Transform
    public float barWidth = 5f;       // 바의 가로 폭 (월드 단위)
    public float speed = 2f;          // 움직임 속도 (유닛/초)

    private bool movingRight = true;

    public Minigame_2_3 minigame;

    public bool IsInCorrectZone { get; private set; }

    private Transform tf;

    void Start()
    {
        tf = transform;
    }

    void Update()
    {
        float delta = speed * Time.deltaTime;

        if (movingRight)
        {
            tf.position += Vector3.right * delta;
            if (tf.position.x >= barTransform.position.x + barWidth / 2)
                movingRight = false;
        }
        else
        {
            tf.position -= Vector3.right * delta;
            if (tf.position.x <= barTransform.position.x - barWidth / 2)
                movingRight = true;
        }

        // Input 타이밍에 중앙 위치 통과 여부 체크만 한다
        if (minigame != null && minigame.IsInputTiming)
        {
            // 중앙과의 거리 체크
            float distanceToCenter = Mathf.Abs(tf.position.x - barTransform.position.x);

            // correctZone 범위 대신 거리 기준으로 판단 가능 (예: 0.1f 이내)
            if (distanceToCenter < 0.1f)
                IsInCorrectZone = true;
            else
                IsInCorrectZone = false;
        }
        else
        {
            IsInCorrectZone = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("CorrectZone"))
            IsInCorrectZone = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("CorrectZone"))
            IsInCorrectZone = false;
    }
}
