using UnityEngine;

public class PrisonerSpawner1_7 : MonoBehaviour
{
    [SerializeField] private GameObject[] prisonerPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private ProhibitedItemManager1_7 itemManager;

    public GameObject SpawnRandomPrisoner(Transform parent = null)
    {
        if (prisonerPrefabs == null || prisonerPrefabs.Length == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, prisonerPrefabs.Length);

        GameObject spawnedPrisoner = Instantiate(
            prisonerPrefabs[randomIndex],
            spawnPoint.position,
            Quaternion.identity,
            parent
        );

        if (itemManager != null)
            itemManager.ActivateRandomItem(spawnedPrisoner);

        return spawnedPrisoner;
    }
}