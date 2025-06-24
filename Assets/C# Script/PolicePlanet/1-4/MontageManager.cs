using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MontageManager : MonoBehaviour
{
    public GameObject stage_1_4;
    private Minigame_1_4 minigame_1_4;

    public GameObject[] montagePrefabs;        // ������ ����Ʈ
    public Transform mainPosition;             // ���� ��Ÿ�� ���� ��ġ
    public Transform[] optionParents;          // Option �ڽ� ��ġ (3��)

    public int correctMontageId;

    void Start()
    {
        minigame_1_4 = stage_1_4.GetComponent<Minigame_1_4>();
        SetupMontages();
    }

    void SetupMontages()
    {
        // ���� ������ ����
        int correctIndex = Random.Range(0, montagePrefabs.Length);
        GameObject correctPrefab = montagePrefabs[correctIndex];
        correctMontageId = correctPrefab.GetComponent<MontageID>().montageID;

        //���� ��ġ�� ���� ������ ����
        GameObject mainMontage = Instantiate(correctPrefab, mainPosition);
        mainMontage.transform.localPosition = Vector3.zero;
        mainMontage.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // ���� ����

        //���� ������ 3�� ���� ����
        List<GameObject> selectedPrefabs = new List<GameObject>();
        selectedPrefabs.Add(correctPrefab);

        List<int> remainingIndices = new List<int>();
        for (int i = 0; i < montagePrefabs.Length; i++)
            if (i != correctIndex) remainingIndices.Add(i);

        while (selectedPrefabs.Count < 3 && remainingIndices.Count > 0)
        {
            int rand = Random.Range(0, remainingIndices.Count);
            selectedPrefabs.Add(montagePrefabs[remainingIndices[rand]]);
            remainingIndices.RemoveAt(rand);
        }

        //���� �� �ɼ� �ڽĿ� ����
        Shuffle(selectedPrefabs);

        for (int i = 0; i < optionParents.Length; i++)
        {
            GameObject optionMontage = Instantiate(selectedPrefabs[i], optionParents[i]);
            optionMontage.transform.localPosition = Vector3.zero;
            optionMontage.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            
            MontageID montageScript = optionMontage.GetComponent<MontageID>();
            if (montageScript == null)
            {
                Debug.LogError("MontageID ��ũ��Ʈ ����");
                continue;
            }

            Debug.Log($"������ �ɼ� {i}�� ID: {montageScript.montageID}");

            // Option �θ� OptionClickHandler ����
            OptionClickHandler clickHandler = optionParents[i].GetComponent<OptionClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.optionIndex = i;
                clickHandler.manager = this;
            }
        }
    }
    public void CheckAnswer(int optionIndex)
    {
        Transform child = optionParents[optionIndex].GetChild(1);
        MontageID selected = child.GetComponent<MontageID>();
        if (selected.montageID == correctMontageId)
        {
            Debug.Log("����");
            minigame_1_4.Succeed();
        }
        else
        {
            Debug.Log("����");
            minigame_1_4.Failure();
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
