using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private GameObject boxPrefab;
    private float[] spawnYPositions = new float[] { 3.2f, 0f, -3.2f };
    public float spawnX = 10f; // 생성될 x 좌표 위치
    public float spawnZ = 0f;  // 2D라면 보통 0

    void Start()
    {
        // Resources 폴더에서 프리팹 로드
        boxPrefab = Resources.Load<GameObject>("MinigamePrefab/MafiaPlanet/TempPrefab/3_10box");

        if (boxPrefab == null)
        {
            Debug.LogError("프리팹을 찾을 수 없습니다: MinigamePrefab/MafiaPlanet/3_10box");
            return;
        }

        // 1초마다 박스 생성
        StartCoroutine(SpawnBoxesEverySecond());
    }

    IEnumerator SpawnBoxesEverySecond()
    {
        while (true)
        {
            SpawnBoxAtRandomY();
            yield return new WaitForSeconds(1f);
        }
    }

    void SpawnBoxAtRandomY()
    {
        int randIndex = Random.Range(0, spawnYPositions.Length);
        float y = spawnYPositions[randIndex];
        Vector3 spawnPos = new Vector3(spawnX, y, spawnZ);

        Instantiate(boxPrefab, spawnPos, Quaternion.identity);
    }
}
