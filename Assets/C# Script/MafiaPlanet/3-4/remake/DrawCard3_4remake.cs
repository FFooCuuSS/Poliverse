using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCard3_4remake : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform[] cardPosition;
    List<GameObject> spawnedCards = new List<GameObject>();
    public GameObject suspiciousFace;
    public GameObject normalFace;

    bool isCardSet = false;
    bool isCardMoving = false;

    int moveCardTurn = 0;
    int cardPosX = -4;

    float moveDuration = 0.5f;
    float moveStartTime;
    Vector3 startPos;
    Vector3 targetPos;

    float cardPopSize = 1.2f;
    bool isPoped = false;
    bool moveCard = false;
    // Start is called before the first frame update
    void Start()
    {
        normalFace.GetComponent<SpriteRenderer>().enabled = true;
        SpawnCard();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (isCardSet && moveCardTurn < spawnedCards.Count)
        {
            if(moveCard) MoveCard(moveCardTurn);
            else if(!isPoped) PopCard();
        }
        else
        {
            GetComponent<ChooseCardRemake>().start = true;
        }
    }
    void PopCard()
    {
        isPoped = true;
        GameObject card = spawnedCards[moveCardTurn];

        StartCoroutine(cardReload(card));
    }
    void SpawnCard()
    {
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
            var cardComponent = card.GetComponent<CardColor>();//이건 추후에 수상한 카드 이미지로 대체

            if (i == trapIndex) cardComponent.IsSetTrap();//수상한카드로 설정
            else cardComponent.IsNotTrap();

            if (cardComponent == null)
            {
                Debug.LogError("Card 프리팹에 Card 스크립트가 없습니다!");
            }
            spawnedCards.Add(card);//list에 설정된 카드 추가

        }
        isCardSet = true;
    }
    void MoveCard(int cardNum)
    {
        
        GameObject card = spawnedCards[cardNum];

        if (!isCardMoving)
        {
            isCardMoving = true;

            startPos = card.transform.position;
            targetPos = new Vector3(cardPosX, -1.5f, 0);
            moveStartTime = Time.time;

            if (card.GetComponent<CardColor>().isTrapCard)
            {
                ChangeSuspicious();
            }
            else isNotSuspicious();
        }

        float t = (Time.time - moveStartTime)/moveDuration;

        card.transform.position = Vector3.Lerp(startPos, targetPos, t);
        //t는 계속 업데이트되는 현재 진척도라고 보면 됨

        if (t > 1f) 
        { 
            card.transform.position = targetPos;
            isCardMoving=false;
            moveCard = false;
            cardPosX += 2;
            moveCardTurn++;
            isNotSuspicious();
            isPoped = false;
        }
    }
    IEnumerator cardReload(GameObject card)
    {
        // 원래 스케일 저장
        Vector3 originalScale = card.transform.localScale;

        // 확대
        Vector3 scale = originalScale;
        scale.x *= cardPopSize;
        scale.y *= cardPopSize;
        card.transform.localScale = scale;

        // 0.5초 대기
        yield return new WaitForSeconds(0.5f);

        // 원래대로 복구
        card.transform.localScale = originalScale;
        moveCard=true;
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
}
