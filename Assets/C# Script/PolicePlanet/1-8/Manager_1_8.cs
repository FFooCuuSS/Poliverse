using System.Collections;
using UnityEngine;

public class Manager_1_8 : MonoBehaviour
{
    public GameObject prisonerPrefab;      // 프리팹
    public RectTransform prison;           // 감옥 오브젝트
    public RectTransform canvasTransform;  // 캔버스의 RectTransform
    public int numberOfPrisoners = 5;
    public float gameTime = 60f;

    private GameObject[] prisoners;

    void Start()
    {
        prisoners = new GameObject[numberOfPrisoners];
        StartGame();
    }

    void StartGame()
    {
        for (int i = 0; i < numberOfPrisoners; i++)
        {
            // 프리팹 생성하면서 canvasTransform 하위로 넣기
            GameObject prisonerObj = Instantiate(prisonerPrefab, canvasTransform);
            RectTransform prisonerRect = prisonerObj.GetComponent<RectTransform>();

            // 초기 위치 설정 (랜덤하게 UI 상에서 배치)
            prisonerRect.anchoredPosition = new Vector2(
                Random.Range(-400f, 400f),
                Random.Range(-200f, 200f)
            );

            // 감옥 오브젝트 코드로 할당
            Prisoner_1_8 prisonerScript = prisonerObj.GetComponent<Prisoner_1_8>();
            if (prisonerScript != null)
            {
                prisonerScript.prison = prison;
            }

            prisoners[i] = prisonerObj;
        }

        StartCoroutine(GameTimer());
    }

    IEnumerator GameTimer()
    {
        float timer = gameTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        CheckPrisonersStatus();
    }

    void CheckPrisonersStatus()
    {
        bool allInPrison = true;

        foreach (GameObject prisoner in prisoners)
        {
            RectTransform prisonerRect = prisoner.GetComponent<RectTransform>();
            if (!RectOverlaps(prisonerRect, prison))
            {
                allInPrison = false;
                break;
            }
        }

        Debug.Log(allInPrison ? " 성공! 모든 죄수가 감옥에 있음" : " 실패! 일부 죄수가 감옥 밖에 있음");
    }

    bool RectOverlaps(RectTransform a, RectTransform b)
    {
        Rect aRect = GetWorldRect(a);
        Rect bRect = GetWorldRect(b);
        return aRect.Overlaps(bRect);
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector2 size = corners[2] - corners[0];
        return new Rect(corners[0], size);
    }
}
