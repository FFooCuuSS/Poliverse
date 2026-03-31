using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    [SerializeField] private int[] spawnPattern; // 0 = left, 1 = right
    private int currentIndex = 0;

    private bool waiting;
    private Queue<Obstacle> obstacleQueue = new Queue<Obstacle>();
    public void Init()
    {
        currentIndex = 0;
    }
    public void SpawnObstacle()
    {
        if (spawnPattern == null || spawnPattern.Length == 0) return;
        int index = spawnPattern[currentIndex];
        float targetX = (index == 0) ? spawnPos1 : spawnPos2;
        float direction = (index == 0) ? 1f : -1f;

        Vector3 spawnPos = new Vector3(targetX, spawnHeight, 0);

        GameObject obj = Instantiate(obstacle, spawnPos, Quaternion.identity, parent);
        // Debug.Log($"Spawn Index: {currentIndex}, Side: {index}");
        Obstacle obs = obj.GetComponent<Obstacle>();
        obs.Init(playerRotate, direction);

        obstacleQueue.Enqueue(obs);

        // 다음 패턴으로 이동
        currentIndex++;

    }
    public void DropNextObstacle()
    {
        while (obstacleQueue.Count > 0)
        {
            Obstacle obs = obstacleQueue.Dequeue();

            if (obs != null)
            {
                obs.Drop();
                break;
            }
        }
    }
}


