using UnityEngine;

public class PrisonerSpawner1_7 : MonoBehaviour
{
    
    public GameObject[] prisonerPrefabs;
    public Transform spawnPoint;
    public bool isSpawning = true;

    public GameObject SpawnRandomPrisoner()
    {
        int randomIndex = Random.Range(0, prisonerPrefabs.Length);

        GameObject prisoner = Instantiate(prisonerPrefabs[randomIndex], spawnPoint.position, Quaternion.identity, this.transform);
        return prisoner;  // 반환 추가
    }

    /*
    private void Update()
    {
        if(isSpawning)
        {
            GameManager1_7.instance.SpawnPrisonerAndItems();
            isSpawning = false;
        }
        if (transform.childCount == 0)
        {
            isSpawning = true;
            Debug.Log("자식이 없으므로 isSpawning을 true로 설정했습니다.");
        }
    }
    */
}
