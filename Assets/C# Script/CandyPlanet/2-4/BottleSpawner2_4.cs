using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleSpawner2_4 : MonoBehaviour
{
    [Header("Bottle Prefabs")]
    [SerializeField] private GameObject[] bottlePrefabs; // 둥근 병, 역삼각형 병 등 완성된 프리팹 3개

    [Header("Spawn / Move")]
    [SerializeField] private Transform rightSpawn;
    [SerializeField] private Transform leftEnd;

    [Header("Spawn Y")]
    [SerializeField] private float spawnYOffset = 1f;

    [Header("Parent")]
    [SerializeField] private Transform bottleParent;

    public Bottle2_4 SpawnBottle()
    {
        if (bottlePrefabs == null || bottlePrefabs.Length == 0)
        {
            Debug.LogError("Bottle Prefabs가 설정되지 않았습니다.");
            return null;
        }

        Vector3 spawnPos = new Vector3(
            rightSpawn.position.x,
            rightSpawn.position.y + spawnYOffset,
            rightSpawn.position.z
        );

        // 보틀 프리팹 랜덤 선택
        int randomIndex = Random.Range(0, bottlePrefabs.Length);
        GameObject selectedPrefab = bottlePrefabs[randomIndex];

        GameObject bottleObj = Instantiate(
            selectedPrefab,
            spawnPos,
            Quaternion.identity,
            bottleParent
        );

        Bottle2_4 bottle = bottleObj.GetComponent<Bottle2_4>();

        if (bottle == null)
        {
            Debug.LogError("Bottle Prefab에 Bottle2_4 스크립트가 없습니다.");
            return null;
        }

        bottle.SetTarget(leftEnd);

        return bottle;
    }
}
