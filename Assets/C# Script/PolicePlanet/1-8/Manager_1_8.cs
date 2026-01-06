using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_1_8 : MonoBehaviour
{
    public GameObject stage_1_8;
    public GameObject[] prisonerPrefab;
    public GameObject prisonObj;

    private Minigame_1_8 minigame_1_8;
    private PrisonTrigger prisonTrigger;
    private Vector2 prisonPos;

    public int numberOfPrisoners = 3;

    private int spawnedCount = 0;
    private List<GameObject> prisonerList = new List<GameObject>();

    public float timeToReachPrison = 1f;

    [Header("Spawn Settings")]
    public int maxPrisonerCount = 3;

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
        //for (int i = 0; i < numberOfPrisoners; i++)
        //{
        //    Vector2 spawnPos = new Vector2(7f, Random.Range(-2f, 2f));

        //    GameObject prisonerObj = Instantiate(prisonerPrefab[i], spawnPos, Quaternion.identity);
        //    Prisoner_1_8 prisoner_1_8 = prisonerObj.GetComponent<Prisoner_1_8>();

        //    prisoner_1_8.prison = prisonObj;

        //    prisonerList.Add(prisonerObj);
        //}
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

    //public void GameSuccess()
    //{
    //    minigame_1_8.Succeed();

    //    DestroyAllPrisoners();
    //}

    //public void GameFail()
    //{
    //    DestroyAllPrisoners();
    //}

    public void DestroyAllPrisoners()
    {
        foreach (GameObject prisoner in prisonerList)
        {
            if (prisoner != null)
            {
                StartCoroutine(FadeAndDestroy(prisoner, 0.3f));
            }
        }

        prisonerList.Clear(); // 리스트 초기화
    }

    public void SpawnPrisoner()
    {
        if (spawnedCount >= maxPrisonerCount)
            return;

        // 감옥 기준 스폰 위치 계산
        float prisonX = prisonObj.transform.position.x;
        float spawnX = prisonX + 7f; // 감옥 오른쪽에서 등장
        float spawnY = Random.Range(-2f, 2f);

        Vector2 spawnPos = new Vector2(spawnX, spawnY);

        // 프리팹 인덱스 순환
        int prefabIndex = spawnedCount % prisonerPrefab.Length;

        GameObject prisonerObj =
            Instantiate(prisonerPrefab[prefabIndex], spawnPos, Quaternion.identity);

        Prisoner_1_8 prisoner = prisonerObj.GetComponent<Prisoner_1_8>();
        prisoner.prison = prisonObj;

        // 이동 거리 → 속도 계산
        float distance = spawnPos.x - prisonX;
        float speed = distance / timeToReachPrison;
        prisoner.SetSpeed(speed);

        prisonerList.Add(prisonerObj);
        spawnedCount++;
    }


    private IEnumerator FadeAndDestroy(GameObject obj, float duration)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Destroy(obj);
            yield break;
        }

        float timer = 0f;
        Color startColor = sr.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            sr.color = Color.Lerp(startColor, endColor, timer / duration);
            yield return null;
        }

        sr.color = endColor;
        Destroy(obj);
    }

    public void SendRhythmInput()
    {
        minigame_1_8.OnPlayerInput();
    }

    public void NotifyMiss()
    {
        //minigame_1_8.OnMiss();
    }
}
