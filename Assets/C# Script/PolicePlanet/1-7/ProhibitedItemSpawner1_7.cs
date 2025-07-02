using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProhibitedItemSpawner1_7 : MonoBehaviour
{
    [Header("랜덤 위치 대상 오브젝트들")]
    public GameObject[] spawnPoints;

    [Header("생성할 프리팹들 (각기 다른 금지 물품 프리팹)")]
    public List<GameObject> prefabVariants;

    [Header("금지 아이템 매니저")]
    public ProhibitedItemManager1_7 manager;

    public Transform spawnParent;

    void Start()
    {
        SpawnRandomPrefabs();
    }

    void SpawnRandomPrefabs()
    {
        if (prefabVariants.Count == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("프리팹 목록 또는 스폰 위치가 비어 있습니다.");
            return;
        }

        int spawnCount =  Random.Range(2, spawnPoints.Length + 1);
        GameObject[] shuffled = (GameObject[])spawnPoints.Clone();
        System.Array.Sort(shuffled, (a, b) => Random.Range(-1, 2)); // 셔플

        int[] spriteCounts = new int[manager.prohibitedSprites.Count];

        for (int i = 0; i < spawnCount; i++)
        {
            int prefabIndex = Random.Range(0, prefabVariants.Count);
            GameObject selectedPrefab = prefabVariants[prefabIndex];

            Vector3 spawnPos = shuffled[i].transform.position;
            spawnPos.z += -3f;

            GameObject newObj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity, spawnParent);
            newObj.tag = "Item";

            // 생성된 프리팹의 스프라이트로 인덱스 매핑
            SpriteRenderer sr = newObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                int index = manager.prohibitedSprites.IndexOf(sr.sprite);
                if (index != -1)
                {
                    spriteCounts[index]++;
                }
                else
                {
                    Debug.LogWarning($"생성된 프리팹의 스프라이트 {sr.sprite.name}가 manager.prohibitedSprites에 없습니다!");
                }
            }
        }

        if (manager != null)
        {
            manager.SetPrefabCounts(spriteCounts);
        }
    }
}
