using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class SpawnDust : MonoBehaviour
{
    [SerializeField] private GameObject dustPrefab;
    [SerializeField] private GameObject spawner;

    void OnMouseDown()
    {
        Debug.Log("Å¬¸¯");
        SpawnDustInCloudArea();
    }
    public void SpawnDustInCloudArea()
    {
        Vector3 cloudPos = transform.position;
        Vector3 cloudScale = transform.localScale;

        for (int i = 0; i < 5; i++)
        {
            float offsetX = Random.Range(-cloudScale.x / 2f, cloudScale.x / 2f);
            float offsetY = Random.Range(-cloudScale.y / 2f, cloudScale.y / 2f);

            Vector3 spawnPos = cloudPos + new Vector3(offsetX, offsetY, 0);
            GameObject dust = Instantiate(dustPrefab, spawnPos, Quaternion.identity);
            Destroy(dust, 2f);
        }
    }
}
