using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRing : MonoBehaviour
{
    [SerializeField] private GameObject ringPrefab;
    public Transform spawnPoint;

    public void SpawnRings(int count)
    {
        StartCoroutine(SpawnRoutine(count));
    }

    IEnumerator SpawnRoutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Instantiate(ringPrefab, spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
        }
    }
}
