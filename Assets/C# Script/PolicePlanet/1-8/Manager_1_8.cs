using System.Collections.Generic;
using UnityEngine;

public class Manager_1_8 : MonoBehaviour
{
    public GameObject stage_1_8;
    public GameObject prisonerPrefab;
    public GameObject prisonObj;

    private Minigame_1_8 minigame_1_8;
    private PrisonTrigger prisonTrigger;

    public int numberOfPrisoners = 3;

    private List<GameObject> prisonerList = new List<GameObject>(); // 생성된 죄수 관리용

    private void Start()
    {
        minigame_1_8 = stage_1_8.GetComponent<Minigame_1_8>();
        prisonTrigger = prisonObj.GetComponent<PrisonTrigger>();

        prisonTrigger.totalPrisoners = numberOfPrisoners;
        prisonTrigger.manager = this;

        StartGame();
    }

    void StartGame()
    {
        for (int i = 0; i < numberOfPrisoners; i++)
        {
            Vector2 spawnPos = GetValidSpawnPosition();

            GameObject prisonerObj = Instantiate(prisonerPrefab, spawnPos, Quaternion.identity);
            Prisoner_1_8 prisoner_1_8 = prisonerObj.GetComponent<Prisoner_1_8>();

            prisoner_1_8.prison = prisonObj;

            prisonerList.Add(prisonerObj);
        }
    }

    Vector2 GetValidSpawnPosition()
    {
        Vector2 spawnPos;

        // x 또는 y 중 하나가 2.5f 이상이 될 때까지 반복
        do
        {
            spawnPos = new Vector2(
                Random.Range(-3f, 3f),
                Random.Range(-3f, 3f)
            );
        }
        while (Mathf.Abs(spawnPos.x) < 2.5f && Mathf.Abs(spawnPos.y) < 2.5f);

        return spawnPos;
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
