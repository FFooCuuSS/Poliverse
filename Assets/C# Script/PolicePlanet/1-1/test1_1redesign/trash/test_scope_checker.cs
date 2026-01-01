using System.Collections;
using UnityEngine;

public class test_scope_checker : MonoBehaviour
{
    public test1_1game_manager manager;

    private Rigidbody2D rb;
    private Vector2 target;
    private bool move;

    private bool again;
    public int cnt;

    // 적 레이어 지정해두면 더 안전함(Inspector에서 Enemy 레이어로 설정)
    public LayerMask enemyLayer;

    void Awake()
    {
        again = false;
        cnt = 0;
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

            // 클릭한 위치에서 바로 판정
            CheckHitAtPoint(target);
        }

        if (again)
        {
            target = Vector2.zero;
            move = true;
            StartCoroutine(restarter());
            again = false;
        }

        if (cnt == 8)
        {
            manager.SetGameClear();
        }
    }

    void FixedUpdate()
    {
        if (!move) return;
        rb.MovePosition(target);
        move = false;
    }

    void CheckHitAtPoint(Vector2 point)
    {
        // 그 점에 있는 적 콜라이더를 찾음 (적이 Point에 완전히 안 걸리면 OverlapCircle로 바꿔도 됨)
        Collider2D hit = Physics2D.OverlapPoint(point, enemyLayer);
        if (hit == null) return;

        manager.CheckHit(hit);
        cnt++;

        if (cnt == 4)
        {
            DestroyAllEnemies();
            again = true;
        }
    }

    IEnumerator restarter()
    {
        yield return new WaitForSeconds(0.5f);
        manager.RestartRound();
    }

    void DestroyAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            Destroy(enemies[i]);
        }
    }
}
