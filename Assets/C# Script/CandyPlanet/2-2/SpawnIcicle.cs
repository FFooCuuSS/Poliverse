using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnIcicle : MonoBehaviour
{
    public GameObject IciclePrefab;
    [SerializeField] private float spawnInterval = 2f;


  
    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnPrefab();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnPrefab()
    {
        float randomX = Random.Range(-7f, 7f);
        Vector3 spawnPos = new Vector3(randomX, transform.position.y, transform.position.z);
        Instantiate(IciclePrefab, spawnPos, Quaternion.identity);
    }
}