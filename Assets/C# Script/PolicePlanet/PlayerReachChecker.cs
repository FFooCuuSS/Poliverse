using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReachChecker : MonoBehaviour
{
    [SerializeField] private BoxCollider2D goalCollider;
    private CircleCollider2D playerCollider;
    private DragAndDrop dragAndDrop;

    private bool isGameOver = false;

    private void Start()
    {
        playerCollider = GetComponent<CircleCollider2D>();
        dragAndDrop = GetComponent<DragAndDrop>();
    }

    private void Update()
    {
        if (isGameOver) return;

        BoundCheck();
        GoalCheck();
    }
    private void BoundCheck()
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);

        bool isOnPath = false;

        foreach(var hit in hits)
        {
            if (hit.CompareTag("Path"))
            {
                isOnPath = true;
                break;
            }
        }
        if(!isOnPath)
        {
            isGameOver = true;
            Debug.Log("게임오버");
        }
    }

    private void GoalCheck()
    {
        Bounds goalBounds = goalCollider.bounds;
        Bounds playerBounds = playerCollider.bounds;

        if(playerBounds.Intersects(goalBounds))
        {
            isGameOver = true;
            Debug.Log("게임 성공");
            //이후 미니게임 창 꺼짐 로직 추가 예정
        }
    }
}
