using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AccessorySpawnPoint : MonoBehaviour
{
    public List<GameObject> hatPrefabs;
    public List<GameObject> mustachePrefabs;
    public List<GameObject> glassesPrefabs;

    public Transform hatSpawnPoint;
    public Transform mustacheSpawnPoint;
    public Transform glassesSpawnPoint;

    void Start()
    {
        SpawnRandomAccessory(hatPrefabs, hatSpawnPoint);
        SpawnRandomAccessory(mustachePrefabs, mustacheSpawnPoint);
        SpawnRandomAccessory(glassesPrefabs, glassesSpawnPoint);
    }

    void SpawnRandomAccessory(List<GameObject> prefabList, Transform spawnPoint)
    {
        if (prefabList.Count == 0 || spawnPoint == null) return;
        GameObject selected = prefabList[Random.Range(0, prefabList.Count)];
        Instantiate(selected, spawnPoint);
    }
}
