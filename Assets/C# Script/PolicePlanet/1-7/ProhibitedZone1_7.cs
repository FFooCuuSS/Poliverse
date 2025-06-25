using System.Collections.Generic;
using UnityEngine;

public class ProhibitedZone1_7 : MonoBehaviour
{
    public ProhibitedItemManager1_7 manager;

    private List<GameObject> itemsInZone = new List<GameObject>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item") && !itemsInZone.Contains(other.gameObject))
        {
            itemsInZone.Add(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            Debug.Log($"Item Entered Zone: {other.name}");
            if (!itemsInZone.Contains(other.gameObject))
                itemsInZone.Add(other.gameObject);
        }
    }

    void Update()
    {
        if (manager == null || manager.prefabCounts == null) return;

        // 카운트 리셋
        int[] currentCounts = new int[manager.prohibitedSprites.Count];

        foreach (GameObject item in itemsInZone)
        {
            if (item == null) continue;

            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            int index = manager.prohibitedSprites.IndexOf(sr.sprite);
            if (index >= 0 && index < currentCounts.Length)
            {
                currentCounts[index]++;
            }
        }

        // Update 된 값으로 체크 (지속 체크 구조)
        manager.UpdateZoneCount(currentCounts);
        
        
    }
}
