using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    public GameObject[] prefabs;

    private float currentX = 0f;
    private float lastHalfWidth = 0f;

    public float spawnY = 1f;
    public float spacing = 0.5f;
    public float spawnOffset = -2f;
    public float spawnThreshold = 10f;

    [SerializeField] private Transform parent;
    private GameObject lastCloud;

    public void Init()
    {
        float leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero).x;
        currentX = leftEdge + spawnOffset;
    }
    public bool CanSpawn()
    {
        if (lastCloud == null) return true;

        float rightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;

        return lastCloud.transform.position.x < rightEdge + spawnThreshold;
    }
    public void Spawn()
    {
        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        float halfWidth = GetHalfWidth(prefab);

        // 첫 생성
        if (lastCloud == null)
        {
            Vector3 pos = new Vector3(currentX, spawnY, 0);
            lastCloud = Instantiate(prefab, pos, Quaternion.identity, parent);
            lastHalfWidth = halfWidth;
            return;
        }

        // 마지막 구름 기준 거리 체크
        float lastX = lastCloud.transform.position.x;
        float distance = (lastHalfWidth + halfWidth + spacing);

        Vector3 spawnPos = new Vector3(lastX + distance, spawnY, 0);

        GameObject newCloud = Instantiate(prefab, spawnPos, Quaternion.identity, parent);

        lastCloud = newCloud;
        lastHalfWidth = halfWidth;
    }

    float GetHalfWidth(GameObject prefab)
    {
        SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();

        if (sr == null || sr.sprite == null)
        {
            Debug.LogWarning("Sprite 없음");
            return 0.5f;
        }

        return sr.sprite.bounds.size.x / 2f;
    }
}