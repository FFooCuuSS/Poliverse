using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner2_6 : MonoBehaviour
{
    public GameObject prefabToSpawn;       // ������ ������ (UI��)
    public Transform spawnParent;          // ĵ���� �Ʒ��� �� ������Ʈ (ex: "UIContainer")
    public Vector2 spawnPosition;          // anchoredPosition (UI ��ġ)

    public float minSpawnTime = 1f;  // �ּ� �ð�
    public float maxSpawnTime = 5f;  // �ִ� �ð�

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    void Spawn()
    {
        if (prefabToSpawn != null && spawnParent != null)
        {
            // �������� spawnParent ������ ����
            GameObject spawned = Instantiate(prefabToSpawn, spawnParent);

            // anchoredPosition ���� (UI ���� ��ǥ��)
            RectTransform rect = spawned.GetComponent<RectTransform>();
            rect.anchoredPosition = spawnPosition;
        }
        else
        {
            Debug.LogWarning("������ �Ǵ� �θ� Transform�� �������� �ʾҽ��ϴ�.");
        }
    }
    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            Spawn();
        }
    }
}
