using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner2_6 : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    public GameObject prefab; // 생성할 프리팹

    [Header("Spawn Range (Local X)")]
    public float leftOffset = -5f;   // 왼쪽 끝
    public float rightOffset = 5f;   // 오른쪽 끝

    [Header("Spawn Interval")]
    public float minInterval = 1f;   // 최소 생성 주기
    public float maxInterval = 2f;   // 최대 생성 주기

    void Start()
    {
        // 실행 시작 시 스폰 루프 시작
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // 랜덤 대기 (1초~2초)
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            // 랜덤 위치 (Spawner 기준 좌우 범위)
            float randomX = Random.Range(leftOffset, rightOffset);
            Vector3 spawnPos = transform.position + new Vector3(randomX, 0f, -2f);

            // 프리팹 생성
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }
}
