using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponSpawner_3_15 : MonoBehaviour
{
    [SerializeField] private Vector2 spawnPos;

    [SerializeField] private GameObject[] weaponPrefabs;
    [SerializeField] private GameObject manager_3_15;


    [SerializeField] private float xMoving;
    [SerializeField] private float elaspingTime = 1f;

    public bool banMoving = false;

    void Start()
    {
        spawnPos = transform.GetChild(0).position;
    }

    void Update()
    {
        if (banMoving) return;

        elaspingTime += Time.deltaTime;

        if (elaspingTime >= 1f)
        {
            int idx = Random.value < 0.33f ? 1 : 0;

            GameObject obj = Instantiate(weaponPrefabs[idx], spawnPos, Quaternion.identity);
            weapon_3_15 weapon = obj.GetComponent<weapon_3_15>();
            weapon.GetXMoving(xMoving);
            weapon.manager = manager_3_15.GetComponent<manager_3_15>();

            elaspingTime = 0f;
        }
    }


}
