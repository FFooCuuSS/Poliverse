using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private GameObject boxPrefab;
    private float[] spawnYPositions = new float[] { 3.2f, 0f, -3.2f };
    public float spawnX = 10f; // ������ x ��ǥ ��ġ
    public float spawnZ = 0f;  // 2D��� ���� 0

    void Start()
    {
        // Resources �������� ������ �ε�
        boxPrefab = Resources.Load<GameObject>("MinigamePrefab/MafiaPlanet/TempPrefab/3_10box");

        if (boxPrefab == null)
        {
            Debug.LogError("�������� ã�� �� �����ϴ�: MinigamePrefab/MafiaPlanet/3_10box");
            return;
        }

        // 1�ʸ��� �ڽ� ����
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
