using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawn_2_14 : MonoBehaviour
{
    public GameObject foodPrefab;
    public int foodCount = 5;

    public float spawnRangeX = 7f;
    public float spawnRangeY = 4f;

    public float avoidRadius = 2f;

    void Start()
    {
        SpawnFoods();
    }

    private void SpawnFoods()
    {
        for (int i = 0; i < foodCount; i++)
        {
            Vector2 spawnPos;

            do
            {
                spawnPos = new Vector2(
                    Random.Range(-spawnRangeX, spawnRangeX),
                    Random.Range(-spawnRangeY, spawnRangeY)
                );
            }
            while (spawnPos.magnitude < avoidRadius);

            Instantiate(foodPrefab, spawnPos, Quaternion.identity);
        }
    }
}
