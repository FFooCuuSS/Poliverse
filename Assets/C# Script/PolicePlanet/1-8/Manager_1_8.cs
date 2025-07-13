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
            Vector2 spawnPos = GetValidSpawnPosition(prisonPos, -1f);

            GameObject prisonerObj = Instantiate(prisonerPrefab[i], spawnPos, Quaternion.identity);
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
        minigame_1_8.Succeed();

        DestroyAllPrisoners();
    }

    public void GameFail()
    {
        DestroyAllPrisoners();
    }

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
}
