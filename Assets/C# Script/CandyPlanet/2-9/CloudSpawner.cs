using UnityEngine;
using System;

public class CloudSpawner : MonoBehaviour
{
    [Header("일반 구름")]
    public GameObject[] normalPrefabs;

    [Header("반짝이는 구름")]
    public GameObject[] shinyPrefabs;

    [Header("반짝일 구름 번호 (스폰 순서 기준)")]
    public int[] shinyCloudIndexes;

    [Header("스폰 위치")]
    public float spawnX = 10f;
    public float spawnY = 1f;

    [Header("속도 조절")]
    [Range(0.1f, 5f)]
    public float speedMultiplier = 1f;

    [SerializeField] private Transform parent;

    private int cloudCount = 0; // 스폰 누적 횟수 (shiny 인덱싱 기준)

    /// <summary>
    /// 현재 떠 있는 모든 Cloud에게 "한 칸 이동해라"고 알리는 신호.
    /// CSV "Show" 이벤트에서 호출.
    /// </summary>
    public event Action OnMoveTick;

    /// <summary>
    /// CSV의 "Show" 이벤트에서 호출 — 이동만 담당.
    /// </summary>
    public void OnBeatEvent()
    {
        OnMoveTick?.Invoke();
    }

    /// <summary>
    /// CSV의 "Spawn" 이벤트에서 호출 — 생성만 담당.
    /// 이제 스폰 타이밍은 자동 계산이 아니라 CSV에 명시적으로 배치된 시점을 따른다.
    /// </summary>
    public void SpawnCloudManual()
    {
        SpawnCloud();
    }

    private void SpawnCloud()
    {
        bool isShiny = IsShinyCloud(cloudCount);

        GameObject prefab = isShiny
            ? shinyPrefabs[UnityEngine.Random.Range(0, shinyPrefabs.Length)]
            : normalPrefabs[UnityEngine.Random.Range(0, normalPrefabs.Length)];

        Vector3 pos = new Vector3(spawnX, spawnY, 0);
        GameObject newCloudObj = Instantiate(prefab, pos, Quaternion.identity, parent);

        Cloud cloud = newCloudObj.GetComponent<Cloud>();
        if (cloud != null)
        {
            cloud.isShiny = isShiny;

            cloud.moveStep *= speedMultiplier;
            cloud.moveDuration /= speedMultiplier;

            cloud.Init(this);
        }
        else
        {
            Debug.LogWarning($"[CloudSpawner] {prefab.name} 프리팹에 Cloud 컴포넌트가 없습니다.");
        }

        cloudCount++;

        Debug.Log($"[CloudSpawner] 구름 스폰 완료: {newCloudObj.name} at {pos} (isShiny={isShiny}, cloudCount={cloudCount})");
    }

    bool IsShinyCloud(int index)
    {
        foreach (int num in shinyCloudIndexes)
        {
            if (num == index)
                return true;
        }

        return false;
    }
}