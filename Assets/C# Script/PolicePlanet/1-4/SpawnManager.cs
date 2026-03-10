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
    private List<Accessory> accessories = new List<Accessory>();

    private GameObject selectedMontage;

    

    void Start()
    {
        SpawnNewRound();
    }

    public void SpawnNewRound()
    {
        ClearCurrentObjects();

        accessories.Clear();

        Spawn(hatPrefabs, hatSpawnPoint);
        Spawn(glassesPrefabs, glassesSpawnPoint);
        Spawn(mustachePrefabs, mustacheSpawnPoint);

        SpawnMontage();
    }
    void ClearCurrentObjects() //기존 오브젝트 삭제
    {
        foreach (var acc in accessories)
        {
            if (acc != null)
                Destroy(acc.gameObject);
        }

        if (selectedMontage != null)
        {
            Destroy(selectedMontage);
        }
    }

    public void Spawn(List<GameObject> prefabs, Transform point)
    {
        GameObject selected = Instantiate(
            prefabs[Random.Range(0, prefabs.Count)],
            point
        );

        Accessory acc = selected.GetComponent<Accessory>();
        accessories.Add(acc);
        blinkManager.RegisterAccessory(acc);
    }
    public void SpawnMontage()
    {
        selectedMontage = Instantiate(
            montagePrefabs[Random.Range(0, montagePrefabs.Count)],
            montageSpawnPoint
        );

        Montage montage = selectedMontage.GetComponent<Montage>();
        foreach (var acc in accessories)
        {
            acc.SetMontage(montage);
        }
    }
}
