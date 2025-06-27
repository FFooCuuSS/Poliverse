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

    private List<GameObject> prisonerList = new List<GameObject>(); // ������ �˼� ������

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
        float spawnRadiusMin = 4f;
        float spawnRadiusMax = 6f;

        Vector2 spawnPos = Random.insideUnitCircle.normalized * Random.Range(spawnRadiusMin, spawnRadiusMax);
        return spawnPos;
    }

    public void GameSuccess()
    {
        Debug.Log("����! ��� �˼��� ���� �ȿ� ����");
        minigame_1_8.Succeed();

        DestroyAllPrisoners();
    }

    public void GameFail()
    {
        Debug.Log("����! �Ϻ� �˼��� ���� �ۿ� ����");

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

        prisonerList.Clear(); // ����Ʈ �ʱ�ȭ
    }
}
