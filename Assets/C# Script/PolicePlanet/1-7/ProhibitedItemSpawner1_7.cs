using System.Collections.Generic;
using UnityEngine;

public class ProhibitedItemSpawner1_7 : MonoBehaviour
{
    //public List<GameObject> prohibitedItemPrefabs;
    //public List<Sprite> prohibitedMarkSprites;
    //public Transform spawnParent;

    //public void SpawnItemsForPrisoner(GameObject prisoner, int index)
    //{
    //    if (index < 0 || index >= prohibitedItemPrefabs.Count)
    //    {
    //        Debug.LogError("잘못된 인덱스 접근");
    //        return;
    //    }

    //    Transform prohibitedItemsParent = prisoner.transform.Find("ProhibitedItems");
    //    if (prohibitedItemsParent == null)
    //    {
    //        Debug.LogError("'ProhibitedItems' 오브젝트가 없습니다!");
    //        return;
    //    }

    //    // 생성
    //    GameObject prohibitedItem = Instantiate(
    //        prohibitedItemPrefabs[index],
    //        prohibitedItemsParent
    //    );

    //    prohibitedItem.transform.localPosition = Vector3.zero;
    //    prohibitedItem.transform.localRotation = Quaternion.identity;
    //    prohibitedItem.tag = "Item";

    //    // Rigidbody2D 설정
    //    Rigidbody2D rb = prohibitedItem.GetComponent<Rigidbody2D>();
    //    if (rb == null) rb = prohibitedItem.AddComponent<Rigidbody2D>();

    //    rb.gravityScale = 0f;
    //    rb.simulated = false;

    //    // 범인 컨트롤러에 전달
    //    PrisonerController1_7 controller = prisoner.GetComponent<PrisonerController1_7>();
    //    if (controller == null)
    //    {
    //        Debug.LogError("PrisonerController1_7가 없습니다!");
    //        return;
    //    }

    //    controller.SetProhibitedItem(prohibitedItem);

    //    Debug.Log($"금지물품 생성 성공: {prohibitedItem.name}");

    //}

    

    [SerializeField] private ProhibitedItemManager1_7 itemManager;
    [SerializeField] private Transform targetPosition;

    public void MoveItem()
    {
        if (itemManager.CurrentItem == null)
        {
            Debug.LogWarning("활성화된 아이템이 없습니다.");
            return;
        }

        itemManager.CurrentItem.transform.position = targetPosition.position;
    }

}
