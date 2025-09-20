using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DountManager : MonoBehaviour
{
    public GameObject[] donutPrefabs;
    public Transform spawnPoint;    // ���� ���� ��ġ
    public Minigame_2_15 minigame;  // �̴ϰ��� ��Ʈ�ѷ�
    public Transform parentTransform;

    private int currentIndex = 0;
    private GameObject currentDonut;

    private void Start()
    {
        parentTransform = minigame.transform;
        SpawnDonut(currentIndex);
    }

    void SpawnDonut(int index)
    {
        if (currentDonut != null) Destroy(currentDonut);

        currentDonut = Instantiate(donutPrefabs[index], spawnPoint.position, Quaternion.identity, parentTransform);
        currentDonut.GetComponent<DonutEater>().Init(this); // Eater�� Manager ����
    }

    public void OnDonutCleared()
    {
        currentIndex++;
        if (currentIndex >= donutPrefabs.Length)
        {
            minigame.Succeed(); // ��� ���� Ŭ����
        }
        else
        {
            SpawnDonut(currentIndex); // ���� ���� ����
        }
    }
}

