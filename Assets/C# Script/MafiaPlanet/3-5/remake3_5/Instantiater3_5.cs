using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instantiater3_5 : MonoBehaviour
{
    [Header("생성할 프리팹")]
    public GameObject enemy1;
    public GameObject enemy2;
    public GameObject closedDoor;
    public GameObject openedDoor;

    [Header("각 줄별 스폰 위치")]
    public Transform[] line0Points;
    public Transform[] line1Points;
    public Transform[] line2Points;
    public Transform[] line3Points;

    [Header("총 생성 개수 범위")]
    public int minSpawnCount = 6;
    public int maxSpawnCount = 12;

    [Header("생성된 오브젝트 부모")]
    public Transform parent;

    void Start()
    {
        SpawnPrefabs();
    }

    void SpawnPrefabs()
    {
        // 줄들을 리스트로 묶어서 관리
        List<Transform[]> lines = new List<Transform[]>
        {
            line0Points,
            line1Points,
            line2Points,
            line3Points
        };

        // 줄 개수보다 최소 생성 개수가 작으면 안 됨
        if (minSpawnCount < lines.Count)
        {
            Debug.LogError("minSpawnCount는 줄 개수보다 작으면 안 됨");
            return;
        }

        // 총 생성 개수 결정
        int totalSpawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);

        // 이미 사용한 위치 저장
        List<Transform> selectedPoints = new List<Transform>();

        // 1. 각 줄마다 최소 1개씩 먼저 뽑기
        for (int i = 0; i < lines.Count; i++)
        {
            Transform[] currentLine = lines[i];

            if (currentLine == null || currentLine.Length == 0)
            {
                Debug.LogError(i + "번째 줄에 스폰 위치가 없음");
                return;
            }

            Transform picked = GetRandomPoint(currentLine, selectedPoints);

            if (picked == null)
            {
                Debug.LogError(i + "번째 줄에서 뽑을 수 있는 남은 위치가 없음");
                return;
            }

            selectedPoints.Add(picked);
        }

        // 2. 남은 개수만큼 전체 위치 중에서 추가로 뽑기
        int remainCount = totalSpawnCount - selectedPoints.Count;

        // 전체 위치 리스트 만들기
        List<Transform> allPoints = new List<Transform>();
        foreach (Transform[] line in lines)
        {
            foreach (Transform point in line)
            {
                if (point != null && !allPoints.Contains(point))
                {
                    allPoints.Add(point);
                }
            }
        }

        for (int i = 0; i < remainCount; i++)
        {
            Transform picked = GetRandomPoint(allPoints.ToArray(), selectedPoints);

            if (picked == null)
            {
                Debug.LogWarning("더 이상 뽑을 수 있는 위치가 없어서 중간 종료");
                break;
            }

            selectedPoints.Add(picked);
        }

        // 3. 실제 생성
        for (int i = 0; i < selectedPoints.Count; i++)
        {
            Transform spawnPoint = selectedPoints[i];
            Instantiate(enemy1, spawnPoint.position, Quaternion.identity, parent);
            Instantiate(openedDoor, spawnPoint.position, Quaternion.identity, parent);
        }
    }

    // 아직 선택되지 않은 위치 중 하나를 랜덤으로 반환
    Transform GetRandomPoint(Transform[] candidates, List<Transform> selectedPoints)
    {
        List<Transform> available = new List<Transform>();

        for (int i = 0; i < candidates.Length; i++)
        {
            if (candidates[i] != null && !selectedPoints.Contains(candidates[i]))
            {
                available.Add(candidates[i]);
            }
        }

        if (available.Count == 0)
            return null;

        int randIndex = Random.Range(0, available.Count);
        return available[randIndex];
    }
}
