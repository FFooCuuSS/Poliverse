using System.Collections.Generic;
using UnityEngine;

public class SpawnPolice1_5 : MonoBehaviour
{
    public GameObject enemyPrefab;   // enemy1_5 ������
    public GameObject policePrefab;  // police1_5 ������
    public Transform parentTransform; // ��ȯ�� �θ� Ʈ������

    public int totalSpawnCount = 15;   // �� ��ȯ �� (�׻� 15)
    public float yValue = -1.779531f;  // y ���� (0 + -1.779531)
    public float zValue = 0f;          // z ����
    public float xStart = -5.573807f;  // ���� x ��ǥ (-4.5 + -1.073807)
    public float xEnd = 10.5f;         // �� x ��ǥ (���� ����)
    public float xStep = 1.0f;         // x ����

    private List<Vector3> candidatePositions; // ������ ��ȯ ��ġ��

    private void Awake()
    {
        // x ��ǥ �ĺ� ��ġ ����
        candidatePositions = new List<Vector3>();
        float xPosition = xStart;
        while (xPosition <= xEnd + 0.0001f)
        {
            candidatePositions.Add(new Vector3(xPosition, yValue, zValue));
            xPosition += xStep;
        }
    }

    private void Start()
    {
        // �׽�Ʈ��: ���� �� 1�ܰ� ��ȯ
        SpawnByStage(1);
    }

    public void SpawnByStage(int stage)
    {
        if (enemyPrefab == null || policePrefab == null)
        {
            Debug.LogError("enemyPrefab �Ǵ� policePrefab�� ����ֽ��ϴ�.");
            return;
        }

        // �ܰ躰 enemy ����
        int enemyCount = 0;
        if (stage == 1) enemyCount = 5;
        else if (stage == 2) enemyCount = 6;
        else if (stage == 3) enemyCount = 7;
        else
        {
            Debug.LogWarning("�߸��� stage ���Դϴ�: " + stage);
            return;
        }

        // �� �������� police ���� ���
        if (enemyCount > totalSpawnCount) enemyCount = totalSpawnCount;
        int policeCount = totalSpawnCount - enemyCount;

        // �ĺ� ��ǥ �� üũ
        if (candidatePositions.Count < totalSpawnCount)
        {
            Debug.LogWarning("�ĺ� ��ǥ�� �����մϴ�.");
            int maxSpawn = candidatePositions.Count;
            if (enemyCount > maxSpawn) enemyCount = maxSpawn;
            policeCount = maxSpawn - enemyCount;
        }

        // ��ġ �ε��� ����Ʈ ����� ����
        List<int> indexes = new List<int>();
        for (int i = 0; i < candidatePositions.Count; i++)
        {
            indexes.Add(i);
        }

        for (int i = indexes.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            int tmp = indexes[i];
            indexes[i] = indexes[r];
            indexes[r] = tmp;
        }

        int spawned = 0;

        // enemy ��ġ
        for (int i = 0; i < enemyCount; i++)
        {
            int pick = indexes[spawned];
            Vector3 pos = candidatePositions[pick];
            Instantiate(enemyPrefab, pos, Quaternion.identity, parentTransform);
            spawned += 1;
        }

        // police ��ġ
        for (int i = 0; i < policeCount; i++)
        {
            int pick = indexes[spawned];
            Vector3 pos = candidatePositions[pick];
            Instantiate(policePrefab, pos, Quaternion.identity, parentTransform);
            spawned += 1;
        }
    }
}
