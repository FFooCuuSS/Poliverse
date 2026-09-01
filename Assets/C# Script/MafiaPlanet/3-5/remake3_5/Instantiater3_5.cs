using System.Collections.Generic;
using UnityEngine;

public class Instantiater3_5 : MonoBehaviour
{
    [Header("생성할 프리팹")]
    public GameObject enemy1;
    public GameObject enemy2;

    // 닫힌 창문
    public GameObject closedDoor;

    // 열린 창문
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

    private void Start()
    {
        SpawnPrefabs();
    }

    private void SpawnPrefabs()
    {
        // 각 줄의 스폰 위치 배열을 하나의 리스트로 묶어서 관리한다.
        List<Transform[]> lines = new List<Transform[]>
        {
            line0Points,
            line1Points,
            line2Points,
            line3Points
        };


        // --------------------------------------------------
        // 0. 등록된 모든 위치에 닫힌 창문 생성
        // --------------------------------------------------

        // 각 위치에 생성한 closedDoor를 저장한다.
        // 이후 랜덤으로 선택된 위치의 closedDoor만 삭제하기 위해 사용한다.
        Dictionary<Transform, GameObject> closedDoorObjects =
            new Dictionary<Transform, GameObject>();

        foreach (Transform[] line in lines)
        {
            // 줄 자체가 등록되지 않은 경우 넘어간다.
            if (line == null)
            {
                continue;
            }

            foreach (Transform point in line)
            {
                // 비어있는 Transform은 무시한다.
                if (point == null)
                {
                    continue;
                }

                // 같은 Transform이 중복 등록되어 있을 가능성을 막는다.
                if (closedDoorObjects.ContainsKey(point))
                {
                    continue;
                }

                // 모든 위치에 닫힌 창문을 생성한다.
                GameObject closedWindow = Instantiate(
                    closedDoor,
                    point.position,
                    point.rotation,
                    parent
                );

                // 어떤 위치에 어떤 닫힌 창문이 생성되었는지 저장한다.
                closedDoorObjects.Add(point, closedWindow);
            }
        }


        // --------------------------------------------------
        // 기존 랜덤 선택 로직
        // --------------------------------------------------

        // 줄 개수보다 최소 생성 개수가 작으면 안 된다.
        // 현재 4줄이므로 최소 4개 이상이어야 한다.
        if (minSpawnCount < lines.Count)
        {
            Debug.LogError(
                "minSpawnCount는 줄 개수보다 작으면 안 됨"
            );

            return;
        }

        // 총 생성 개수를 랜덤으로 결정한다.
        // 예: min=6, max=12라면 6~12개
        int totalSpawnCount =
            Random.Range(
                minSpawnCount,
                maxSpawnCount + 1
            );


        // 랜덤으로 선택된 위치들을 저장한다.
        List<Transform> selectedPoints =
            new List<Transform>();


        // --------------------------------------------------
        // 1. 각 줄마다 최소 1개씩 먼저 선택
        // --------------------------------------------------

        for (int i = 0; i < lines.Count; i++)
        {
            Transform[] currentLine = lines[i];

            // 해당 줄에 위치가 하나도 없다면 오류
            if (currentLine == null ||
                currentLine.Length == 0)
            {
                Debug.LogError(
                    i + "번째 줄에 스폰 위치가 없음"
                );

                return;
            }


            // 현재 줄에서 아직 선택되지 않은 위치 하나를 랜덤으로 선택
            Transform picked =
                GetRandomPoint(
                    currentLine,
                    selectedPoints
                );


            if (picked == null)
            {
                Debug.LogError(
                    i +
                    "번째 줄에서 뽑을 수 있는 남은 위치가 없음"
                );

                return;
            }


            // 선택된 위치 저장
            selectedPoints.Add(picked);
        }


        // --------------------------------------------------
        // 2. 남은 개수만큼 전체 위치에서 추가 선택
        // --------------------------------------------------

        int remainCount =
            totalSpawnCount -
            selectedPoints.Count;


        // 모든 줄의 모든 위치를 하나의 리스트로 만든다.
        List<Transform> allPoints =
            new List<Transform>();


        foreach (Transform[] line in lines)
        {
            if (line == null)
            {
                continue;
            }

            foreach (Transform point in line)
            {
                // null 위치는 제외
                // 같은 Transform이 중복 등록된 경우도 제외
                if (point != null &&
                    !allPoints.Contains(point))
                {
                    allPoints.Add(point);
                }
            }
        }


        // 필요한 개수만큼 추가 선택
        for (int i = 0; i < remainCount; i++)
        {
            Transform picked =
                GetRandomPoint(
                    allPoints.ToArray(),
                    selectedPoints
                );


            // 더 이상 선택 가능한 위치가 없는 경우
            if (picked == null)
            {
                Debug.LogWarning(
                    "더 이상 뽑을 수 있는 위치가 없어서 중간 종료"
                );

                break;
            }


            selectedPoints.Add(picked);
        }


        // --------------------------------------------------
        // 3. 선택된 위치를 열린 창문으로 변경
        // --------------------------------------------------

        for (int i = 0;
             i < selectedPoints.Count;
             i++)
        {
            Transform spawnPoint =
                selectedPoints[i];


            // 해당 위치에 생성되어 있던 닫힌 창문 삭제
            if (closedDoorObjects.ContainsKey(spawnPoint))
            {
                Destroy(
                    closedDoorObjects[spawnPoint]
                );
            }


            // 열린 창문 생성
            Instantiate(
                openedDoor,
                spawnPoint.position,
                spawnPoint.rotation,
                parent
            );


            // --------------------------------------------------
            // enemy1 / enemy2 중 하나 랜덤 선택
            // --------------------------------------------------

            GameObject selectedEnemy;

            // Random.Range(0, 2)는
            // 0 또는 1 중 하나를 반환한다.
            if (Random.Range(0, 2) == 0)
            {
                selectedEnemy = enemy1;
            }
            else
            {
                selectedEnemy = enemy2;
            }


            // 랜덤으로 선택된 적 생성
            Instantiate(
                selectedEnemy,
                spawnPoint.position,
                spawnPoint.rotation,
                parent
            );
        }
    }


    /// <summary>
    /// 후보 위치 중에서
    /// 아직 선택되지 않은 위치 하나를 랜덤으로 반환한다.
    /// </summary>
    private Transform GetRandomPoint(
        Transform[] candidates,
        List<Transform> selectedPoints
    )
    {
        // 선택 가능한 위치들을 임시로 저장한다.
        List<Transform> available =
            new List<Transform>();


        for (int i = 0;
             i < candidates.Length;
             i++)
        {
            // null이 아니고
            // 아직 선택되지 않은 위치만 추가한다.
            if (candidates[i] != null &&
                !selectedPoints.Contains(
                    candidates[i]
                ))
            {
                available.Add(
                    candidates[i]
                );
            }
        }


        // 선택 가능한 위치가 하나도 없는 경우
        if (available.Count == 0)
        {
            return null;
        }


        // 선택 가능한 위치 중 하나 랜덤 선택
        int randIndex =
            Random.Range(
                0,
                available.Count
            );


        return available[randIndex];
    }
}