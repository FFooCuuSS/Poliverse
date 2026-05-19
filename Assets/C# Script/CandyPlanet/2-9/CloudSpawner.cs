using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [Header("일반 구름")]
    public GameObject[] normalPrefabs;

    [Header("반짝이는 구름")]
    public GameObject[] shinyPrefabs;

    [Header("반짝일 구름 번호")]
    public int[] shinyCloudIndexes;

    public float spawnX = 10f;
    public float spawnY = 1f;
    public float spacing = 0.5f;

    [SerializeField] private Transform parent;

    private GameObject lastCloud;

    private int cloudCount = 0;

    public void TrySpawn()
    {
        bool isShiny = IsShinyCloud(cloudCount);

        GameObject prefab;

        if (isShiny)
        {
            prefab = shinyPrefabs[
                Random.Range(0, shinyPrefabs.Length)
            ];
        }
        else
        {
            prefab = normalPrefabs[
                Random.Range(0, normalPrefabs.Length)
            ];
        }

        if (!CanSpawn(prefab)) return;

        Vector3 pos = new Vector3(spawnX, spawnY, 0);

        GameObject newCloud =
            Instantiate(prefab, pos, Quaternion.identity, parent);

        lastCloud = newCloud;

        cloudCount++;
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

    bool CanSpawn(GameObject prefab)
    {
        if (lastCloud == null) return true;

        float lastLeft = GetLeft(lastCloud);
        float newHalf = GetHalfWidth(prefab);

        float newRight = spawnX + newHalf;

        return newRight < lastLeft - spacing;
    }

    float GetHalfWidth(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();
        return sr.bounds.size.x / 2f;
    }

    float GetLeft(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();
        return sr.bounds.min.x;
    }
}