using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DrawCard3_4 : MonoBehaviour
{
    public GameObject cardPrefab; // ������ ����
    public Transform[] spawnPositions; // 5���� ī�尡 ��ġ�� ��ġ��
    public float delay = 2f;
    private List<GameObject> spawnedCards = new List<GameObject>();
    public GameObject suspiciousFace;
    public GameObject normalFace;
    bool isCardSet = false;
    bool isCardMoving = false;
    float movingCardSpeed=4f;
    int moveCardTurn=0;
    int CardPos = -7;

    void Start()
    {
        normalFace.GetComponent<SpriteRenderer>().enabled = true;
        SpawnCard();
    }
    private void Update()
    {
        if (isCardSet && moveCardTurn < spawnedCards.Count)
        {
            MoveCard(moveCardTurn);
        }

    }
    void MoveCard(int cardNum)
    {
        isCardMoving = true;
        
        GameObject card = spawnedCards[cardNum];
        Vector3 currentPos = card.transform.position;
        Vector3 targetPos = new Vector3(CardPos, 0, 0); // ��ǥ ��ġ (-7, 0)
        if (spawnedCards[cardNum].GetComponent<CardColor>().isTrapCard)
        {
            ChangeSuspicious();
        }
        else
        {
            isNotSuspicious();
        }
            // �̵�
            card.transform.position = Vector3.MoveTowards(currentPos, targetPos, movingCardSpeed * Time.deltaTime);

        // ���������� ���� ī�� �̵�
        if (Vector3.Distance(card.transform.position, targetPos) < 0.01f)
        {
            isCardMoving = false;
            CardPos += 2;
            moveCardTurn++; // ���� ī��� �̵�
            isNotSuspicious();
        }


    }
    void isNotSuspicious()
    {
        suspiciousFace.GetComponent<SpriteRenderer>().enabled = false;
        normalFace.GetComponent<SpriteRenderer>().enabled = true;
    }
    void ChangeSuspicious()
    {
        suspiciousFace.GetComponent<SpriteRenderer>().enabled = true;
        normalFace.GetComponent<SpriteRenderer>().enabled = false;
    }
    void SpawnCard()
    {
        // ���� ī�� ����
        Debug.Log("SpawnCards() ����");
        foreach (var card in spawnedCards)
        {
            Destroy(card);
        }
        spawnedCards.Clear();
        Vector3 pos = transform.position;
        pos.x = 7; pos.y = 0;
        int trapIndex = Random.Range(0, 5);
        for (int i = 0; i < 5; i++)
        {
            
            
            GameObject card = Instantiate(cardPrefab, pos, Quaternion.identity);

            var cardComponent = card.GetComponent<CardColor>();
            if (i == trapIndex)
            {
                cardComponent.IsSetTrap();
            }
            else
            {
                cardComponent.IsNotTrap();
            }


            if (cardComponent == null)
            {
                Debug.LogError("Card �����տ� Card ��ũ��Ʈ�� �����ϴ�!");
            }

            spawnedCards.Add(card);
            //yield return new WaitForSeconds(delay);
        }
        isCardSet = true;   
    }
   /* IEnumerator SpawnCards()
    {
        // ���� ī�� ����
        Debug.Log("SpawnCards() ����");

        foreach (var card in spawnedCards)
        {
            Destroy(card);
        }
        spawnedCards.Clear();
        int trapIndex = Random.Range(0, 5);


        for (int i = 0; i < 5; i++)
        {
            GameObject card = Instantiate(cardPrefab, spawnPositions[i].position, Quaternion.identity);

            var cardComponent = card.GetComponent<CardColor>();
            if (i== trapIndex)
            {
                cardComponent.isSetTrap();
            }
            else
            {
                cardComponent.isNotTrap();
            }


            if (cardComponent == null)
            {
                Debug.LogError("Card �����տ� Card ��ũ��Ʈ�� �����ϴ�!");
            }

            spawnedCards.Add(card);
            yield return new WaitForSeconds(delay);
        }

    }*/
}
