using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CakeReachPlayer : MonoBehaviour
{
    public CircleCollider2D playerCollider;
    public BoxCollider2D cakeCollider;

    private bool isGameOver = false;

    private void Update()
    {
        BoundCheck();
    }
    private void BoundCheck()
    {
        if (isGameOver == true) return;

        Bounds playerBounds = playerCollider.bounds;
        Bounds cakeBounds = cakeCollider.bounds;

        if(playerBounds.Intersects(cakeBounds))
        {
            isGameOver = true;
            Debug.Log("게임오버");
        }
    }
}
