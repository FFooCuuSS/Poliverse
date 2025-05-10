using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReachChecker : MonoBehaviour
{
    [SerializeField] private List<BoxCollider2D> ObstacleColliders;
    [SerializeField] private BoxCollider2D goalCollider;
    private CircleCollider2D playerCollider;

    private bool gameEnd = false;

    private void Start()
    {
        playerCollider = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        if (gameEnd) return;

        BoundCheck();
        GoalCheck();
    }
    private void BoundCheck()
    {
        Bounds playerBounds = playerCollider.bounds;

        foreach (var obstacleCol in ObstacleColliders)
        {
            Bounds obstacleBounds = obstacleCol.bounds;

            if(playerBounds.Intersects(obstacleBounds))
            {
                gameEnd = true;
                Debug.Log("게임 실패");
                //이후 미니게임 창 꺼짐 로직 추가 예정
            }
        }
    }

    private void GoalCheck()
    {
        Bounds goalBounds = goalCollider.bounds;
        Bounds playerBounds = playerCollider.bounds;

        if(playerBounds.Intersects(goalBounds))
        {
            gameEnd = true;
            Debug.Log("게임 성공");
            //이후 미니게임 창 꺼짐 로직 추가 예정
        }
    }
}
