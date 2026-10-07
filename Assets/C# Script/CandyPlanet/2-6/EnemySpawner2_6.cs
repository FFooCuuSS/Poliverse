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
    [SerializeField] private GameObject prefab;

    [Header("Spawn Range (Local X)")]
    [SerializeField] private float leftX = -300f;
    [SerializeField] private float rightX = 300f;
    [SerializeField] private float spawnY = 0f;

    [SerializeField] private MiniGame2_6 minigame;

    private int safeLane = 1;

    public int SpawnObstacle()
    {
        if (prefab == null)
        {
            Debug.LogError(
                "[EnemySpawner2_6] prefab이 연결되지 않았습니다."
            );

            return -1;
        }

        if (obstacleSprites == null ||
            obstacleSprites.Length == 0)
        {
            Debug.LogError(
                "[EnemySpawner2_6] obstacleSprites가 없습니다."
            );

            return -1;
        }

        float width =
            (rightX - leftX) / 3f;

        float obstacleWidth =
            width * (2f / 3f);

        List<int> lanes =
            new List<int>
            {
                0,
                1,
                2
            };

        List<int> spriteIndexes =
            new List<int>();

        for (int i = 0;
             i < obstacleSprites.Length;
             i++)
        {
            spriteIndexes.Add(i);
        }


        for (int i = 0; i < 2; i++)
        {
            int laneRandom =
                Random.Range(0, lanes.Count);

            int lane =
                lanes[laneRandom];

            lanes.RemoveAt(laneRandom);

            int spriteIndex;

            if (spriteIndexes.Count > 0)
            {
                int spriteRandom =
                    Random.Range(
                        0,
                        spriteIndexes.Count
                    );

                spriteIndex =
                    spriteIndexes[spriteRandom];

                spriteIndexes.RemoveAt(spriteRandom);
            }
            else
            {
                spriteIndex = 0;
            }

            float startX =
                leftX + lane * width;

            float spawnX =
                startX + width * 0.5f;

            Vector3 spawnPos =
                new Vector3(
                    spawnX,
                    spawnY,
                    0f
                );

            GameObject obj =
                Instantiate(
                    prefab,
                    spawnPos,
                    Quaternion.identity,
                    transform
                );

            Enemy2_6 enemy =
                obj.GetComponent<Enemy2_6>();

            if (enemy != null)
            {
                enemy.Init(minigame);
            }

            SpriteRenderer sr =
                obj.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.sprite =
                    obstacleSprites[spriteIndex];

                float spriteWidth =
                    sr.sprite.bounds.size.x;

                if (spriteWidth > 0f)
                {
                    float scale =
                        (obstacleWidth / spriteWidth)
                        * scaleMultiplier;

                    obj.transform.localScale =
                        Vector3.one * scale;
                }
            }
        }

        safeLane = lanes[0];

        Debug.Log(
            $"[EnemySpawner2_6] 안전 레인 = {safeLane}"
        );

        return safeLane;
    }

    public int GetSafeLane()
    {
        return safeLane;
    }
}
