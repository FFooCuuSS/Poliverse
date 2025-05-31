using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandNum : MonoBehaviour
{
    
    public TextMeshProUGUI randomTMPText,targetText,randText1,randText2,randText3;
    

    public GameObject randNumObj1, randNumObj2, randNumObj3, targetOBJ;
    public int randNum1,randNum2,randNum3, targetNumber;
    

    void Start()
    {
       
        GenerateNewRound();
        randomTMPText.text = targetNumber.ToString();
        randNumObj1.SetActive(false);
        randNumObj2.SetActive(false);
        randNumObj3.SetActive(false);
        targetOBJ.SetActive(false);
        // 3�� �ڿ� �ؽ�Ʈ �����
        StartCoroutine(HideTextAfterDelay(3f));
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D click = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (click.collider != null)
            {
                GameObject clickedObj = click.collider.gameObject;
                if (clickedObj == targetOBJ)
                {
                    Debug.Log("Clear");
                }
                else
                {
                    Debug.Log("Fail");
                }
            }
        }
    }
    private IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        randomTMPText.gameObject.SetActive(false);
        //�������� ���� �� ����
        AssignRandomPositions();
        //�� ���� ���ڸ� ǥ���ϱ�
        targetText.text = targetNumber.ToString();
        randText1.text = randNum1.ToString();
        randText2.text = randNum2.ToString();
        randText3.text = randNum3.ToString();
        //Ű ���� ���� ���� �� ���������� �༮�� ȭ�鿡 ����

        randNumObj1.SetActive(true);
        randNumObj2.SetActive(true);
        randNumObj3.SetActive(true);
        targetOBJ.SetActive(true);
    }

    void GenerateNewRound()
    {
        targetNumber = Random.Range(10000, 100000);
        Debug.Log("Target Number: " + targetNumber);
        //��ġ�� �ʴ� ���� ����
        randNum1 = GetUniqueRandomNumber(new List<int> { targetNumber });
        randNum2 = GetUniqueRandomNumber(new List<int> { targetNumber, randNum1 });
        randNum3 = GetUniqueRandomNumber(new List<int> { targetNumber, randNum1, randNum2 });
    }

    int GetUniqueRandomNumber(List<int> exclude)
    {
        int rand;
        do
        {
            rand = Random.Range(10000, 100000);
        } while (exclude.Contains(rand));
        return rand;
    }

    void AssignRandomPositions()
    {
        // ��ġ 4�� �������� ����
        List<Vector2> positions = new List<Vector2>
        {
            new Vector2(-2, 0),
            new Vector2(4, 4),
            new Vector2(-2, 4),
            new Vector2(4, 0)
        };

        // ��ġ �����ϰ� ����
        for (int i = 0; i < positions.Count; i++)
        {
            int randIndex = Random.Range(i, positions.Count);
            Vector2 temp = positions[i];
            positions[i] = positions[randIndex];
            positions[randIndex] = temp;
        }

        // ��ġ�� ���� ����Ʈ
        List<GameObject> objList = new List<GameObject>
        {
            randNumObj1,
            randNumObj2,
            randNumObj3,
            targetOBJ
        };

        // ��ġ ������ �Ҵ�
        for (int i = 0; i < objList.Count; i++)
        {
            objList[i].transform.position = positions[i];
        }
    }

}
