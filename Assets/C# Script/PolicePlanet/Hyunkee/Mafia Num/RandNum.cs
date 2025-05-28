using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandNum : MonoBehaviour
{
    public Text targetNumberText;
    public Button[] optionButtons;

    private int targetNumber;
    public int randNum1,randNum2,randNum3;
    

    void Start()
    {
        GenerateNewRound();
    }

    void GenerateNewRound()
    {
        // 목표 숫자 생성
        targetNumber = Random.Range(10000, 100000);
        Debug.Log("타겟넘");
        Debug.Log(targetNumber);
        randNum1 = Random.Range(10000, 100000);
        randNum2 = Random.Range(10000, 100000);
        randNum3 = Random.Range(10000, 100000);
        if (randNum1 == targetNumber)
        {
            randNum1 = Random.Range(10000, 100000);

        }
        if (randNum2 == targetNumber)
        {
            randNum2 = Random.Range(10000, 100000);

        }
        if (randNum3 == targetNumber)
        {
            randNum3 = Random.Range(10000, 100000);
        }
        List<int> numbers = new List<int> { 1, 2, 3, 4 };

        // 리스트를 무작위로 섞습니다.
        for (int i = 0; i < numbers.Count; i++)
        {
            int randomIndex = Random.Range(i, numbers.Count);
            int temp = numbers[i];
            numbers[i] = numbers[randomIndex];
            numbers[randomIndex] = temp;
        }
        Debug.Log("으애");
        for (int i = 0; i < numbers.Count; i++)
        {
            Debug.Log(numbers[i]);
        }
        randNum1 = numbers[1];
        randNum2 = numbers[2];
        randNum3 = numbers[3];

        // 결과 출력
        /*foreach (int num in numbers)
        {
            Debug.Log("으애");
            for (int i = 0; i < numbers.Count; i++)
            {
                Debug.Log(numbers[i]);
            }
            
        }*/



    }
        
}
