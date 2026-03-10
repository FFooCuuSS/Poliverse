using UnityEngine;

public class PrisonerSpawner1_7 : MonoBehaviour
{
    public GameObject[] prisonerPrefabs;
    public Transform spawnPoint;

    [SerializeField] private ProhibitedItemManager1_7 itemManager;

    public GameObject SpawnRandomPrisoner(Transform parent = null)
    {
        int randomIndex = Random.Range(0, prisonerPrefabs.Length);

        GameObject spawnedPrisoner = Instantiate(
            prisonerPrefabs[randomIndex],
            spawnPoint.position,
            Quaternion.identity,
            parent
        );

        itemManager.ActivateRandomItem(spawnedPrisoner);

        return spawnedPrisoner;
    }
}
