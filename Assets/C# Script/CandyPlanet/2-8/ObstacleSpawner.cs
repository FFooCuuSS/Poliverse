using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject obstacle;
    [SerializeField] private float spawnPos1 = -6f;
    [SerializeField] private float spawnPos2 = 6f;
    [SerializeField] private float spawnHeight = 6f;

    [SerializeField] private GameObject space;

    public void SpawnObstacle()
    {
        int randomIndex = Random.Range(0, 2);
        float targetX = (randomIndex == 0) ? spawnPos1 : spawnPos2;
        Vector3 spawnPos = new Vector3(targetX, spawnHeight, 0);
        Instantiate(obstacle, spawnPos, Quaternion.identity);
    }
}


