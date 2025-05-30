using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DrawCard3_4 : MonoBehaviour
{
    public GameObject cardPrefab; // 프리팹 연결
    public Transform[] spawnPositions; // 5장의 카드가 위치할 위치들
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
        Vector3 targetPos = new Vector3(CardPos, 0, 0); // 목표 위치 (-7, 0)
        if (spawnedCards[cardNum].GetComponent<CardColor>().isTrapCard)
        {
            ChangeSuspicious();
        }
        else
        {
            isNotSuspicious();
        }
            // 이동
            card.transform.position = Vector3.MoveTowards(currentPos, targetPos, movingCardSpeed * Time.deltaTime);

        // 도착했으면 다음 카드 이동
        if (Vector3.Distance(card.transform.position, targetPos) < 0.01f)
        {
            isCardMoving = false;
            CardPos += 2;
            moveCardTurn++; // 다음 카드로 이동
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
        // 이전 카드 제거
        Debug.Log("SpawnCards() 시작");
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
                Debug.LogError("Card 프리팹에 Card 스크립트가 없습니다!");
            }

            spawnedCards.Add(card);
            //yield return new WaitForSeconds(delay);
        }
        isCardSet = true;   
    }
   /* IEnumerator SpawnCards()
    {
        // 이전 카드 제거
        Debug.Log("SpawnCards() 시작");

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
                Debug.LogError("Card 프리팹에 Card 스크립트가 없습니다!");
            }

            spawnedCards.Add(card);
            yield return new WaitForSeconds(delay);
        }

    }*/
}
