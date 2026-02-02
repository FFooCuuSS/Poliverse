using System.Collections.Generic;
using UnityEngine;

public class ProhibitedItemManager1_7 : MonoBehaviour
{
    //public List<Sprite> prohibitedSprites;
    //public List<Sprite> prohibitedMarkSprites;
    //public SpriteRenderer prohibitedMarkRenderer;
    //public ProhibitedItemSpawner1_7 itemSpawner;
    //public GameManager1_7 gameManager;

    //private int currentTargetIndex = -1;

    //public void SpawnMarkAndItems(GameObject prisoner)
    //{
    //    // 중요: 두 리스트 중 개수가 적은 쪽을 기준으로 랜덤 범위를 정합니다.
    //    int maxCount = Mathf.Min(prohibitedMarkSprites.Count, itemSpawner.prohibitedItemPrefabs.Count);

    //    if (maxCount == 0)
    //    {
    //        Debug.LogError("아이템 프리팹이나 마크 스프라이트 리스트가 비어있습니다!");
    //        return;
    //    }

    //    currentTargetIndex = Random.Range(0, maxCount);
    //    prohibitedMarkRenderer.sprite = prohibitedMarkSprites[currentTargetIndex];

    //    // 죄수에게 아이템 생성 요청
    //    //itemSpawner.SpawnItemsForPrisoner(prisoner, currentTargetIndex);
    //}

    //public int GetCurrentTargetIndex()
    //{
    //    return currentTargetIndex;
    //}

    //public void RegisterCollision(GameObject collidedItem)
    //{
    //    PrisonerController1_7 prisoner = FindObjectOfType<PrisonerController1_7>();
    //    if (prisoner == null) return;

    //    GameObject currentItem = prisoner.GetProhibitedItem();
    //    if (currentItem == null)
    //    {
    //        Debug.Log("범인이 금지물품을 들고 있지 않음");
    //        return;
    //    }

    //    if (collidedItem == currentItem)
    //    {
    //        Debug.Log("정답 금지물품!");
    //        gameManager.SendRhythmInput();
    //    }
    //    else
    //    {
    //        Debug.Log("다른 물품");
    //    }
    //}

    public GameObject CurrentItem { get; private set; }

    public void ActivateRandomItem(GameObject spawnedPrisoner)
    {
        Transform prohibitedItems =
            spawnedPrisoner.transform.Find("ProhibitedItems");

        if (prohibitedItems == null)
        {
            Debug.LogError("ProhibitedItems 없음");
            return;
        }

        // 이전 아이템 비활성화
        for (int i = 0; i < prohibitedItems.childCount; i++)
        {
            prohibitedItems.GetChild(i).gameObject.SetActive(false);
        }

        int index = Random.Range(0, prohibitedItems.childCount);
        CurrentItem = prohibitedItems.GetChild(index).gameObject;
        CurrentItem.SetActive(true);

        // 물리 초기화 (들고 있는 상태)
        Rigidbody2D rb = CurrentItem.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = CurrentItem.AddComponent<Rigidbody2D>();

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.simulated = false;

        PrisonerController1_7 controller =
            spawnedPrisoner.GetComponent<PrisonerController1_7>();

        if (controller == null)
        {
            Debug.LogError("PrisonerController1_7 없음");
            return;
        }

        controller.SetProhibitedItem(CurrentItem);
    }

}
