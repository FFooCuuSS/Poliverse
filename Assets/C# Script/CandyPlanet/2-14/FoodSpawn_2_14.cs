using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawn_2_14 : MonoBehaviour
{
    public GameObject foodPrefab;
    public int foodCount = 5;

    public Sprite[] foodSprites;


    public float spawnRangeX = 9f;
    public float spawnRangeY = 4f;

    public float avoidRadius = 2f;

    public Transform player; // 중심
    public float shieldRadius = 2f;


    public void SpawnOneFood(float timeToHit)
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

        GameObject food = Instantiate(foodPrefab, spawnPos, Quaternion.identity);

        SpriteRenderer sr = food.GetComponent<SpriteRenderer>();

        if (sr != null && foodSprites.Length > 0)
        {
            int rand = Random.Range(0, foodSprites.Length);
            sr.sprite = foodSprites[rand];
        }

        // 목표 위치 고정 (0,0,0) 으로 이동시키도록 Init 호출
        food.GetComponent<FoodMove_2_14>()?.Init(timeToHit);
    }
}
