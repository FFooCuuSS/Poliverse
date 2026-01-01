using System.Collections;
using UnityEngine;

public class test1_1game_manager : MonoBehaviour
{
    public GameObject enemy;
    public float spawnInterval = 0.5f;
    public bool GameClear;

    private int expected;
    public Vector2[] pos =
    {
        new Vector2( 4.5f,  2f),
        new Vector2( 4.5f, -2f),
        new Vector2(-4.5f,  2f),
        new Vector2(-4.5f, -2f)
    };

    void Start()
    {
        GameClear = false;
        Shuffle(pos);
        StartCoroutine(Spawn());
    }

    public IEnumerator Spawn()
    {
        expected = 0;

        for (int i = 0; i < pos.Length; i++)
        {
            var go = Instantiate(enemy, pos[i], Quaternion.identity);

            var tag = go.GetComponent<test_enemy_order_tag>();
            if (tag == null) tag = go.AddComponent<test_enemy_order_tag>();

            tag.orderIndex = i;
            tag.cleared = false;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void CheckHit(Collider2D other)
    {
        var tag = other.GetComponentInParent<test_enemy_order_tag>();
        if (tag == null || tag.cleared) return;

        if (tag.orderIndex == expected)
        {
            tag.cleared = true;
            expected++;

            Debug.Log("Success: " + tag.orderIndex);

            Destroy(tag.transform.root.gameObject);

            //if (expected >= pos.Length) Debug.Log("All Clear!");
            if(GameClear)
            {
               // Time.timeScale = 0f;
            }
        }
        else
        {
            Debug.Log("Fail: got " + tag.orderIndex + " expected " + expected);
            expected++;
        }
    }

    public void Shuffle(Vector2[] a)
    {
        for (int i = 0; i < a.Length; i++)
        {
            int r = Random.Range(i, a.Length);
            (a[i], a[r]) = (a[r], a[i]);
        }
    }
    public void RestartRound()
    {
        StopAllCoroutines();
        Shuffle(pos);
        StartCoroutine(Spawn());
    }
    public void SetGameClear()
    {
        StartCoroutine(GameClearRoutine());
    }

    IEnumerator GameClearRoutine()
    {
        GameClear = true;

        // 다음 프레임까지 기다리기 (Destroy 처리되게)
        yield return null;

        Time.timeScale = 0f;
    }

}
