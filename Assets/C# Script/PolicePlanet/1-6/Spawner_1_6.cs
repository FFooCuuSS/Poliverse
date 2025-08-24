using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner1_6 : MonoBehaviour
{
    public GameObject defenderPrefab;
    public GameObject officerPrefab;
    public GameObject offenderPrefab;

    private void Start()
    {
        SpawnAllRandom();
    }

    void SpawnAllRandom()
    {
        List<GameObject> prefabList = new List<GameObject>()
        {
            defenderPrefab,
            officerPrefab,
            offenderPrefab
        };

        List<Vector3> positionList = new List<Vector3>()
        {
            new Vector3(4.25f-1.41f, -0.45f-1.77f, 0f),
            new Vector3(2.49f-1.41f, -0.45f-1.77f, 0f),
            new Vector3(5.93f-1.41f, -0.45f-1.77f, 0f)
        };

        for (int i = 0; i < prefabList.Count; i++)
        {
            int randomIndex = Random.Range(0, positionList.Count);

            Instantiate(prefabList[i], positionList[randomIndex], Quaternion.identity);

            positionList.RemoveAt(randomIndex);//중복배치 방지
        }
    }
}