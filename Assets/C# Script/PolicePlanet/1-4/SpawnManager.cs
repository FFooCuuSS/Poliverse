using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public List<GameObject> hatPrefabs;
    public List<GameObject> glassesPrefabs;
    public List<GameObject> mustachePrefabs; 
    public List<GameObject> montagePrefabs;


    public Transform hatSpawnPoint;
    public Transform glassesSpawnPoint;
    public Transform mustacheSpawnPoint;
    public Transform montageSpawnPoint;

    public AccessoryBlinkManager blinkManager;

    void Start()
    {
        Spawn(hatPrefabs, hatSpawnPoint);
        Spawn(glassesPrefabs, glassesSpawnPoint);
        Spawn(mustachePrefabs, mustacheSpawnPoint);
        SpawnMontage();
    }

    void Spawn(List<GameObject> prefabs, Transform point)
    {
        GameObject selected = Instantiate(
            prefabs[Random.Range(0, prefabs.Count)],
            point
        );

        Accessory acc = selected.GetComponent<Accessory>();
        blinkManager.RegisterAccessory(acc);
    }
    void SpawnMontage()
    {
        Instantiate(
            montagePrefabs[Random.Range(0, montagePrefabs.Count)],
            montageSpawnPoint
        );
    }
}
