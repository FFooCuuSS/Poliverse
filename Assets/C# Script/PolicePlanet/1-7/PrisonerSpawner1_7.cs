using UnityEngine;

public class PrisonerSpawner1_7 : MonoBehaviour
{

    public GameObject[] prisonerPrefabs;
    public Transform spawnPoint;

    [SerializeField] private ProhibitedItemManager1_7 itemManager;

    public GameObject SpawnRandomPrisoner()
    {
        int randomIndex = Random.Range(0, prisonerPrefabs.Length);

        GameObject spawnedPrisoner = Instantiate(
            prisonerPrefabs[randomIndex],
            spawnPoint.position,
            Quaternion.identity
        );

        // 아이템 활성화
        itemManager.ActivateRandomItem(spawnedPrisoner);

        return spawnedPrisoner;
    }
}
