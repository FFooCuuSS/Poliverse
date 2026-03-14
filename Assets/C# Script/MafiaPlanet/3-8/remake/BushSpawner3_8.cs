using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushSpawner3_8 : MonoBehaviour
{
    public GameObject bushPrefab;
    public EnemyWatch3_8 enemyWatch;

    public Vector3 spawnPosition = new Vector3(10f, 0f, 0f);

    public float spawnInterval = 4f;

    private List<BushMover3_8> bushes = new List<BushMover3_8>();

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StopAllBushes();
        }

        Cleanup();
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnBush();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnBush()
    {
        enemyWatch.PlayWatchRoutine();
        GameObject obj = Instantiate(bushPrefab, spawnPosition, Quaternion.identity);

        BushMover3_8 mover = obj.GetComponent<BushMover3_8>();

        if (mover != null)
        {
            bushes.Add(mover);
        }
    }

    void StopAllBushes()
    {
        for (int i = 0; i < bushes.Count; i++)
        {
            if (bushes[i] != null)
            {
                bushes[i].StopForMoment();
            }
        }
    }

    void Cleanup()
    {
        for (int i = bushes.Count - 1; i >= 0; i--)
        {
            if (bushes[i] == null)
            {
                bushes.RemoveAt(i);
            }
        }
    }
}
