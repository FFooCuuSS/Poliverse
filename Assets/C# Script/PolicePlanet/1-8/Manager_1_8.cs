using System.Collections.Generic;
using UnityEngine;

public class Manager_1_8 : MonoBehaviour
{
    public GameObject stage_1_8;
    public GameObject prisonerPrefab;
    public GameObject prisonObj;

    private Minigame_1_8 minigame_1_8;
    private PrisonTrigger prisonTrigger;
    private Vector2 prisonPos;

    public int numberOfPrisoners = 3;

    private List<GameObject> prisonerList = new List<GameObject>(); // 생성된 죄수 관리용

    private void Start()
    {
        minigame_1_8 = stage_1_8.GetComponent<Minigame_1_8>();
        prisonTrigger = prisonObj.GetComponent<PrisonTrigger>();

        prisonTrigger.totalPrisoners = numberOfPrisoners;
        prisonTrigger.manager = this;
        prisonPos = prisonObj.transform.position;

        StartGame();
    }

    void StartGame()
    {
        for (int i = 0; i < numberOfPrisoners; i++)
        {
            Vector2 spawnPos = GetValidSpawnPosition(prisonPos, prisonPos.y);

            GameObject prisonerObj = Instantiate(prisonerPrefab, spawnPos, Quaternion.identity);
            Prisoner_1_8 prisoner_1_8 = prisonerObj.GetComponent<Prisoner_1_8>();

            prisoner_1_8.prison = prisonObj;

            prisonerList.Add(prisonerObj);
        }
    }

    Vector2 GetValidSpawnPosition(Vector2 center, float fixedY)
    {
        float spawnRadiusMin = 4f;
        float spawnRadiusMax = 6f;

        // x축에서만 거리 유지
        float randomDistance = Random.Range(spawnRadiusMin, spawnRadiusMax);
        float direction = Random.value < 0.5f ? -1f : 1f;
        float offsetX = randomDistance * direction;

        return new Vector2(center.x + offsetX, fixedY);
    }

    public void GameSuccess()
    {
        Debug.Log("성공! 모든 죄수가 감옥 안에 있음");
        minigame_1_8.Succeed();

        DestroyAllPrisoners();
    }

    public void GameFail()
    {
        Debug.Log("실패! 일부 죄수가 감옥 밖에 있음");

        DestroyAllPrisoners();
    }

    public void DestroyAllPrisoners()
    {
        foreach (GameObject prisoner in prisonerList)
        {
            if (prisoner != null)
            {
                Destroy(prisoner);
            }
        }

        prisonerList.Clear(); // 리스트 초기화
    }
}
