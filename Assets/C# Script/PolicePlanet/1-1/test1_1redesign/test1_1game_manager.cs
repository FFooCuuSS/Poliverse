using System.Collections;
using UnityEngine;

public class test1_1game_manager : MonoBehaviour
{
    public GameObject enemy;
    public float spawnInterval = 0.5f;

    private int expected;
    private Vector2[] pos =
    {
        new Vector2( 4.5f,  2f),
        new Vector2( 4.5f, -2f),
        new Vector2(-4.5f,  2f),
        new Vector2(-4.5f, -2f)
    };

    void Start()
    {
        Shuffle(pos);
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
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

            if (expected >= pos.Length) Debug.Log("All Clear!");
        }
        else
        {
            Debug.Log("Fail: got " + tag.orderIndex + " expected " + expected);
        }
    }

    void Shuffle(Vector2[] a)
    {
        for (int i = 0; i < a.Length; i++)
        {
            int r = Random.Range(i, a.Length);
            (a[i], a[r]) = (a[r], a[i]);
        }
    }
}
