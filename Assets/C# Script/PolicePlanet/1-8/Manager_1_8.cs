using System.Collections;
using UnityEngine;

public class Manager_1_8 : MonoBehaviour
{
    public GameObject prisonerPrefab;      // ������
    public RectTransform prison;           // ���� ������Ʈ
    public RectTransform canvasTransform;  // ĵ������ RectTransform
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
            // ������ �����ϸ鼭 canvasTransform ������ �ֱ�
            GameObject prisonerObj = Instantiate(prisonerPrefab, canvasTransform);
            RectTransform prisonerRect = prisonerObj.GetComponent<RectTransform>();

            // �ʱ� ��ġ ���� (�����ϰ� UI �󿡼� ��ġ)
            prisonerRect.anchoredPosition = new Vector2(
                Random.Range(-400f, 400f),
                Random.Range(-200f, 200f)
            );

            // ���� ������Ʈ �ڵ�� �Ҵ�
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

        Debug.Log(allInPrison ? " ����! ��� �˼��� ������ ����" : " ����! �Ϻ� �˼��� ���� �ۿ� ����");
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
