using System.Collections.Generic;
using UnityEngine;

// 3-4 카드 돌리기. 노래 시간으로 직접 계산한다.
// 카드 k: 1.2k초에 확대(0.6초) → 1.2k+0.6초부터 0.6초 동안 자리로 이동 (x = -4 + 2k)
// 함정 카드가 이동하는 동안 상대 표정이 바뀐다.
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
    [SerializeField] private Vector3 deckPos = new Vector3(7f, -1.8f, 0f);
    [SerializeField] private float firstCardX = -4f;
    [SerializeField] private float cardStepX = 2f;
    [SerializeField] private float cardY = -1.5f;
    [SerializeField] private float cardPopSize = 1.2f;

    private readonly List<GameObject> spawnedCards = new List<GameObject>();
    private readonly List<Vector3> baseScales = new List<Vector3>();
    private readonly List<int> sfxPlayed = new List<int>();

    public int TrapIndex { get; private set; } = -1;

    private void Start()
    {
        if (minigame3_4 == null)
            minigame3_4 = GetComponentInParent<Minigame_3_4_Remake>();

        SetFace(false);
        SpawnCard();
    }

    private void SpawnCard()
    {
        foreach (var c in spawnedCards)
            if (c != null) Destroy(c);

        spawnedCards.Clear();
        baseScales.Clear();
        sfxPlayed.Clear();

        TrapIndex = Random.Range(0, 5);

        for (int i = 0; i < 5; i++)
        {
            GameObject card = Instantiate(
                cardPrefab,
                deckPos,
                Quaternion.identity,
                cardParent != null ? cardParent.transform : null
            );

            CardColor cc = card.GetComponent<CardColor>();

            if (cc != null)
            {
                if (i == TrapIndex) cc.IsSetTrap();
                else cc.IsNotTrap();
            }

            spawnedCards.Add(card);
            baseScales.Add(card.transform.localScale);
        }

        Debug.Log($"[3-4] 함정 카드 = {TrapIndex}번째");
    }

    private void Update()
    {
        if (minigame3_4 == null) return;

        float t = minigame3_4.SongTime;
        bool suspicious = false;

        for (int i = 0; i < spawnedCards.Count; i++)
        {
            GameObject card = spawnedCards[i];
            if (card == null) continue;

            float popStart = firstCardTime + cardInterval * i;
            float moveStart = popStart + popDuration;
            float moveEnd = moveStart + moveDuration;

            Vector3 target = new Vector3(firstCardX + cardStepX * i, cardY, 0f);

            if (t < popStart)
            {
                card.transform.position = deckPos;
                card.transform.localScale = baseScales[i];
            }
            else if (t < moveStart)
            {
                // 확대
                card.transform.position = deckPos;
                card.transform.localScale = new Vector3(
                    baseScales[i].x * cardPopSize,
                    baseScales[i].y * cardPopSize,
                    baseScales[i].z);
            }
            else if (t < moveEnd)
            {
                // 자리로 이동
                if (!sfxPlayed.Contains(i))
                {
                    sfxPlayed.Add(i);
                    if (clickSound != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
                        GameRoot.Instance.Audio.PlaySfx(clickSound);
                }

                card.transform.localScale = baseScales[i];
                float k = (t - moveStart) / moveDuration;
                card.transform.position = Vector3.Lerp(deckPos, target, k);

                if (i == TrapIndex) suspicious = true;
            }
            else
            {
                card.transform.localScale = baseScales[i];
                card.transform.position = target;
            }
        }

        SetFace(suspicious);
    }

    private void SetFace(bool suspicious)
    {
        if (suspiciousFace != null)
        {
            var sr = suspiciousFace.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = suspicious;
        }

        if (normalFace != null)
        {
            var sr = normalFace.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = !suspicious;
        }
    }
}