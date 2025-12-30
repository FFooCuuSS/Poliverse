using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public List<GameObject> hatPrefabs;
    public List<GameObject> mustachePrefabs;
    public List<GameObject> glassesPrefabs;
    public List<GameObject> montagePrefabs;

    public Transform hatSpawnPoint;
    public Transform mustacheSpawnPoint;
    public Transform glassesSpawnPoint;
    public Transform montageSpawnPoint;

    public Minigame_1_4 minigame;

    void Start()
    {
        SpawnRandom(hatPrefabs, hatSpawnPoint);
        SpawnRandom(mustachePrefabs, mustacheSpawnPoint);
        SpawnRandom(glassesPrefabs, glassesSpawnPoint);
        SpawnRandom(montagePrefabs, montageSpawnPoint);
    }

    void SpawnRandom(List<GameObject> prefabList, Transform spawnPoint)
    {
        if (prefabList.Count == 0 || spawnPoint == null) return;

        GameObject selected = prefabList[Random.Range(0, prefabList.Count)];
        GameObject accObj = Instantiate(selected, spawnPoint);

        Accessory acc = accObj.GetComponent<Accessory>();
        if (acc != null)
            minigame.RegisterAccessory(acc);
    }
}
