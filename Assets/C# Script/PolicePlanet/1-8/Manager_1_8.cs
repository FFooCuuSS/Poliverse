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
    //public int maxPrisonerCount = 3;
    public float spawnY = -3.5f;

    [Header("Timing Settings")]
    public float timeToReachPrison = 1f;
    public float spawnXOffsetMin = 0f;
    public float spawnXOffsetMax = 1f;

    [Header("Speed Clamp Settings")]
    public float minMoveSpeed = 6f;
    public float maxMoveSpeed = 10f;

    public bool hasAnySuccess = false;
    public int endedPrisoner = 0;
    private bool isEnded = false;

    [Header("Round Settings")]
    public int totalRounds = 3;          // 총 라운드
    public int prisonersPerRound = 4;    // 라운드당 범인 수

    private int currentRound = 0;
    private int spawnedThisRound = 0;

    private int successRounds = 0;
    private int failRounds = 0;

    private void Start()
    {
        minigame_1_8 = stage_1_8.GetComponent<Minigame_1_8>();
        prisonTrigger = prisonObj.GetComponent<PrisonTrigger>();

        prisonTrigger.manager = this;

        StartRound();
    }

    void StartRound()
    {
        Debug.Log("Round Start : " + (currentRound + 1));

        spawnedThisRound = 0;
        endedPrisoner = 0;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < prisonersPerRound; i++)
        {
            SpawnPrisoner();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void Update()
    {
        if (endedPrisoner >= prisonersPerRound && !isEnded)
        {
            isEnded = true;

            if (hasAnySuccess)
                successRounds++;
            else
                failRounds++;

            currentRound++;

            if (currentRound >= totalRounds)
            {
                FinalResult();
            }
            else
            {
                StartCoroutine(StartNextRound());
            }
        }
    }

    public void DestroyAllPrisoners()
    {
        foreach (GameObject prisoner in prisonerList)
        {
            if (prisoner == null) continue;

            Prisoner_1_8 p = prisoner.GetComponent<Prisoner_1_8>();

            // 잡힌 범인은 즉시 제거하지 않음
            if (p != null && p.IsCaptured)
                continue;

            StartCoroutine(FadeAndDestroy(prisoner, 0.3f));
        }

        // 리스트에서 살아있는 것만 유지
        prisonerList.RemoveAll(p =>
        {
            if (p == null) return true;
            Prisoner_1_8 pr = p.GetComponent<Prisoner_1_8>();
            return pr != null && pr.IsCaptured;
        });
    }


    public void SpawnPrisoner()
    {
        if (spawnedThisRound >= prisonersPerRound)
            return;

        Camera cam = Camera.main;

        float baseSpawnX =
            cam.ViewportToWorldPoint(new Vector3(0.9f, 0f, 0f)).x;

        float offsetX = Random.Range(spawnXOffsetMin, spawnXOffsetMax);

        float spawnX = baseSpawnX + offsetX;
        float spawnY = this.spawnY;

        Vector2 spawnPos = new Vector2(spawnX, spawnY);

        int prefabIndex = spawnedThisRound % prisonerPrefab.Length;

        GameObject prisonerObj =
            Instantiate(prisonerPrefab[prefabIndex], spawnPos, Quaternion.identity, transform);

        Prisoner_1_8 prisoner = prisonerObj.GetComponent<Prisoner_1_8>();
        prisoner.prison = prisonObj;

        float prisonX = prisonObj.transform.position.x;
        float distance = spawnX - prisonX;

        float rawSpeed = distance / Mathf.Max(timeToReachPrison, 0.01f);
        float speed = Mathf.Clamp(rawSpeed, minMoveSpeed, maxMoveSpeed);

        prisoner.SetSpeed(speed);

        prisonerList.Add(prisonerObj);

        spawnedThisRound++;
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

    IEnumerator StartNextRound()
    {
        yield return new WaitForSeconds(1f);

        isEnded = false;
        hasAnySuccess = false;

        StartRound();
    }

    void FinalResult()
    {
        Debug.Log("Success Rounds : " + successRounds);

        if (successRounds >= 3)
            minigame_1_8.Success();
        else
            minigame_1_8.Fail();
    }
}
