using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTargetObjTest : MonoBehaviour
{

    private Rigidbody2D rb;
    private Vector2 target;
    private bool move;


    // 적 레이어 지정해두면 더 안전함(Inspector에서 Enemy 레이어로 설정)

    void Awake()
    {
       
        rb = GetComponent<Rigidbody2D>();
        target = rb.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 w = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target = new Vector2(w.x, w.y);
            move = true;

        }

       
    }

    void FixedUpdate()
    {
        if (!move) return;
        rb.MovePosition(target);
        move = false;
    }

  
}
