using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CakeReachPlayer : MonoBehaviour
{
    public CircleCollider2D playerCollider;
    public BoxCollider2D cakeCollider;

    private Minigame_2_1 minigame_2_1;
    public GameObject stage_2_1;
    public GameObject player;

    private bool isGameOver = false;

    private void Start()
    {
        minigame_2_1 = stage_2_1.GetComponent<Minigame_2_1>();
    }
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
            
        }
    }

}
