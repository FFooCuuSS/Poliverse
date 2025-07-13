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
            Debug.LogError(" �߸��� �ε��� ����");
            return;
        }

        //// 1) ���� ��ũ ���� (�˼� �Ӹ� ���� ǥ��)
        //GameObject markObj = new GameObject("ProhibitedMark");
        //SpriteRenderer markSr = markObj.AddComponent<SpriteRenderer>();
        //markSr.sprite = prohibitedMarkSprites[index];
        //markObj.transform.SetParent(prisoner.transform);
        //markObj.transform.localPosition = new Vector3(0, 1.2f, 0);

        // 2) �˼� ���� "ProhibitedItems" �������� item ��ġ ã��
        string itemName = prohibitedItemPrefabs[index].name.Replace("(Clone)", "").Trim();

        Transform prohibitedItemsParent = prisoner.transform.Find("ProhibitedItems");
        if (prohibitedItemsParent == null)
        {
            Debug.LogWarning(" 'ProhibitedItems' ������Ʈ�� ã�� �� �����ϴ�.");
            return;
        }

        Transform targetPoint = prohibitedItemsParent.Find(itemName);
        if (targetPoint == null)
        {
            Debug.LogWarning($" '{itemName}' �̸��� �ڽ��� 'ProhibitedItems' �ȿ� �����ϴ�.");
            return;
        }

        // 3) ������ ����
        GameObject prohibitedItem = Instantiate(prohibitedItemPrefabs[index], prisoner.transform);
        prohibitedItem.transform.localPosition = targetPoint.localPosition;
        prohibitedItem.tag = "Item";

        // 4) �˼� ��Ʈ�ѷ��� ���
        PrisonerController1_7 controller = prisoner.GetComponent<PrisonerController1_7>();
        if (controller != null)
        {
            controller.SetProhibitedItem(prohibitedItem);
        }
    }


}
