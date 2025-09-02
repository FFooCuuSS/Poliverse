using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class instantiate_3_10_box : MonoBehaviour
{
    private GameObject boxPrefab;
    private float[] spawnYPositions = new float[] { 3.2f, 0f, -3.2f };
    public float spawnX = 10f;
    public float spawnZ = 0f;

    private Coroutine spawnRoutine;

    void Start()
    {
        boxPrefab = Resources.Load<GameObject>("MinigamePrefab/MafiaPlanet/TempPrefab/3_10box");
        if (boxPrefab == null)
        {
            Debug.LogError("Prefab not found: MinigamePrefab/MafiaPlanet/TempPrefab/3_10box");
            return;
        }

        spawnRoutine = StartCoroutine(SpawnBoxesEverySecond());
    }

    IEnumerator SpawnBoxesEverySecond()
    {
        while (true)
        {
            SpawnBoxAtRandomY();
            yield return new WaitForSeconds(1f);
        }
    }

    void SpawnBoxAtRandomY()
    {
        int randIndex = Random.Range(0, spawnYPositions.Length);
        float y = spawnYPositions[randIndex];
        Vector3 spawnPos = new Vector3(spawnX, y, spawnZ);

        GameObject enemy = Instantiate(boxPrefab, spawnPos, Quaternion.identity);
        enemy.tag = "Enemy"; // 혹시 prefab에 태그 안 달려 있으면 보정
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void StartSpawning() // 필요 시 재시작용
    {
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnBoxesEverySecond());
    }
}
