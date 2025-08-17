using System.Collections.Generic;
using UnityEngine;

public class SpawnPolice1_5 : MonoBehaviour
{
    public GameObject enemyPrefab;   // enemy1_5 프리팹
    public GameObject policePrefab;  // police1_5 프리팹
    public Transform parentTransform; // 소환될 부모 트랜스폼

    public int totalSpawnCount = 15;   // 총 소환 수 (항상 15)
    public float yValue = -1.779531f;  // y 고정 (0 + -1.779531)
    public float zValue = 0f;          // z 고정
    public float xStart = -5.573807f;  // 시작 x 좌표 (-4.5 + -1.073807)
    public float xEnd = 10.5f;         // 끝 x 좌표 (변경 없음)
    public float xStep = 1.0f;         // x 간격

    private List<Vector3> candidatePositions; // 가능한 소환 위치들

    private void Awake()
    {
        // x 좌표 후보 위치 생성
        candidatePositions = new List<Vector3>();
        float xPosition = xStart;
        while (xPosition <= xEnd + 0.0001f)
        {
            candidatePositions.Add(new Vector3(xPosition, yValue, zValue));
            xPosition += xStep;
        }
    }

    private void Start()
    {
        // 테스트용: 시작 시 1단계 소환
        SpawnByStage(1);
    }

    public void SpawnByStage(int stage)
    {
        if (enemyPrefab == null || policePrefab == null)
        {
            Debug.LogError("enemyPrefab 또는 policePrefab이 비어있습니다.");
            return;
        }

        // 단계별 enemy 개수
        int enemyCount = 0;
        if (stage == 1) enemyCount = 5;
        else if (stage == 2) enemyCount = 6;
        else if (stage == 3) enemyCount = 7;
        else
        {
            Debug.LogWarning("잘못된 stage 값입니다: " + stage);
            return;
        }

        // 총 개수에서 police 개수 계산
        if (enemyCount > totalSpawnCount) enemyCount = totalSpawnCount;
        int policeCount = totalSpawnCount - enemyCount;

        // 후보 좌표 수 체크
        if (candidatePositions.Count < totalSpawnCount)
        {
            Debug.LogWarning("후보 좌표가 부족합니다.");
            int maxSpawn = candidatePositions.Count;
            if (enemyCount > maxSpawn) enemyCount = maxSpawn;
            policeCount = maxSpawn - enemyCount;
        }

        // 위치 인덱스 리스트 만들고 셔플
        List<int> indexes = new List<int>();
        for (int i = 0; i < candidatePositions.Count; i++)
        {
            indexes.Add(i);
        }

        for (int i = indexes.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            int tmp = indexes[i];
            indexes[i] = indexes[r];
            indexes[r] = tmp;
        }

        int spawned = 0;

        // enemy 배치
        for (int i = 0; i < enemyCount; i++)
        {
            int pick = indexes[spawned];
            Vector3 pos = candidatePositions[pick];
            Instantiate(enemyPrefab, pos, Quaternion.identity, parentTransform);
            spawned += 1;
        }

        // police 배치
        for (int i = 0; i < policeCount; i++)
        {
            int pick = indexes[spawned];
            Vector3 pos = candidatePositions[pick];
            Instantiate(policePrefab, pos, Quaternion.identity, parentTransform);
            spawned += 1;
        }
    }
}
