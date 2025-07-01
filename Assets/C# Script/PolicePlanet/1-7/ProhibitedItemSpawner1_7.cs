using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProhibitedItemSpawner1_7 : MonoBehaviour
{
    [Header("���� ��ġ ��� ������Ʈ��")]
    public GameObject[] spawnPoints;

    [Header("������ �����յ� (���� �ٸ� ���� ��ǰ ������)")]
    public List<GameObject> prefabVariants;

    [Header("���� ������ �Ŵ���")]
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
            Debug.LogWarning("������ ��� �Ǵ� ���� ��ġ�� ��� �ֽ��ϴ�.");
            return;
        }

        int spawnCount =  Random.Range(2, spawnPoints.Length + 1);
        GameObject[] shuffled = (GameObject[])spawnPoints.Clone();
        System.Array.Sort(shuffled, (a, b) => Random.Range(-1, 2)); // ����

        int[] spriteCounts = new int[manager.prohibitedSprites.Count];

        for (int i = 0; i < spawnCount; i++)
        {
            int prefabIndex = Random.Range(0, prefabVariants.Count);
            GameObject selectedPrefab = prefabVariants[prefabIndex];

            Vector3 spawnPos = shuffled[i].transform.position;
            spawnPos.z += -3f;

            GameObject newObj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity, spawnParent);
            newObj.tag = "Item";

            // ������ �������� ��������Ʈ�� �ε��� ����
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
                    Debug.LogWarning($"������ �������� ��������Ʈ {sr.sprite.name}�� manager.prohibitedSprites�� �����ϴ�!");
                }
            }
        }

        if (manager != null)
        {
            manager.SetPrefabCounts(spriteCounts);
        }
    }
}
