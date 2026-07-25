using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner_3_11 : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private GameObject bombPrefab;

    [Header("생성된 오브젝트 부모")]
    [SerializeField] private Transform objectParent;

    [Header("중앙 도착 위치")]
    [SerializeField] private Transform centerPoint;

    [Header("소환 설정")]
    [SerializeField] private float firstSpawnTime = 2.5f;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int totalSpawnCount = 8;

    [Header("오브젝트 패턴")]
    [Tooltip("false = Target, true = Bomb")]
    [SerializeField] private bool[] bombPattern;

    [Header("이동 시간")]
    [Tooltip("소환 후 중앙에 도착하는 데 걸리는 시간")]
    [SerializeField] private float travelTime = 1f;

    [Tooltip("중앙을 지난 후 화면 밖까지 이동하는 시간")]
    [SerializeField] private float afterCenterTime = 1f;

    [Header("포물선 높이")]
    [SerializeField] private float minArcHeight = 1.5f;
    [SerializeField] private float maxArcHeight = 3f;

    [Header("회전 속도")]
    [SerializeField] private float minRotation = -300f;
    [SerializeField] private float maxRotation = 300f;

    [Header("중앙 통과 후 이동 거리")]
    [SerializeField] private float endDistance = 14f;

    [Header("전체 소환 범위")]
    [SerializeField] private float minX = -11f;
    [SerializeField] private float maxX = 11f;
    [SerializeField] private float minY = -7f;
    [SerializeField] private float maxY = 7f;

    [Header("소환 금지 범위")]
    [SerializeField] private float blockMinX = -9f;
    [SerializeField] private float blockMaxX = 9f;
    [SerializeField] private float blockMinY = -5f;
    [SerializeField] private float blockMaxY = 5f;

    // 게임 시작 후 흐른 시간
    private float timer;

    // 지금까지 소환한 개수
    private int spawnIndex;

    // 현재 소환 진행 중인지 확인
    private bool isSpawning;

    // 생성된 오브젝트 관리
    private readonly List<FlyingObject_3_11> spawnedObjects =
        new List<FlyingObject_3_11>();

    private void Update()
    {
        if (!isSpawning)
        {
            return;
        }

        timer += Time.deltaTime;
        
        CheckSpawnTime();
        RemoveDestroyedObjects();
    }

    /// <summary>
    /// 외부에서 소환을 시작할 때 호출한다.
    /// GameManager_3_11의 StartGame()에서 호출된다.
    /// </summary>
    public void BeginSpawn()
    {
        timer = 0f;
        spawnIndex = 0;
        isSpawning = true;

        ClearAllObjects();

        Debug.Log("[3-11] 오브젝트 소환 시작");
    }

    /// <summary>
    /// 소환을 중지하고 현재 생성된 물체를 모두 제거한다.
    /// </summary>
    public void StopSpawn()
    {
        isSpawning = false;

        ClearAllObjects();

        Debug.Log("[3-11] 오브젝트 소환 중지");
    }

    /// <summary>
    /// 코드에 지정된 시간에 맞춰 오브젝트를 생성한다.
    /// </summary>
    private void CheckSpawnTime()
    {
        // 총 8번을 모두 생성했다면 더 이상 처리하지 않는다.
        if (spawnIndex >= totalSpawnCount)
        {
            return;
        }

        // 현재 생성할 오브젝트의 소환 시간을 계산한다.
        // 0번: 2.5초
        // 1번: 3.5초
        // 2번: 4.5초
        // ...
        float nextSpawnTime =
            firstSpawnTime +
            spawnIndex * spawnInterval;

        // 아직 다음 소환 시간이 되지 않았다면 대기한다.
        if (timer < nextSpawnTime)
        {
            return;
        }

        SpawnObject(spawnIndex);
        spawnIndex++;

        // 프레임이 크게 밀린 경우에도 누락되지 않도록 추가 확인한다.
        while (spawnIndex < totalSpawnCount)
        {
            nextSpawnTime =
                firstSpawnTime +
                spawnIndex * spawnInterval;

            if (timer < nextSpawnTime)
            {
                break;
            }

            SpawnObject(spawnIndex);
            spawnIndex++;
        }

        if (spawnIndex >= totalSpawnCount)
        {
            Debug.Log("[3-11] 총 8개 소환 완료");
        }
    }

    /// <summary>
    /// 지정된 순서에 맞춰 Target 또는 Bomb을 생성한다.
    /// </summary>
    private void SpawnObject(int index)
    {
        if (centerPoint == null)
        {
            Debug.LogWarning(
                "[3-11] CenterPoint가 연결되지 않았습니다."
            );

            return;
        }

        bool isBomb = GetBombPattern(index);

        GameObject selectedPrefab =
            isBomb ? bombPrefab : targetPrefab;

        if (selectedPrefab == null)
        {
            Debug.LogWarning(
                "[3-11] 생성할 프리팹이 연결되지 않았습니다."
            );

            return;
        }

        Vector3 startPosition =
            GetRandomSpawnPosition();

        Vector3 centerPosition =
            centerPoint.position;

        Vector3 endPosition =
            GetEndPosition(
                startPosition,
                centerPosition
            );

        GameObject spawnedObject = Instantiate(
            selectedPrefab,
            startPosition,
            Quaternion.identity,
            objectParent
        );

        FlyingObject_3_11 flyingObject =
            spawnedObject.GetComponent<FlyingObject_3_11>();

        if (flyingObject == null)
        {
            Debug.LogError(
                "[3-11] 프리팹에 FlyingObject_3_11이 없습니다."
            );

            Destroy(spawnedObject);
            return;
        }

        float arcHeight = Random.Range(
            minArcHeight,
            maxArcHeight
        );

        float arcDirection =
            Random.value < 0.5f ? -1f : 1f;

        float rotationSpeed = Random.Range(
            minRotation,
            maxRotation
        );

        // 정확히 1초 후 중앙에 도착하도록 초기화한다.
        flyingObject.Initialize(
            startPosition,
            centerPosition,
            endPosition,
            travelTime,
            afterCenterTime,
            arcHeight,
            arcDirection,
            rotationSpeed
        );

        spawnedObjects.Add(flyingObject);

        Debug.Log(
            "[3-11] " +
            (index + 1) +
            "번째 소환 / 타입: " +
            (isBomb ? "Bomb" : "Target") +
            " / 소환 시간: " +
            timer.ToString("F2") +
            " / 중앙 도착 예정: " +
            (timer + travelTime).ToString("F2")
        );
    }

    /// <summary>
    /// bombPattern 값을 확인한다.
    /// 값이 없거나 배열 길이가 부족하면 Target으로 생성한다.
    /// </summary>
    private bool GetBombPattern(int index)
    {
        if (bombPattern == null)
        {
            return false;
        }

        if (index < 0 || index >= bombPattern.Length)
        {
            return false;
        }

        return bombPattern[index];
    }

    /// <summary>
    /// 전체 범위에서 랜덤 좌표를 뽑되
    /// 중앙 금지 구역 내부는 제외한다.
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        const int maxTryCount = 100;

        for (int i = 0; i < maxTryCount; i++)
        {
            float randomX = Random.Range(
                minX,
                maxX
            );

            float randomY = Random.Range(
                minY,
                maxY
            );

            bool isInsideBlockedX =
                randomX >= blockMinX &&
                randomX <= blockMaxX;

            bool isInsideBlockedY =
                randomY >= blockMinY &&
                randomY <= blockMaxY;

            // X와 Y가 모두 금지 범위 안이면 다시 뽑는다.
            if (isInsideBlockedX && isInsideBlockedY)
            {
                continue;
            }

            return new Vector3(
                randomX,
                randomY,
                centerPoint.position.z
            );
        }

        // 안전장치: 좌표 생성 실패 시 왼쪽 외곽 사용
        return new Vector3(
            minX,
            0f,
            centerPoint.position.z
        );
    }

    /// <summary>
    /// 시작 위치에서 중앙을 지난 방향으로
    /// 화면 밖 끝 위치를 계산한다.
    /// </summary>
    private Vector3 GetEndPosition(
        Vector3 startPosition,
        Vector3 centerPosition
    )
    {
        Vector3 direction =
            (centerPosition - startPosition).normalized;

        return centerPosition +
               direction * endDistance;
    }

    /// <summary>
    /// 이미 파괴된 오브젝트를 리스트에서 정리한다.
    /// </summary>
    private void RemoveDestroyedObjects()
    {
        for (int i = spawnedObjects.Count - 1;
             i >= 0;
             i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 현재 생성된 오브젝트를 모두 제거한다.
    /// </summary>
    private void ClearAllObjects()
    {
        for (int i = spawnedObjects.Count - 1;
             i >= 0;
             i--)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(
                    spawnedObjects[i].gameObject
                );
            }
        }

        spawnedObjects.Clear();
    }
}