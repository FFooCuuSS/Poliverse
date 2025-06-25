using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProhibitedItemSpawner1_7 : MonoBehaviour
{
    [Header("랜덤 위치 대상 오브젝트들")]
    public GameObject[] spawnPoints;

    [Header("생성할 프리팹")]
    public GameObject prefabToSpawn;

    [Header("랜덤하게 적용할 스프라이트들")]
    public Sprite[] randomSprites;

    [Header("금지 아이템 매니저")]
    public ProhibitedItemManager1_7 manager;

    // 생성된 오브젝트들을 관리할 부모
    public Transform spawnParent;

    void Start()
    {
        SpawnRandomPrefabs();
    }

    void SpawnRandomPrefabs()
    {
        if (randomSprites.Length == 0 || spawnPoints.Length == 0 || prefabToSpawn == null)
        {
            Debug.LogWarning("스프라이트, 프리팹 또는 스폰 포인트가 설정되지 않았습니다.");
            return;
        }

        int spawnCount = 4;//Random.Range(2, spawnPoints.Length + 1);
        GameObject[] shuffled = (GameObject[])spawnPoints.Clone();
        System.Array.Sort(shuffled, (a, b) => Random.Range(-1, 2)); // 간단한 셔플

        int[] spriteCounts = new int[randomSprites.Length];

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = shuffled[i].transform.position;
            spawnPos.z = -2f;

            GameObject newObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, spawnParent);
            SpriteRenderer sr = newObj.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                int spriteIndex = Random.Range(0, randomSprites.Length);
                sr.sprite = randomSprites[spriteIndex];
                spriteCounts[spriteIndex]++;
            }

            newObj.tag = "Item";
        }

        if (manager != null)
        {
            manager.SetPrefabCounts(spriteCounts);
        }
    }
}
