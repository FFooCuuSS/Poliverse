
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3-4 카드 돌리기.
///
/// 카드 k:
/// 1.2k초에 확대 (0.6초)
/// 이후 0.6초 동안 목표 위치로 이동
///
/// 함정 카드가 이동하는 동안 상대 표정 변경.
///
/// 클릭 판정된 카드는 1.2배 확대된 상태로 유지한다.
/// </summary>
public class DrawCard3_4remake : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;

    public Minigame_3_4_Remake minigame3_4;

    public GameObject cardPrefab;
    public GameObject cardParent;
    public Transform[] cardPosition;

    public GameObject suspiciousFace;
    public GameObject normalFace;

    [Header("타이밍 (노래 시간, 초)")]
    [SerializeField] private float firstCardTime = 0f;
    [SerializeField] private float cardInterval = 1.2f;
    [SerializeField] private float popDuration = 0.6f;
    [SerializeField] private float moveDuration = 0.6f;

    [Header("배치")]
    [SerializeField]
    private Vector3 deckPos =
        new Vector3(7f, -1.8f, 0f);

    [SerializeField] private float firstCardX = -4f;
    [SerializeField] private float cardStepX = 2f;
    [SerializeField] private float cardY = -1.5f;

    [SerializeField] private float cardPopSize = 1.2f;

    // 생성된 카드 관리
    private readonly List<GameObject> spawnedCards =
        new List<GameObject>();

    // 카드 최초 크기 저장
    private readonly List<Vector3> baseScales =
        new List<Vector3>();

    // 효과음 재생 여부
    private readonly List<int> sfxPlayed =
        new List<int>();

    // ★ 클릭으로 확대된 카드 인덱스 저장
    private readonly HashSet<int> clickedCards =
        new HashSet<int>();

    // 함정 카드 인덱스
    public int TrapIndex { get; private set; } = -1;

    private void Start()
    {
        if (minigame3_4 == null)
        {
            minigame3_4 =
                GetComponentInParent<Minigame_3_4_Remake>();
        }

        SetFace(false);

        SpawnCard();
    }

    /// <summary>
    /// 카드 5장을 생성하고 함정 카드를 무작위로 지정한다.
    /// </summary>
    private void SpawnCard()
    {
        // 기존 카드 삭제
        foreach (var c in spawnedCards)
        {
            if (c != null)
            {
                Destroy(c);
            }
        }

        // 카드 정보 초기화
        spawnedCards.Clear();
        baseScales.Clear();
        sfxPlayed.Clear();

        // 클릭 확대 기록 초기화
        clickedCards.Clear();

        // 함정 카드 랜덤 지정
        TrapIndex = Random.Range(0, 5);

        for (int i = 0; i < 5; i++)
        {
            // 카드 생성
            GameObject card = Instantiate(
                cardPrefab,
                deckPos,
                Quaternion.identity,
                cardParent != null
                    ? cardParent.transform
                    : null
            );

            // 카드 함정 여부 설정
            CardColor cc = card.GetComponent<CardColor>();

            if (cc != null)
            {
                if (i == TrapIndex)
                {
                    cc.IsSetTrap();
                }
                else
                {
                    cc.IsNotTrap();
                }
            }

            // 생성한 카드 저장
            spawnedCards.Add(card);

            // 기본 크기 저장
            baseScales.Add(card.transform.localScale);
        }

        Debug.Log($"[3-4] 함정 카드 = {TrapIndex}번째");
    }

    /// <summary>
    /// 클릭한 카드를 1.2배 확대한다.
    ///
    /// 인덱스는 0~4.
    /// 이미 확대된 카드는 다시 확대하지 않는다.
    /// </summary>
    public void ScaleClickedCard(int index)
    {
        // 잘못된 카드 인덱스 방지
        if (index < 0 || index >= spawnedCards.Count)
            return;

        // 이미 확대된 카드라면 중복 처리하지 않음
        if (!clickedCards.Add(index))
            return;

        GameObject card = spawnedCards[index];

        if (card == null)
            return;

        // 최초 크기를 기준으로 1.2배 확대
        card.transform.localScale =
            baseScales[index] * cardPopSize;

        Debug.Log(
            $"[3-4] 카드{index} 클릭 확대 ({cardPopSize}배)");
    }

    private void Update()
    {
        if (minigame3_4 == null)
            return;

        float t = minigame3_4.SongTime;

        bool suspicious = false;

        for (int i = 0; i < spawnedCards.Count; i++)
        {
            GameObject card = spawnedCards[i];

            if (card == null)
                continue;

            // 카드별 애니메이션 시작/종료 시간
            float popStart =
                firstCardTime + cardInterval * i;

            float moveStart =
                popStart + popDuration;

            float moveEnd =
                moveStart + moveDuration;

            // 카드 목표 위치
            Vector3 target = new Vector3(
                firstCardX + cardStepX * i,
                cardY,
                0f
            );

            // ★ 클릭한 카드라면 1.2배 크기 유지
            bool isClicked = clickedCards.Contains(i);

            // 클릭 후 유지할 카드 크기
            Vector3 finalScale =
                isClicked
                    ? baseScales[i] * cardPopSize
                    : baseScales[i];

            if (t < popStart)
            {
                // 카드 등장 전에는 덱 위치 대기
                card.transform.position = deckPos;

                card.transform.localScale = finalScale;
            }
            else if (t < moveStart)
            {
                // 카드 배치 애니메이션의 확대 구간
                card.transform.position = deckPos;

                // 기존 카드 등장 확대 효과 유지
                card.transform.localScale = new Vector3(
                    baseScales[i].x * cardPopSize,
                    baseScales[i].y * cardPopSize,
                    baseScales[i].z
                );
            }
            else if (t < moveEnd)
            {
                // 카드 이동 시작 시 효과음 재생
                if (!sfxPlayed.Contains(i))
                {
                    sfxPlayed.Add(i);

                    if (clickSound != null &&
                        GameRoot.Instance != null &&
                        GameRoot.Instance.Audio != null)
                    {
                        GameRoot.Instance.Audio.PlaySfx(clickSound);
                    }
                }

                // 클릭한 카드는 확대 유지
                card.transform.localScale = finalScale;

                // 덱에서 카드 목표 위치로 이동
                float k = (t - moveStart) / moveDuration;

                card.transform.position =
                    Vector3.Lerp(deckPos, target, k);

                // 함정 카드 이동 중 표정 변경
                if (i == TrapIndex)
                {
                    suspicious = true;
                }
            }
            else
            {
                // 카드 배치 완료
                card.transform.position = target;

                // ★ 클릭한 카드의 확대 상태 유지
                card.transform.localScale = finalScale;
            }
        }

        SetFace(suspicious);
    }

    /// <summary>
    /// 상대 표정 변경.
    /// 함정 카드 이동 중 의심스러운 표정으로 변경한다.
    /// </summary>
    private void SetFace(bool suspicious)
    {
        if (suspiciousFace != null)
        {
            SpriteRenderer sr =
                suspiciousFace.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.enabled = suspicious;
            }
        }

        if (normalFace != null)
        {
            SpriteRenderer sr =
                normalFace.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.enabled = !suspicious;
            }
        }
    }
}
