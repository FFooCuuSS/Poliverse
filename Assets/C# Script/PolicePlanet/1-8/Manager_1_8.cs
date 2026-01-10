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

    [Header("Spawn Settings")]
    public int maxPrisonerCount = 3;
    public float spawnY = -1f;

    [Header("Timing Settings")]
    public float timeToReachPrison = 1f;
    public float spawnXOffsetMin = 0f;
    public float spawnXOffsetMax = 1f;

    [Header("Speed Clamp Settings")]
    public float minMoveSpeed = 6f;
    public float maxMoveSpeed = 10f;


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

        Camera cam = Camera.main;

        // 화면 오른쪽 바깥 기준 스폰 X
        float baseSpawnX =
            cam.ViewportToWorldPoint(new Vector3(0.9f, 0f, 0f)).x;

        // 거리 차이를 위한 오프셋
        float offsetX = Random.Range(spawnXOffsetMin, spawnXOffsetMax);

        float spawnX = baseSpawnX + offsetX;
        float spawnY = this.spawnY;

        Vector2 spawnPos = new Vector2(spawnX, spawnY);

        // 프리팹 선택
        int prefabIndex = spawnedCount % prisonerPrefab.Length;
        GameObject prisonerObj =
            Instantiate(prisonerPrefab[prefabIndex], spawnPos, Quaternion.identity);

        Prisoner_1_8 prisoner = prisonerObj.GetComponent<Prisoner_1_8>();
        prisoner.prison = prisonObj;

        // 핵심: 도착 시간 기준 속도 계산
        float prisonX = prisonObj.transform.position.x;
        float distance = spawnX - prisonX;

        float rawSpeed = distance / Mathf.Max(timeToReachPrison, 0.01f);
        float speed = Mathf.Clamp(rawSpeed, minMoveSpeed, maxMoveSpeed);

        prisoner.SetSpeed(speed);

        prisonerList.Add(prisonerObj);
        spawnedCount++;
    }

    private IEnumerator FadeAndDestroy(GameObject obj, float duration)
    {
        if (obj == null) yield break;

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
            if (sr == null || obj == null)
                yield break;

            timer += Time.deltaTime;
            sr.color = Color.Lerp(startColor, endColor, timer / duration);
            yield return null;
        }

        if (obj != null)
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
