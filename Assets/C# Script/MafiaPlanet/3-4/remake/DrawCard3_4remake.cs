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

    // 카드 이동 시간
    // 1박 = 0.6초
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
        // 처음에는 일반 표정 활성화
        normalFace.GetComponent<SpriteRenderer>().enabled = true;

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
            // 카드 5장의 배치가 전부 끝난 뒤
            // 실제 미니게임 시작
            minigame3_4_started = true;

            minigame3_4.StartGame();
        }
    }


    void PopCard()
    {
        // 같은 카드에서 여러 번 실행되는 것을 방지
        isPoped = true;

        // 현재 차례의 카드 가져오기
        GameObject card = spawnedCards[moveCardTurn];

        // 카드 확대 연출 시작
        StartCoroutine(cardReload(card));
    }


    void SpawnCard()
    {
        Debug.Log("SpawnCards() 시작");

        // 기존에 생성된 카드가 있다면 삭제
        foreach (var card in spawnedCards)
        {
            Destroy(card);
        }

        // 리스트 초기화
        spawnedCards.Clear();

        // 카드 생성 위치
        Vector3 pos = transform.position;

        pos.x = 7f;
        pos.y = -1.8f;

        // 0~4 중 수상한 카드 하나를 랜덤 선택
        int trapIndex = Random.Range(0, 5);

        // 카드 총 5장 생성
        for (int i = 0; i < 5; i++)
        {
            // 카드를 생성하면서 동시에 cardParent의 자식으로 설정
            GameObject card = Instantiate(
                cardPrefab,
                pos,
                Quaternion.identity,
                cardParent.transform
            );

            // CardColor 컴포넌트 가져오기
            CardColor cardComponent =
                card.GetComponent<CardColor>();

            // CardColor가 없는 경우 오류 출력
            if (cardComponent == null)
            {
                Debug.LogError(
                    "Card 프리팹에 CardColor 스크립트가 없습니다!"
                );

                continue;
            }

            // 랜덤으로 선택된 한 장은 수상한 카드
            if (i == trapIndex)
            {
                cardComponent.IsSetTrap();
            }
            else
            {
                // 나머지는 일반 카드
                cardComponent.IsNotTrap();
            }

            // 생성된 카드를 리스트에 추가
            spawnedCards.Add(card);
        }

        // 카드 생성 완료
        isCardSet = true;
    }


    void MoveCard(int cardNum)
    {
        // 현재 이동시킬 카드
        GameObject card = spawnedCards[cardNum];

        // 아직 이동을 시작하지 않은 상태
        if (!isCardMoving)
        {
            if (clickSound != null)
            {
                GameRoot.Instance.Audio.PlaySfx(clickSound);
            }


            isCardMoving = true;

            // 현재 위치 저장
            startPos = card.transform.position;

            // 카드가 이동할 목표 위치
            targetPos =
                new Vector3(cardPosX, -1.5f, 0f);

            // 이동 시작 시간 저장
            moveStartTime = Time.time;

            // 현재 카드가 수상한 카드인지 확인
            if (card.GetComponent<CardColor>().isTrapCard)
            {
                ChangeSuspicious();
            }
            else
            {
                isNotSuspicious();
            }
        }

        // 이동 진행률 계산
        // 0.6초 동안 0 -> 1로 증가
        float t =
            (Time.time - moveStartTime) / moveDuration;

        // 카드 이동
        card.transform.position =
            Vector3.Lerp(startPos, targetPos, t);

        // 이동 완료
        if (t >= 1f)
        {
            // 정확한 최종 위치로 설정
            card.transform.position = targetPos;

            isCardMoving = false;
            moveCard = false;

            // 다음 카드는 오른쪽으로 2만큼 떨어진 위치
            cardPosX += 2;

            // 다음 카드로 넘어감
            moveCardTurn++;

            // 표정을 다시 일반 표정으로 변경
            isNotSuspicious();

            // 다음 카드 Pop 가능하게 설정
            isPoped = false;
        }
    }


    IEnumerator cardReload(GameObject card)
    {
        // 카드의 원래 크기 저장
        Vector3 originalScale =
            card.transform.localScale;

        // 확대될 크기 계산
        Vector3 scale = originalScale;

        scale.x *= cardPopSize;
        scale.y *= cardPopSize;

        // 카드 확대
        card.transform.localScale = scale;

        // 1박 = 0.6초 동안 확대 상태 유지
        yield return new WaitForSeconds(0.6f);

        // 원래 크기로 복구
        card.transform.localScale = originalScale;

        // 카드 이동 시작
        moveCard = true;
    }


    void isNotSuspicious()
    {
        // 수상한 표정 OFF
        suspiciousFace
            .GetComponent<SpriteRenderer>()
            .enabled = false;

        // 일반 표정 ON
        normalFace
            .GetComponent<SpriteRenderer>()
            .enabled = true;
    }


    void ChangeSuspicious()
    {
        // 수상한 표정 ON
        suspiciousFace
            .GetComponent<SpriteRenderer>()
            .enabled = true;

        // 일반 표정 OFF
        normalFace
            .GetComponent<SpriteRenderer>()
            .enabled = false;
    }
}