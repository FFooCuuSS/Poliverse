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
    public Transform[] optionParents;          // Option 자식 위치 (3개)

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
        mainMontage.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // 비율 조정

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

        //셔플 후 옵션 자식에 생성
        Shuffle(selectedPrefabs);

        for (int i = 0; i < optionParents.Length; i++)
        {
            GameObject optionMontage = Instantiate(selectedPrefabs[i], optionParents[i]);
            optionMontage.transform.localPosition = Vector3.zero;
            optionMontage.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            
            MontageID montageScript = optionMontage.GetComponent<MontageID>();
            if (montageScript == null)
            {
                Debug.LogError("MontageID 스크립트 없음");
                continue;
            }

            Debug.Log($"생성된 옵션 {i}의 ID: {montageScript.montageID}");

            // Option 부모에 OptionClickHandler 연결
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
            Debug.Log("정답");
            minigame_1_4.Succeed();
        }
        else
        {
            Debug.Log("오답");
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
