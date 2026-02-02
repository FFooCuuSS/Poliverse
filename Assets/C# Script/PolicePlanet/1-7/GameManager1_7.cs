using UnityEngine;

public class GameManager1_7 : MonoBehaviour
{
    public static GameManager1_7 instance;

    private PrisonerController1_7 currentPrisoner;

    public PrisonerSpawner1_7 prisonerSpawner;
    public GameObject stage_1_7;

    private Minigame_1_7 minigame_1_7;

    public int successCount = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        minigame_1_7 = stage_1_7.GetComponent<Minigame_1_7>();

        SpawnPrisonerAndItems();
    }

    public void SpawnPrisonerAndItems()
    {
        GameObject prisonerObj = prisonerSpawner.SpawnRandomPrisoner();

        if (prisonerObj == null)
        {
            Debug.LogError("Prisoner 생성 실패");
            return;
        }

        currentPrisoner = prisonerObj.GetComponent<PrisonerController1_7>();

        if (currentPrisoner == null)
        {
            Debug.LogError("PrisonerController1_7 없음");
        }
    }

    public void IncreaseSuccessCount()
    {
        successCount++;
        Debug.Log("성공 카운트: " + successCount);
    }

    public void SendRhythmInput()
    {
        minigame_1_7.OnPlayerInput();
    }

    public void OnMinigameSuccess(Transform basket)
    {
        if (currentPrisoner != null)
        {
            Debug.Log("성공! 현재 죄수에게 물건을 던지라고 명령합니다.");
            currentPrisoner.DropToBasket(basket);
        }
        else
        {
            Debug.LogError("현재 죄수 참조 없음");
        }
    }
}
