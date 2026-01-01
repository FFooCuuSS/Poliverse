using System.Collections.Generic;
using UnityEngine;

public class ProhibitedItemManager1_7 : MonoBehaviour
{
    public List<Sprite> prohibitedSprites;
    public List<Sprite> prohibitedMarkSprites;
    public SpriteRenderer prohibitedMarkRenderer;
    public ProhibitedItemSpawner1_7 itemSpawner;
    public GameManager1_7 gameManager;

    private int currentTargetIndex = -1;

    public void SpawnMarkAndItems(GameObject prisoner)
    {
        currentTargetIndex = Random.Range(0, prohibitedMarkSprites.Count);
        prohibitedMarkRenderer.sprite = prohibitedMarkSprites[currentTargetIndex];

        // 죄수에게 아이템 생성 요청
        itemSpawner.SpawnItemsForPrisoner(prisoner, currentTargetIndex);
    }

    public int GetCurrentTargetIndex()
    {
        return currentTargetIndex;
    }

    public void RegisterCollision(Sprite collidedSprite)
    {
        int index = prohibitedSprites.IndexOf(collidedSprite);
        if (index != -1)
        {
            gameManager.SendRhythmInput();
        }

        if (index == currentTargetIndex)
        {
            gameManager.SendRhythmInput();
        }
        else
        {
            //gameManager.NotifyMiss();
        }
    }
}
