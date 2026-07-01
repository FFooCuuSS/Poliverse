using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject obstacle;
    [SerializeField] private float spawnPos1 = -6f;
    [SerializeField] private float spawnPos2 = 6f;
    [SerializeField] private float spawnHeight = 6f;

    [SerializeField] private Transform parent;

    [SerializeField] private int[] spawnPattern; // 0 = left, 1 = right
    private int currentIndex = 0;

    public void Init()
    {
        currentIndex = 0;
    }

    // 1,2박: 장애물 낙하(연출) 시작과 동시에 판을 바로 기울여서
    // 플레이어가 3,4박 입력 타이밍을 미리 인지할 수 있게 함
    public void SpawnObstacle(PlayerRotate playerRotate)
    {
        if (spawnPattern == null || spawnPattern.Length == 0) return;

        int index = spawnPattern[currentIndex % spawnPattern.Length];
        float targetX = (index == 0) ? spawnPos1 : spawnPos2;
        float direction = (index == 0) ? 1f : -1f;

        Vector3 spawnPos = new Vector3(targetX, spawnHeight, 0);
        Instantiate(obstacle, spawnPos, Quaternion.identity, parent);

        // 판을 즉시 기울임 (입력 타이밍 예고)
        playerRotate.AddImpactAngle(direction * 15f);

        currentIndex++;
    }
}