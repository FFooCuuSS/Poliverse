using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MontageManager : MonoBehaviour
{
    public GameObject stage_1_4;
    private Minigame_1_4 minigame_1_4;

    public GameObject[] montagePrefabs;        // 프리팹 리스트
    public Transform mainPosition;             // 정답 몽타주 생성 위치

    public int correctMontageId;

    void Start()
    {
        minigame_1_4 = stage_1_4.GetComponent<Minigame_1_4>();
        SetupMontages();
    }

    void SetupMontages()
    {
        // 정답 프리팹 고르기
        int correctIndex = Random.Range(0, montagePrefabs.Length);
        GameObject correctPrefab = montagePrefabs[correctIndex];
        correctMontageId = correctPrefab.GetComponent<MontageID>().montageID;

        //메인 위치에 정답 프리팹 생성
        GameObject mainMontage = Instantiate(correctPrefab, mainPosition);
        mainMontage.transform.localPosition = Vector3.zero;
        mainMontage.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f); // 비율 조정

        //정답 포함한 3개 랜덤 선택
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
    }
    public void CheckAnswer(int optionIndex)
    {
        if (optionIndex == correctMontageId)
        {
            minigame_1_4.Succeed();
        }
        else
        {
            minigame_1_4.Failure();
        }
        Debug.Log($"{optionIndex}, {correctMontageId}");
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
