using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleSpawner2_4 : MonoBehaviour
{
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private Transform rightSpawn;
    [SerializeField] private Transform leftEnd;

    [SerializeField] private Transform bottleParent;

    public Bottle2_4 SpawnBottle()
    {
        Vector3 spawnPos = new Vector3(
            rightSpawn.position.x,
            rightSpawn.position.y,
            0f
        );

        GameObject obj = Instantiate(
            bottlePrefab,
            spawnPos,
            Quaternion.identity,
            bottleParent
        );

        Bottle2_4 bottle = obj.GetComponent<Bottle2_4>();

        if (bottle == null)
        {
            Debug.LogError("Bottle 프리팹에 Bottle2_4 컴포넌트가 없습니다.");
            Destroy(obj);
            return null;
        }

        Vector3 targetPos = new Vector3(
            leftEnd.position.x,
            leftEnd.position.y,
            0f
        );

        bottle.SetTarget(targetPos);

        return bottle;
    }
}
