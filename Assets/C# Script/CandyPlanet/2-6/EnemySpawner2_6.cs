using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner2_6 : MonoBehaviour
{
    public GameObject prefabToSpawn;       // 생성할 프리팹 (UI용)
    public Transform spawnParent;          // 캔버스 아래의 빈 오브젝트 (ex: "UIContainer")
    public Vector2 spawnPosition;          // anchoredPosition (UI 위치)

    public float minSpawnTime = 1f;  // 최소 시간
    public float maxSpawnTime = 5f;  // 최대 시간

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    void Spawn()
    {
        if (prefabToSpawn != null && spawnParent != null)
        {
            // 프리팹을 spawnParent 하위에 생성
            GameObject spawned = Instantiate(prefabToSpawn, spawnParent);

            // anchoredPosition 조정 (UI 전용 좌표계)
            RectTransform rect = spawned.GetComponent<RectTransform>();
            rect.anchoredPosition = spawnPosition;
        }
        else
        {
            Debug.LogWarning("프리팹 또는 부모 Transform이 설정되지 않았습니다.");
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
