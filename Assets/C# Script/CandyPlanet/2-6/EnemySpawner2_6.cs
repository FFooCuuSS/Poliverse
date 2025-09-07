using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner2_6 : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    public GameObject prefab; // ������ ������

    [Header("Spawn Range (Local X)")]
    public float leftOffset = -5f;   // ���� ��
    public float rightOffset = 5f;   // ������ ��

    [Header("Spawn Interval")]
    public float minInterval = 1f;   // �ּ� ���� �ֱ�
    public float maxInterval = 2f;   // �ִ� ���� �ֱ�

    void Start()
    {
        // ���� ���� �� ���� ���� ����
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // ���� ��� (1��~2��)
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            // ���� ��ġ (Spawner ���� �¿� ����)
            float randomX = Random.Range(leftOffset, rightOffset);
            Vector3 spawnPos = transform.position + new Vector3(randomX, 0f, -2f);

            // ������ ����
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }
}
