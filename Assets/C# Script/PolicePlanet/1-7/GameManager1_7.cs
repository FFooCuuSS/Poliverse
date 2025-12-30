using UnityEngine;

public class GameManager1_7 : MonoBehaviour
{
    public static GameManager1_7 instance;

    public PrisonerSpawner1_7 prisonerSpawner;
    public ProhibitedItemManager1_7 prohibitedItemManager;
    public GameObject stage_1_7;
    private Minigame_1_7 minigame_1_7;

    public int successCount = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        minigame_1_7 = stage_1_7.GetComponent<Minigame_1_7>();
        SpawnPrisonerAndItems();
    }

    public void SpawnPrisonerAndItems()
    {
        GameObject prisoner = prisonerSpawner.SpawnRandomPrisoner();
        prohibitedItemManager.SpawnMarkAndItems(prisoner);
    }

    public void IncreaseSuccessCount()
    {
        successCount++;
        Debug.Log("성공 카운트: " + successCount);

        //if (successCount >= 1)
        //{
        //    minigame_1_7.Success();
        //}
    }

    public void SendRhythmInput()
    {
        minigame_1_7.OnPlayerInput();
    }

    public void NotifyMiss()
    {
        minigame_1_7.OnMiss();
    }
}
