using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject obstacle;
    [SerializeField] private PlayerRotate playerRotate;
    [SerializeField] private float spawnPos1 = -6f;
    [SerializeField] private float spawnPos2 = 6f;
    [SerializeField] private float spawnHeight = 6f;

    [SerializeField] private GameObject space;
    [SerializeField] private Transform parent;

    private bool waiting;
    private Queue<Obstacle> obstacleQueue = new Queue<Obstacle>();

    public void SpawnObstacle()
    {
        int randomIndex = Random.Range(0, 2);
        float targetX = (randomIndex == 0) ? spawnPos1 : spawnPos2;
        float direction = (randomIndex == 0) ? 1f : -1f;
        Vector3 spawnPos = new Vector3(targetX, spawnHeight, 0);

        GameObject obj = Instantiate(obstacle, spawnPos, Quaternion.identity, parent);
        Obstacle obs = obj.GetComponent<Obstacle>();
        obs.Init(playerRotate, direction);

        obstacleQueue.Enqueue(obs);
    }
    public void DropNextObstacle()
    {
        if (obstacleQueue.Count == 0) return;

        Obstacle obs = obstacleQueue.Dequeue();

        if (obs != null)
        {
            obs.Drop();
            return;
        }
    }
}


