using UnityEngine;
using System;

public class CloudSpawner : MonoBehaviour
{
    [Header("일반 구름")]
    public GameObject[] normalPrefabs;

    [Header("반짝이는 구름")]
    public GameObject[] shinyPrefabs;

    [Header("반짝일 구름 번호 (스폰 성공 누적 순서 기준)")]
    public int[] shinyCloudIndexes;

    [Header("스폰 위치")]
    public float spawnX = 10f;
    public float spawnY = 1f;

    [Header("스폰 간격 (자동 계산)")]
    [Tooltip("구름 너비 위에 추가로 더 벌릴 여유 간격. 0이면 딱 붙어서 스폰됨.")]
    public float extraMargin = 0.3f;

    [Tooltip("자동 계산된 값. 직접 수정 X - Start()에서 프리팹 너비와 moveStep을 보고 계산됨.")]
    [SerializeField] private int spawnEveryNShows = 1;

    [SerializeField] private Transform parent;

    private int cloudCount = 0;   // 스폰 성공 누적 횟수 (shiny 인덱싱 기준)
    private int showCounter = 0;  // spawnEveryNShows 카운트용

    /// <summary>
    /// 현재 떠 있는 모든 Cloud에게 "한 칸 이동해라"고 알리는 신호.
    /// CSV "Show" 이벤트가 들어올 때마다 OnBeatEvent를 통해 같이 발화된다.
    /// </summary>
    public event Action OnMoveTick;

    void Start()
    {
        RecalculateSpawnInterval();
    }

    /// <summary>
    /// 등록된 모든 프리팹(normal + shiny) 중 가장 넓은 것을 기준으로,
    /// "이 정도는 이동해야 다음 구름과 안 겹친다"는 Show 횟수를 계산한다.
    /// moveStep은 Cloud 프리팹들이 공통으로 쓰는 값이라고 가정한다(첫 프리팹에서 읽음).
    /// </summary>
    private void RecalculateSpawnInterval()
    {
        float maxWidth = 0f;
        float moveStep = 1.5f;
        bool foundMoveStep = false;

        foreach (var prefab in normalPrefabs)
        {
            maxWidth = Mathf.Max(maxWidth, GetWidth(prefab));
            if (!foundMoveStep)
            {
                float? step = GetMoveStep(prefab);
                if (step.HasValue) { moveStep = step.Value; foundMoveStep = true; }
            }
        }

        foreach (var prefab in shinyPrefabs)
        {
            maxWidth = Mathf.Max(maxWidth, GetWidth(prefab));
            if (!foundMoveStep)
            {
                float? step = GetMoveStep(prefab);
                if (step.HasValue) { moveStep = step.Value; foundMoveStep = true; }
            }
        }

        if (maxWidth <= 0f || moveStep <= 0f)
        {
            Debug.LogWarning("[CloudSpawner] 너비 또는 moveStep을 계산할 수 없어 spawnEveryNShows를 기본값(1)으로 둡니다. 프리팹에 SpriteRenderer/Cloud 컴포넌트가 있는지 확인하세요.");
            spawnEveryNShows = 1;
            showCounter = spawnEveryNShows;
            return;
        }

        float requiredDistance = maxWidth + extraMargin;
        spawnEveryNShows = Mathf.Max(1, Mathf.CeilToInt(requiredDistance / moveStep));

        // 첫 Show 이벤트에 바로 스폰되도록, 카운터를 이미 "가득 찬" 상태로 시작시킨다.
        // OnBeatEvent에서 showCounter++ 되자마자 spawnEveryNShows를 넘어서서 즉시 스폰된다.
        showCounter = spawnEveryNShows;

        Debug.Log($"[CloudSpawner] 자동 계산: maxWidth={maxWidth:F2}, moveStep={moveStep:F2}, extraMargin={extraMargin:F2} → spawnEveryNShows={spawnEveryNShows}");
    }

    private float GetWidth(GameObject prefab)
    {
        if (prefab == null) return 0f;
        SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
        return sr != null ? sr.bounds.size.x : 0f;
    }

    private float? GetMoveStep(GameObject prefab)
    {
        if (prefab == null) return null;
        Cloud cloud = prefab.GetComponent<Cloud>();
        return cloud != null ? cloud.moveStep : (float?)null;
    }

    /// <summary>
    /// Minigame_2_9.OnRhythmEvent의 "Show" 케이스에서 호출.
    /// 매번 이동 신호는 발사하고, 자동 계산된 spawnEveryNShows 간격마다 새 구름을 스폰한다.
    /// </summary>
    public void OnBeatEvent()
    {
        showCounter++;

        if (showCounter >= spawnEveryNShows)
        {
            showCounter = 0;
            SpawnCloud();
        }

        // 새로 생긴 구름 포함, 기존에 떠 있던 모든 구름을 같이 한 칸 이동시킨다.
        OnMoveTick?.Invoke();
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