using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemySpawner2_6 : MonoBehaviour
{
    [Header("Obstacle Sprites")]
    [SerializeField] private float scaleMultiplier = 0.5f;
    [SerializeField] private Sprite[] obstacleSprites;

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

        List<int> lanes = new List<int> { 0, 1, 2 };

        List<int> spriteIndexes = new List<int>();
        for (int i = 0; i < obstacleSprites.Length; i++)
            spriteIndexes.Add(i);

        for (int i = 0; i < 2; i++)
        {
            int laneRandom = Random.Range(0, lanes.Count);
            int lane = lanes[laneRandom];
            lanes.RemoveAt(laneRandom);

            int spriteRandom = Random.Range(0, spriteIndexes.Count);
            int spriteIndex = spriteIndexes[spriteRandom];
            spriteIndexes.RemoveAt(spriteRandom);

            float startX = leftX + lane * width;
            float spawnX = startX + width * 0.5f;

            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity, transform);

            Enemy2_6 enemy = obj.GetComponent<Enemy2_6>();
            enemy.Init(minigame);

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

            sr.sprite = obstacleSprites[spriteIndex];

            float spriteWidth = sr.sprite.bounds.size.x;
            float scale = (obstacleWidth / spriteWidth) * scaleMultiplier;

            obj.transform.localScale = Vector3.one * scale;
        }
    }
}
