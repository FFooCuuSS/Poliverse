using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DountManager : MonoBehaviour
{
    public GameObject[] donutPrefabs;
    public Transform spawnPoint;    // 도넛 생성 위치
    public Minigame_2_15 minigame;  // 미니게임 컨트롤러
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
        currentDonut.GetComponent<DonutEater>().Init(this); // Eater에 Manager 전달
    }

    public void OnDonutCleared()
    {
        currentIndex++;
        if (currentIndex >= donutPrefabs.Length)
        {
            minigame.Succeed(); // 모든 도넛 클리어
        }
        else
        {
            SpawnDonut(currentIndex); // 다음 도넛 생성
        }
    }
}

