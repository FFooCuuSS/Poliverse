using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner_3_5 : MonoBehaviour
{
    public GameObject enemyPrefab; 
    public float zPosition = 0f;   
    public Transform parentTransform; 

    public bool noReuseAcrossStages = false; // 여러 단계 연속 소환 시 같은 위치 재사용 방지 여부

    private readonly float[] yPositions = { 2.5f+ 0.68f, 0.1f + 0.68f, -2.4f + 0.68f, -4.8f + 0.68f };
    private readonly float[] xPositions = { -9.5f+ 2.14f, -6.6f + 2.14f, -3.5f + 2.14f, 2.2f + 2.14f, -0.5f + 2.14f, 5.4f + 2.14f };

    private HashSet<int> globallyUsedIndexes = new HashSet<int>();
    private List<Vector3> spawnPositions;
    public int spawnCount;

    private void Start()
    {
        SpawnByStage(1);
    }
    void Awake()
    {
        spawnPositions = new List<Vector3>(yPositions.Length * xPositions.Length);
        for (int yIndex = 0; yIndex < yPositions.Length; yIndex++)
        {
            for (int xIndex = 0; xIndex < xPositions.Length; xIndex++)
            {
                spawnPositions.Add(new Vector3(xPositions[xIndex], yPositions[yIndex], zPosition));
            }
        }
    }

    public void SpawnByStage(int stage)
    {
        
        if (stage == 1) spawnCount = 5;
        else if (stage == 2) spawnCount = 6;
        else if (stage == 3) spawnCount = 7;
        else if (stage == 4) spawnCount = 8;
        else
        {
            Debug.LogWarning("잘못된 stage 값입니다: " + stage);
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("enemyPrefab이 비어있습니다. 인스펙터에 3_5enemy 프리팹을 할당하세요.");
            return;
        }

        List<int> availableIndexes = new List<int>(spawnPositions.Count);
        for (int index = 0; index < spawnPositions.Count; index++)
        {
            if (!noReuseAcrossStages || !globallyUsedIndexes.Contains(index))
            {
                availableIndexes.Add(index);
            }
        }

        if (availableIndexes.Count < spawnCount)
        {
            Debug.LogWarning("소환 개수 초과");
            spawnCount = availableIndexes.Count;
        }

        // 셔플
        for (int i = availableIndexes.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = availableIndexes[i];
            availableIndexes[i] = availableIndexes[randomIndex];
            availableIndexes[randomIndex] = temp;
        }

        for (int k = 0; k < spawnCount; k++)
        {
            int spawnIndex = availableIndexes[k];
            Vector3 spawnPosition = spawnPositions[spawnIndex];
            GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, parentTransform);
            //spawnedEnemy.name = enemyPrefab.name + " (" + spawnPosition.x.ToString("F1") + "," + spawnPosition.y.ToString("F1") + ")";

            if (noReuseAcrossStages)
            {
                globallyUsedIndexes.Add(spawnIndex);
            }
        }
    }

    
}