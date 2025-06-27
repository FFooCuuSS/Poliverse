using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProhibitedItemSpawner1_7 : MonoBehaviour
{
    [Header("���� ��ġ ��� ������Ʈ��")]
    public GameObject[] spawnPoints;

    [Header("������ ������")]
    public GameObject prefabToSpawn;

    [Header("�����ϰ� ������ ��������Ʈ��")]
    public Sprite[] randomSprites;

    [Header("���� ������ �Ŵ���")]
    public ProhibitedItemManager1_7 manager;

    // ������ ������Ʈ���� ������ �θ�
    public Transform spawnParent;

    void Start()
    {
        SpawnRandomPrefabs();
    }

    void SpawnRandomPrefabs()
    {
        if (randomSprites.Length == 0 || spawnPoints.Length == 0 || prefabToSpawn == null)
        {
            Debug.LogWarning("��������Ʈ, ������ �Ǵ� ���� ����Ʈ�� �������� �ʾҽ��ϴ�.");
            return;
        }

        int spawnCount = 4;//Random.Range(2, spawnPoints.Length + 1);
        GameObject[] shuffled = (GameObject[])spawnPoints.Clone();
        System.Array.Sort(shuffled, (a, b) => Random.Range(-1, 2)); // ������ ����

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
