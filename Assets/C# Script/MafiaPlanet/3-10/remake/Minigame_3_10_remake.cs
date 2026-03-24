using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_10_remake : MiniGameBase
{
    public GameObject warningSign1;
    public GameObject warningSign2;
    public GameObject enemy;
    //public GameObject warningSign3;

    public Transform[] spawnPoints; // 3개라고 가정
    float fixedEnemyPosX = 11f;

    // 현재 선택된 스폰 인덱스 저장
    private int index1;
    private int index2;

    void Start()
    {
        base.StartGame();
        // 시작할 때 랜덤 위치 먼저 세팅
        SetRandomPositions();
    }

    public override void OnRhythmEvent(string action)
    {
        if (action == "Show")
        {
            Debug.Log("[Show] Show 이벤트 들어옴");
            ShowObject();
        }

        if (action == "Spawn")
        {
            Debug.Log("[Spawn] Spawn 이벤트 들어옴");
            SpawnObject();
        }
    }

    // 랜덤으로 서로 다른 2개 인덱스 뽑기
    void SetRandomPositions()
    {
        index1 = Random.Range(0, spawnPoints.Length);

        // index2는 index1과 겹치지 않게
        do
        {
            index2 = Random.Range(0, spawnPoints.Length);
        } while (index2 == index1);

        // 위치 배치
        warningSign1.transform.position = spawnPoints[index1].position;
        warningSign2.transform.position = spawnPoints[index2].position;
    }

    // Show 처리
    void ShowObject()
    {
        SetRandomPositions();

        // 표시
        warningSign1.SetActive(true);
        warningSign2.SetActive(true);

        // 2초 후 숨기기 + 재배치
        Invoke(nameof(Hide), 2f);
    }

    // 2초 뒤 실행
    void Hide()
    {
        warningSign1.SetActive(false);
        warningSign2.SetActive(false);

    }

    // Spawn 처리
    void SpawnObject()
    {
        // spawnPoints 위치 가져오되 x만 11로 고정
        Vector3 pos1 = spawnPoints[index1].position;
        Vector3 pos2 = spawnPoints[index2].position;

        pos1.x = fixedEnemyPosX;
        pos2.x = fixedEnemyPosX;

        Instantiate(enemy, pos1, Quaternion.identity);
        Instantiate(enemy, pos2, Quaternion.identity);
    }
}
