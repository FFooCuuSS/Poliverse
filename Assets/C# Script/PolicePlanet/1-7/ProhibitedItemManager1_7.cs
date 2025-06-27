using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProhibitedItemManager1_7 : MonoBehaviour
{
    public GameObject stage_1_7;
    private Minigame_1_7 minigame_1_7;

    [Header("금지 물품 스프라이트 리스트")]
    [SerializeField] public List<Sprite> prohibitedSprites;

    [Header("금지 마크 스프라이트 리스트")]
    [SerializeField] private List<Sprite> prohibitedMarkSprites;

    [Header("충돌 카운트 리스트")]
    [SerializeField] private List<int> collectedCounts;

    [Header("금지 마크 오브젝트")]
    [SerializeField] private SpriteRenderer prohibitedMarkRenderer;

    [HideInInspector] public int targetIndex = -1;

    [HideInInspector] public int[] prefabCounts; // 각 스프라이트별 프리팹 생성 개수

    void Start()
    {
        collectedCounts = new List<int>(new int[prohibitedSprites.Count]);
        minigame_1_7 = stage_1_7.GetComponent<Minigame_1_7>();

        //// 금지 마크를 랜덤 선택
        //targetIndex = Random.Range(0, prohibitedMarkSprites.Count);
        //prohibitedMarkRenderer.sprite = prohibitedMarkSprites[targetIndex];
    }

    // 충돌 시 호출될 함수
    public void RegisterCollision(Sprite collidedSprite)
    {
        int index = prohibitedSprites.IndexOf(collidedSprite);
        if (index != -1)
        {
            collectedCounts[index]++;
            
            Debug.Log("Count++");
        }
    }

    public void SetPrefabCounts(int[] counts)
    {
        prefabCounts = counts;

        // 금지 마크로 선택 가능한 스프라이트 인덱스 필터링 (1개 이상 생성된 것만)
        List<int> validIndices = new List<int>();
        for (int i = 0; i < counts.Length; i++)
        {
            if (counts[i] > 0)
            {
                validIndices.Add(i);
            }
        }

        if (validIndices.Count == 0)
        {
            Debug.LogWarning("생성된 프리팹 중 금지 물품으로 사용할 수 있는 스프라이트가 없습니다.");
            return;
        }

        // 유효한 인덱스 중에서 금지 마크로 랜덤 선택
        targetIndex = validIndices[Random.Range(0, validIndices.Count)];
        prohibitedMarkRenderer.sprite = prohibitedMarkSprites[targetIndex];
    }

    public void UpdateZoneCount(int[] currentZoneCounts)
    {
        if (prefabCounts == null || targetIndex < 0 || targetIndex >= currentZoneCounts.Length)
            return;

        if (currentZoneCounts[targetIndex] == prefabCounts[targetIndex])
        {
            minigame_1_7.Success();
        }
    }
}
