using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemySpawner2_6 : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    public GameObject prefab;

    [Header("Spawn Range (Local X)")]
    public float leftX = -300f;
    public float rightX = 300f;

    public float spawnY = 0f;

    public MiniGame2_6 minigame;

    public void SpawnObstacle()
    {
        float width = (rightX - leftX) / 3f;
        float obstacleWidth = width * (2f / 3f);

        int emptyLane = Random.Range(0, 3);

        for (int lane = 0; lane < 3; lane++)
        {
            if (lane == emptyLane) continue;

            float startX = leftX + lane * width;
            float spawnX = startX + (width / 2f);

            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity, transform);

            Enemy2_6 enemy = obj.GetComponent<Enemy2_6>();
            enemy.Init(minigame);

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

            float spriteWidth = sr.bounds.size.x;
            float extraWidth = 1.2f;
            float scale = (obstacleWidth / spriteWidth) * extraWidth;

            obj.transform.localScale = new Vector3(scale, 0.7f, 1f);
        }
    }
}
