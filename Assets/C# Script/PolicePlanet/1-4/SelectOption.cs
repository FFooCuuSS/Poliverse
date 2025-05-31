using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectOption : MonoBehaviour
{
    public List<GameObject> Montages;
    public GameObject correctMontage;
    //private List<GameObject> wrongMontages;

    public Transform[] spawnPoints; //canvas 안에 위치시키기 (button Onclick 기능 땜에)

    private void Start()
    {
        DrawCorrectMontage(Montages);
        SpawnMontage(Montages, spawnPoints);
    }

    void DrawCorrectMontage(List<GameObject> optionList)
    {
        if (optionList.Count == 0) return;
        int random = Random.Range(0, optionList.Count);
        correctMontage = optionList[random];

        /*for (int i = 0; i < optionList.Count; i++)
        {
            if ( i != random)
            {
                wrongMontages.Add(optionList[i]);
            }
        }*/
    }

    void SpawnMontage(List<GameObject> optionList, Transform[] spawnPoints)
    {
        List<GameObject> shuffled = new List<GameObject>(optionList);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int random = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[random]) = (shuffled[random], shuffled[i]);
        }

        //Debug.Log($"Montage: {shuffled[1]}");

        for(int i = 0; i < shuffled.Count; i++)
        {
            Debug.Log(spawnPoints[i].transform);
            Instantiate(shuffled[i], spawnPoints[i]);
        }
    }

    void SelectMontage(GameObject clickedOption)
    {
       // if(clickedOption.)
    }
}
