using System;
using System.Collections.Generic;
using UnityEngine;

public class ProhibitedItemSpawner1_7 : MonoBehaviour
{
    public List<GameObject> prohibitedItemPrefabs;
    public List<Sprite> prohibitedMarkSprites;
    public Transform spawnParent;

    public void SpawnItemsForPrisoner(GameObject prisoner, int index)
    {
        if (index < 0 || index >= prohibitedItemPrefabs.Count || index >= prohibitedMarkSprites.Count)
        {
            Debug.LogError("잘못된 인덱스 접근");
            return;
        }

        string itemName = prohibitedItemPrefabs[index].name.Replace("(Clone)", "").Trim();

        Transform prohibitedItemsParent = prisoner.transform.Find("ProhibitedItems");
        if (prohibitedItemsParent == null)
        {
            Debug.LogWarning("'ProhibitedItems' 오브젝트를 찾을 수 없습니다.");
            return;
        }

        Transform targetPoint = prohibitedItemsParent.Find(itemName);
        if (targetPoint == null)
        {
            Debug.LogWarning($"'{itemName}' 이름의 자식이 'ProhibitedItems' 안에 없습니다.");
            return;
        }

        // 금지 아이템 생성 (범인 하위)
        GameObject prohibitedItem = Instantiate(
            prohibitedItemPrefabs[index],
            targetPoint.position,
            targetPoint.rotation,
            prohibitedItemsParent
        );
        prohibitedItem.transform.localPosition = targetPoint.localPosition;
        prohibitedItem.transform.localRotation = targetPoint.localRotation;
        prohibitedItem.tag = "Item";

        // Rigidbody2D 확인
        Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
        if (rb == null) rb = prohibitedItem.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;  // 중력 OFF
        rb.simulated = false;  // 시뮬레이션 OFF

        // 범인 스크립트에 연결
        PrisonerController1_7 controller = prisoner.GetComponent<PrisonerController1_7>();
        if (controller == null)
        {
            Debug.LogError($"PrisonerController1_7가 {prisoner.name}에 없습니다!");
            return;
        }

        controller.SetProhibitedItem(prohibitedItem);
    }

}
