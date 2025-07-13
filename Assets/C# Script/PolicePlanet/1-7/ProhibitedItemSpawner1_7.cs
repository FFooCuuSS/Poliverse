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
            Debug.LogError(" 잘못된 인덱스 접근");
            return;
        }

        //// 1) 금지 마크 생성 (죄수 머리 위에 표시)
        //GameObject markObj = new GameObject("ProhibitedMark");
        //SpriteRenderer markSr = markObj.AddComponent<SpriteRenderer>();
        //markSr.sprite = prohibitedMarkSprites[index];
        //markObj.transform.SetParent(prisoner.transform);
        //markObj.transform.localPosition = new Vector3(0, 1.2f, 0);

        // 2) 죄수 안의 "ProhibitedItems" 하위에서 item 위치 찾기
        string itemName = prohibitedItemPrefabs[index].name.Replace("(Clone)", "").Trim();

        Transform prohibitedItemsParent = prisoner.transform.Find("ProhibitedItems");
        if (prohibitedItemsParent == null)
        {
            Debug.LogWarning(" 'ProhibitedItems' 오브젝트를 찾을 수 없습니다.");
            return;
        }

        Transform targetPoint = prohibitedItemsParent.Find(itemName);
        if (targetPoint == null)
        {
            Debug.LogWarning($" '{itemName}' 이름의 자식이 'ProhibitedItems' 안에 없습니다.");
            return;
        }

        // 3) 아이템 생성
        GameObject prohibitedItem = Instantiate(prohibitedItemPrefabs[index], prisoner.transform);
        prohibitedItem.transform.localPosition = targetPoint.localPosition;
        prohibitedItem.tag = "Item";

        // 4) 죄수 컨트롤러에 등록
        PrisonerController1_7 controller = prisoner.GetComponent<PrisonerController1_7>();
        if (controller != null)
        {
            controller.SetProhibitedItem(prohibitedItem);
        }
    }


}
