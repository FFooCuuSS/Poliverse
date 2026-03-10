using System.Collections.Generic;
using UnityEngine;

public class ProhibitedItemSpawner1_7 : MonoBehaviour
{
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
