using System.Collections.Generic;
using UnityEngine;

public class ProhibitedItemManager1_7 : MonoBehaviour
{
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

        CurrentItem.transform.SetParent(spawnedPrisoner.transform, true);

        // 물리 초기화
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
