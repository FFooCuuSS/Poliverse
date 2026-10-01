using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCard3_4remake : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;
    public Minigame_3_4_Remake minigame3_4;

    public GameObject cardPrefab;

    // 생성된 카드들의 부모가 될 오브젝트
    public GameObject cardParent;

    public Transform[] cardPosition;

    // 생성된 카드들을 저장하는 리스트
    List<GameObject> spawnedCards = new List<GameObject>();

    public GameObject suspiciousFace;
    public GameObject normalFace;

    bool isCardSet = false;
    bool isCardMoving = false;

    int moveCardTurn = 0;
    int cardPosX = -4;

    // 카드 이동 시간 (1박 = 0.6초)
    float moveDuration = 0.6f;
    float moveStartTime;

    Vector3 startPos;
    Vector3 targetPos;

    // 카드가 확대될 크기
    float cardPopSize = 1.2f;

    bool isPoped = false;
    bool moveCard = false;
    bool minigame3_4_started = false;

    void Start()
    {
        // Null 예외 방지용 안전 장치
        if (normalFace != null)
        {
            var sr = normalFace.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
        }

        // 미니게임 참조 자동 찾기 (인스펙터 연결 안 되어있을 때 대비)
        if (minigame3_4 == null)
        {
            minigame3_4 = FindObjectOfType<Minigame_3_4_Remake>();
        }

        // 카드 생성
        SpawnCard();
    }

    void Update()
    {
        // 아직 움직여야 할 카드가 남아있는 경우
        if (isCardSet && moveCardTurn < spawnedCards.Count)
        {
            // 확대 연출이 끝났으면 카드 이동
            if (moveCard)
            {
                MoveCard(moveCardTurn);
            }
            // 아직 확대 연출을 하지 않았다면 확대
            else if (!isPoped)
            {
                PopCard();
            }
        }
        else if (!minigame3_4_started)
        {
            minigame3_4_started = true;

            if (minigame3_4 != null)
            {
                minigame3_4.StartGame();
            }
            else
            {
                Debug.LogError("[DrawCard3_4remake] minigame3_4 참조가 누락되었습니다!");
            }
        }
    }

    void PopCard()
    {
        isPoped = true;

        if (moveCardTurn < spawnedCards.Count && spawnedCards[moveCardTurn] != null)
        {
            GameObject card = spawnedCards[moveCardTurn];
            StartCoroutine(cardReload(card));
        }
    }

    void SpawnCard()
    {
        Debug.Log("SpawnCards() 시작");

        foreach (var card in spawnedCards)
        {
            if (card != null) Destroy(card);
        }

        spawnedCards.Clear();

        Vector3 pos = transform.position;
        pos.x = 7f;
        pos.y = -1.8f;

        int trapIndex = Random.Range(0, 5);

        for (int i = 0; i < 5; i++)
        {
            GameObject card = Instantiate(
                cardPrefab,
                pos,
                Quaternion.identity,
                cardParent != null ? cardParent.transform : null
            );

            CardColor cardComponent = card.GetComponent<CardColor>();

            if (cardComponent == null)
            {
                Debug.LogError("Card 프리팹에 CardColor 스크립트가 없습니다!");
                continue;
            }

            if (i == trapIndex)
            {
                cardComponent.IsSetTrap();
            }
            else
            {
                cardComponent.IsNotTrap();
            }

            spawnedCards.Add(card);
        }

        isCardSet = true;
    }

    void MoveCard(int cardNum)
    {
        if (cardNum >= spawnedCards.Count || spawnedCards[cardNum] == null) return;

        GameObject card = spawnedCards[cardNum];

        if (!isCardMoving)
        {
            // 오디오 매니저 예외 방지 처리
            if (clickSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            {
                GameRoot.Instance.Audio.PlaySfx(clickSound);
            }

            isCardMoving = true;
            startPos = card.transform.position;
            targetPos = new Vector3(cardPosX, -1.5f, 0f);
            moveStartTime = Time.time;

            CardColor colorComp = card.GetComponent<CardColor>();
            if (colorComp != null && colorComp.isTrapCard)
            {
                ChangeSuspicious();
            }
            else
            {
                isNotSuspicious();
            }
        }

        float t = (Time.time - moveStartTime) / moveDuration;
        card.transform.position = Vector3.Lerp(startPos, targetPos, t);

        if (t >= 1f)
        {
            card.transform.position = targetPos;
            isCardMoving = false;
            moveCard = false;

            cardPosX += 2;
            moveCardTurn++;

            isNotSuspicious();
            isPoped = false;
        }
    }

    IEnumerator cardReload(GameObject card)
    {
        if (card == null) yield break;

        Vector3 originalScale = card.transform.localScale;
        Vector3 scale = originalScale;

        scale.x *= cardPopSize;
        scale.y *= cardPopSize;

        card.transform.localScale = scale;

        yield return new WaitForSeconds(0.6f);

        if (card != null)
        {
            card.transform.localScale = originalScale;
        }

        moveCard = true;
    }

    void isNotSuspicious()
    {
        if (suspiciousFace != null)
        {
            var sr = suspiciousFace.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }

        if (normalFace != null)
        {
            var sr = normalFace.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
        }
    }

    void ChangeSuspicious()
    {
        if (suspiciousFace != null)
        {
            var sr = suspiciousFace.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
        }

        if (normalFace != null)
        {
            var sr = normalFace.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }
}