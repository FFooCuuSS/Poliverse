using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DrawCard3_4 : MonoBehaviour
{
    public GameObject cardPrefab; // 프리팹 연결
    public Transform[] spawnPositions; // 5장의 카드가 위치할 위치들
    //public float delay = 2f;
    private List<GameObject> spawnedCards = new List<GameObject>();
    public GameObject suspiciousFace;
    public GameObject normalFace;
    bool isCardSet = false;
    bool isCardMoving = false;
    float movingCardSpeed=13f;
    int moveCardTurn=0;
    int CardPos = -4;


    // 카드 이동에 걸리는 시간
    private float moveDuration = 0.5f;

    // 이동 시작 시간을 저장
    private float moveStartTime;
    private Vector3 startPos;
    private Vector3 targetPos;

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
        else
        {
            GetComponent<ChooseCard>().start=true;
        }

    }
    void MoveCard(int cardNum)
    {
        GameObject card = spawnedCards[cardNum];

        // 처음 실행될 때만 초기화
        if (!isCardMoving)
        {
            isCardMoving = true;

            startPos = card.transform.position;
            targetPos = new Vector3(CardPos, -1.5f, 0);
            moveStartTime = Time.time;

            if (card.GetComponent<CardColor>().isTrapCard)
            {
                ChangeSuspicious();
            }
            else
            {
                isNotSuspicious();
            }
        }

        // 경과 시간 비율 계산 (0 ~ 1)
        float t = (Time.time - moveStartTime) / moveDuration;

        // 부드럽게 이동
        card.transform.position = Vector3.Lerp(startPos, targetPos, t);

        // 도착 체크
        if (t >= 1f)
        {
            card.transform.position = targetPos; // 정확히 맞춰줌
            isCardMoving = false;
            CardPos += 2;
            moveCardTurn++;
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
        pos.x = 7; pos.y = -1.8f;
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
